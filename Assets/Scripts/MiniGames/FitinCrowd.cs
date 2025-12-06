using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class FitInCrowd : MinigameBase
{
    [Header("Prefabs")]
    [SerializeField] List<GameObject> crowdPrefabs;
    [SerializeField] GameObject playerBallPrefab;

    [Header("Layout")]
    [SerializeField] Transform playerStart;
    [SerializeField] int totalUnits = 20;
    [SerializeField] float unitWidth = 4f;
    [SerializeField] float crowdY = 0f;
    [SerializeField] float minGapWorld = 10f;
    [SerializeField] float maxGapWorld = 30f;

    [Header("Movement")]
    [SerializeField] float baseMoveSpeed = 1.5f;
    [SerializeField] float moveRange = 10f;

    [Header("Collision")]
    [SerializeField] LayerMask crowdLayer;
    [SerializeField] float overlapScale = 0.95f;

    [Header("Gaps")]
    [SerializeField] int minGaps = 2;
    [SerializeField] int maxGaps = 4;
    [SerializeField] int safeUnitsAroundPlayer = 1;

    [Header("Ball Visual")]
    [SerializeField] Sprite normalBallSprite;
    [SerializeField] Sprite failBallSprite;
    [SerializeField] float failBounceHeight = 0.3f;
    [SerializeField] float failBounceDuration = 0.1f;

    [Header("Debug")]
    [SerializeField] bool debugOverlap = true;

    Transform crowdRoot;
    Bus_Char ball;

    Vector3 crowdRootStartPos;
    bool crowdFrozen;
    bool isDropping;
    bool pendingNewWall;

    float currentDifficulty;
    float currentMoveSpeed;

    void Awake()
    {
        Physics2D.queriesHitTriggers = true;
        Init(0f);
    }

    public override void Init(float difficulty)
    {
        base.Init(difficulty);

        currentDifficulty = difficulty;
        currentMoveSpeed = baseMoveSpeed + difficulty * 0.4f;

        BuildCrowdWall();
        SpawnBall();
    }

    void BuildCrowdWall()
    {
        if (crowdRoot != null)
            Destroy(crowdRoot.gameObject);

        crowdRoot = new GameObject("CrowdRoot").transform;
        crowdRoot.SetParent(transform, false);
        crowdRoot.position = Vector3.zero;
        crowdRootStartPos = crowdRoot.position;

        float totalWidth = (totalUnits - 1) * unitWidth;
        float startX = -totalWidth * 0.5f;

        int minUnits = Mathf.Max(1, Mathf.RoundToInt(minGapWorld / unitWidth));
        int maxUnits = Mathf.Max(minUnits, Mathf.RoundToInt(maxGapWorld / unitWidth));
        int maxGapUnits = Mathf.Max(1, totalUnits - 1);

        minUnits = Mathf.Clamp(minUnits, 1, maxGapUnits);
        maxUnits = Mathf.Clamp(maxUnits, minUnits, maxGapUnits);

        int[] tiles = new int[totalUnits];
        for (int i = 0; i < totalUnits; i++)
            tiles[i] = 1;

        float playerX = playerStart != null ? playerStart.position.x : 0f;
        int playerIndex = Mathf.RoundToInt((playerX - startX) / unitWidth);
        playerIndex = Mathf.Clamp(playerIndex, 0, totalUnits - 1);

        float d01 = Mathf.Clamp01(currentDifficulty / 5f);
        int maxGapsThisDifficulty = Mathf.RoundToInt(Mathf.Lerp(maxGaps, minGaps, d01));
        int gapCount = Random.Range(minGaps, maxGapsThisDifficulty + 1);
        gapCount = Mathf.Clamp(gapCount, 1, totalUnits);

        for (int g = 0; g < gapCount; g++)
        {
            bool placed = false;

            for (int tries = 0; tries < 40; tries++)
            {
                int gapUnits = Random.Range(minUnits, maxUnits + 1);
                gapUnits = Mathf.Clamp(gapUnits, 1, totalUnits);

                int startIndex = Random.Range(0, totalUnits - gapUnits + 1);

                bool bad = false;
                for (int i = startIndex; i < startIndex + gapUnits; i++)
                {
                    if (tiles[i] == 0)
                    {
                        bad = true;
                        break;
                    }

                    if (Mathf.Abs(i - playerIndex) <= safeUnitsAroundPlayer)
                    {
                        bad = true;
                        break;
                    }
                }

                if (bad) continue;

                for (int i = startIndex; i < startIndex + gapUnits; i++)
                    tiles[i] = 0;

                placed = true;
                break;
            }

            if (!placed && debugOverlap)
                Debug.LogWarning("[FitInCrowd] Failed to place gap " + g);
        }

        bool anyGap = false;
        for (int i = 0; i < totalUnits; i++)
        {
            if (tiles[i] == 0)
            {
                anyGap = true;
                break;
            }
        }

        if (!anyGap)
        {
            int idx = playerIndex < totalUnits / 2 ? totalUnits - 1 : 0;
            tiles[idx] = 0;
        }

        for (int i = 0; i < totalUnits; i++)
        {
            if (tiles[i] == 0) continue;

            float x = startX + i * unitWidth;
            Vector3 pos = new Vector3(x, crowdY, 0f);

            if (crowdPrefabs != null && crowdPrefabs.Count > 0)
            {
                int index = Random.Range(0, crowdPrefabs.Count);
                GameObject prefab = crowdPrefabs[index];
                Instantiate(prefab, pos, Quaternion.identity, crowdRoot);
            }
        }

        crowdFrozen = false;
        isDropping = false;
        pendingNewWall = false;

        if (debugOverlap)
        {
            char[] mask = new char[totalUnits];
            for (int i = 0; i < totalUnits; i++)
                mask[i] = tiles[i] == 0 ? 'G' : '#';
        }
    }

    void SpawnBall()
    {
        if (ball != null)
            Destroy(ball.gameObject);

        GameObject obj = Instantiate(playerBallPrefab, playerStart);
        obj.transform.localPosition = Vector3.zero;
        obj.transform.localRotation = Quaternion.identity;

        ball = obj.GetComponent<Bus_Char>();

        var sr = ball.GetComponent<SpriteRenderer>();
        if (sr != null && normalBallSprite != null)
            sr.sprite = normalBallSprite;
    }

    void Update()
    {
        base.Update();
        if (!running) return;

        if (!isDropping)
            UpdateCrowdMovement();

        if (!isDropping && Input.GetMouseButtonDown(0))
            StartDrop();
    }

    void UpdateCrowdMovement()
    {
        if (crowdRoot == null) return;
        if (crowdFrozen) return;

        float t = Mathf.PingPong(Time.time * currentMoveSpeed, moveRange * 2f) - moveRange;
        Vector3 pos = crowdRootStartPos;
        pos.x += t;
        crowdRoot.position = pos;
    }

    void StartDrop()
    {
        if (ball == null) return;

        isDropping = true;
        crowdFrozen = true;

        float targetY = -2f;

        if (debugOverlap)
            Debug.Log($"[FitInCrowd] StartDrop targetY={targetY}");

        ball.FallTo(targetY, OnBallLanded);
    }

    void OnBallLanded()
    {
        if (!running) return;

        bool hitCrowd = CheckCrowdOverlap();

        if (hitCrowd)
            StartCoroutine(HandleFailAndRetry());
        else
            StartCoroutine(HandleSuccess());
    }

    IEnumerator HandleSuccess()
    {
        yield return new WaitForSeconds(0.1f);
        Win();
    }

    IEnumerator HandleFailAndRetry()
    {
        if (ball != null)
        {
            var sr = ball.GetComponent<SpriteRenderer>();
            if (sr != null && failBallSprite != null)
                sr.sprite = failBallSprite;

            ball.transform.DOKill();

            Vector3 basePos = ball.transform.localPosition;
            ball.transform.DOLocalJump(basePos, failBounceHeight, 1, failBounceDuration)
                .SetEase(Ease.OutQuad);

            yield return new WaitForSeconds(failBounceDuration);

            if (sr != null && normalBallSprite != null)
                sr.sprite = normalBallSprite;
        }

        running = true;
        Retry();
    }

    bool CheckCrowdOverlap()
    {
        if (ball == null)
        {
            if (debugOverlap)
                Debug.LogWarning("[FitInCrowd] CheckCrowdOverlap: ball is null");
            return false;
        }

        Collider2D ballCol = ball.GetComponent<Collider2D>();
        if (ballCol == null)
        {
            if (debugOverlap)
                Debug.LogWarning("[FitInCrowd] CheckCrowdOverlap: ball has no Collider2D");
            return false;
        }

        Bounds b = ballCol.bounds;
        Vector2 center = b.center;
        Vector2 size = b.size * overlapScale;

        if (debugOverlap)
        {
            Debug.Log($"[FitInCrowd] Overlap check center={center}, size={size}, crowdLayer={crowdLayer.value}");

            Vector3 tl = new Vector3(center.x - size.x * 0.5f, center.y + size.y * 0.5f, 0f);
            Vector3 tr = new Vector3(center.x + size.x * 0.5f, center.y + size.y * 0.5f, 0f);
            Vector3 bl = new Vector3(center.x - size.x * 0.5f, center.y - size.y * 0.5f, 0f);
            Vector3 br = new Vector3(center.x + size.x * 0.5f, center.y - size.y * 0.5f, 0f);
            Debug.DrawLine(tl, tr, Color.yellow, 0.5f);
            Debug.DrawLine(tr, br, Color.yellow, 0.5f);
            Debug.DrawLine(br, bl, Color.yellow, 0.5f);
            Debug.DrawLine(bl, tl, Color.yellow, 0.5f);
        }

        Collider2D[] hitsMask = Physics2D.OverlapBoxAll(center, size, 0f, crowdLayer);

        if (debugOverlap)
        {
            Debug.Log($"[FitInCrowd] OverlapBoxAll with mask count={hitsMask.Length}");
            for (int i = 0; i < hitsMask.Length; i++)
            {
                var h = hitsMask[i];
                Debug.Log($"[FitInCrowd]  mask-hit[{i}]={h.name}, layer={h.gameObject.layer}, isTrigger={h.isTrigger}");
            }
        }

        for (int i = 0; i < hitsMask.Length; i++)
        {
            Collider2D h = hitsMask[i];
            if (h == null) continue;
            if (h.gameObject == ballCol.gameObject) continue;
            return true;
        }

        return false;
    }

    void Retry()
    {
        if (!running) return;

        float targetY = playerStart.position.y;
        pendingNewWall = true;
        ball.FallTo(targetY, OnBallReset);
    }

    void OnBallReset()
    {
        if (!running) return;

        isDropping = false;
        crowdFrozen = false;

        if (pendingNewWall)
            BuildCrowdWall();
    }
}
