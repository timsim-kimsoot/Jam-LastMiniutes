using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class MosquitoRhythm : MinigameBase
{
    [Header("Refs")]
    [SerializeField] Camera cam;
    [SerializeField] GameObject mosquitoPrefab;
    [SerializeField] Transform mosquitoContainer;

    [Header("Pattern Settings")]
    [SerializeField] Vector2Int stepLengthRange = new Vector2Int(2, 4);

    [Header("Timing")]
    [SerializeField] float leadIn = 0.5f;
    [SerializeField] float beatInterval = 0.2f;
    [SerializeField] float stepPause = 0.5f;
    [SerializeField] float noteLifetime = 1.1f;

    [Header("screen space")]
    [Range(0f, 0.5f)] public float leftMinXNorm = 0.2f;
    [Range(0f, 0.5f)] public float leftMaxXNorm = 0.45f;
    [Range(0.5f, 1f)] public float rightMinXNorm = 0.7f;
    [Range(0.5f, 1f)] public float rightMaxXNorm = 0.95f;
    [Range(0f, 1f)] public float minYNorm = 0.3f;
    [Range(0f, 1f)] public float maxYNorm = 0.9f;
    [Range(0f, 0.4f)] public float laneInnerVerticalPadding = 0.3f;

    [Header("Spray Cans")]
    public Transform leftCan;
    public Transform rightCan;
    public float idleY = -9f;
    public float hoverY = -5f;
    public float tweenTime = 0.25f;
    Tween leftTween;
    Tween rightTween;

    [Header("Spray")]
    [SerializeField] float sprayCooldown = 0.25f;

    List<string> stepPatterns = new();
    readonly List<Note> activeNotes = new();

    struct Note
    {
        public MosquitoNote mosquito;
        public bool isLeft;
        public float spawnTime;
        public bool hit;
    }

    int currentStepIndex;
    int spawnIndexInStep;
    float nextSpawnTime;

    bool waitingForStepClear;
    bool allStepsSpawned;

    float nextSprayTime;

    public override void Init(float difficulty)
    {
        base.Init(difficulty);

        nextSprayTime = 0f;

        StartRhythmGame(difficulty);
    }

    void StartRhythmGame(float difficulty)
    {
        if (cam == null)
            cam = Camera.main;

        if (mosquitoContainer == null)
        {
            var go = new GameObject("Mosquitoes");
            go.transform.SetParent(transform, false);
            mosquitoContainer = go.transform;
        }

        CleanupNotes();

        GeneratePatternsFromTimeAndDifficulty(difficulty);
        if (stepPatterns.Count == 0)
        {
            Debug.LogError("MosquitoRhythm: No patterns generated.");
            Win();
            return;
        }

        currentStepIndex = 0;
        spawnIndexInStep = 0;
        waitingForStepClear = false;
        allStepsSpawned = false;

        nextSpawnTime = Time.time + leadIn;
    }

    void CleanupNotes()
    {
        foreach (var n in activeNotes)
        {
            if (n.mosquito != null)
                Destroy(n.mosquito.gameObject);
        }
        activeNotes.Clear();
    }

    void GeneratePatternsFromTimeAndDifficulty(float difficulty)
    {
        stepPatterns.Clear();

        float ratio = timeLimit <= 10f ? 0.6f : 0.4f;
        float usableTime = timeLimit * ratio;

        float sequenceDuration = 2f;
        int steps = Mathf.Max(1, Mathf.FloorToInt(usableTime / sequenceDuration));
        steps = Mathf.Clamp(steps, 2, 8);

        int minNotes = Mathf.Max(1, stepLengthRange.x);
        int baseMaxNotes = Mathf.Max(minNotes, stepLengthRange.y);
        int diffBoost = Mathf.FloorToInt(difficulty * 1f);
        int maxNotes = Mathf.Clamp(baseMaxNotes + diffBoost, minNotes, 8);

        for (int s = 0; s < steps; s++)
        {
            int length = Random.Range(minNotes, maxNotes + 1);
            var sb = new System.Text.StringBuilder(length);

            bool lastLeft = Random.value < 0.5f;
            for (int i = 0; i < length; i++)
            {
                if (i == 0)
                {
                    lastLeft = Random.value < 0.5f;
                }
                else
                {
                    float sameChance = 0.3f;
                    if (Random.value > sameChance)
                        lastLeft = !lastLeft;
                }

                sb.Append(lastLeft ? 'L' : 'R');
            }

            stepPatterns.Add(sb.ToString());
        }
    }

    void Update()
    {
        base.Update();

        if (!running) return;

        HandleSpawning();

        if (Input.GetMouseButtonDown(0))
            HandleClick();

        CheckNotes();
        CheckStepProgress();
        UpdateSprayCans();
    }

    void HandleSpawning()
    {
        if (allStepsSpawned) return;
        if (waitingForStepClear) return;
        if (Time.time < nextSpawnTime) return;

        string pattern = stepPatterns[currentStepIndex];

        if (spawnIndexInStep >= pattern.Length)
        {
            waitingForStepClear = true;
            return;
        }

        bool isLeft = pattern[spawnIndexInStep] == 'L';
        SpawnNote(isLeft);

        spawnIndexInStep++;

        if (spawnIndexInStep < pattern.Length)
            nextSpawnTime = Time.time + beatInterval;
        else
            nextSpawnTime = Time.time + stepPause;
    }

    void SpawnNote(bool isLeft)
    {
        GameObject obj = Instantiate(
            mosquitoPrefab,
            Vector3.zero,
            Quaternion.identity,
            mosquitoContainer
        );

        MosquitoNote mosquito = obj.GetComponent<MosquitoNote>();

        activeNotes.Add(new Note
        {
            mosquito = mosquito,
            isLeft = isLeft,
            spawnTime = Time.time,
            hit = false
        });

        UpdateLanePositions();
    }

    void HandleClick()
    {
        if (activeNotes.Count == 0) return;

        if (Time.time < nextSprayTime)
            return;

        nextSprayTime = Time.time + sprayCooldown;

        float half = Screen.width * 0.5f;
        bool clickLeft = Input.mousePosition.x < half;

        int idx = activeNotes.FindIndex(n => !n.hit && n.isLeft == clickLeft);
        if (idx == -1) return;

        Note note = activeNotes[idx];
        note.hit = true;
        activeNotes[idx] = note;

        note.mosquito?.PlayHitAnimation();
        UpdateLanePositions();
    }

    void CheckNotes()
    {
        for (int i = activeNotes.Count - 1; i >= 0; i--)
        {
            Note n = activeNotes[i];

            if (n.hit && (n.mosquito == null || n.mosquito.IsFinished))
                activeNotes.RemoveAt(i);
        }
    }

    void CheckStepProgress()
    {
        if (!waitingForStepClear) return;
        if (activeNotes.Count > 0) return;

        if (currentStepIndex + 1 < stepPatterns.Count)
        {
            currentStepIndex++;
            spawnIndexInStep = 0;
            waitingForStepClear = false;
            nextSpawnTime = Time.time + stepPause;
        }
        else
        {
            allStepsSpawned = true;
            Win();
        }
    }

    void UpdateLanePositions()
    {
        List<Transform> leftLane = new();
        List<Transform> rightLane = new();

        foreach (var n in activeNotes)
        {
            if (n.hit || n.mosquito == null) continue;
            if (n.isLeft) leftLane.Add(n.mosquito.transform);
            else rightLane.Add(n.mosquito.transform);
        }

        PositionLane(leftLane, true);
        PositionLane(rightLane, false);
    }

    void PositionLane(List<Transform> lane, bool isLeft)
    {
        if (lane.Count == 0) return;

        float w = Screen.width;
        float h = Screen.height;

        float minX = (isLeft ? leftMinXNorm : rightMinXNorm) * w;
        float maxX = (isLeft ? leftMaxXNorm : rightMaxXNorm) * w;
        float minY = minYNorm * h;
        float maxY = maxYNorm * h;
        float total = maxY - minY;

        float segment = total / lane.Count;

        for (int i = 0; i < lane.Count; i++)
        {
            float segMinY = minY + segment * i;
            float segMaxY = segMinY + segment;

            float yMin = Mathf.Lerp(segMinY, segMaxY, laneInnerVerticalPadding);
            float yMax = Mathf.Lerp(segMinY, segMaxY, 1f - laneInnerVerticalPadding);

            float sx = Random.Range(minX, maxX);
            float sy = Random.Range(yMin, yMax);

            Transform yung = lane[i];
            float z = cam.WorldToScreenPoint(yung.position).z;
            Vector3 world = cam.ScreenToWorldPoint(new Vector3(sx, sy, z));

            yung.position = world;

            var mn = yung.GetComponent<MosquitoNote>();
            if (mn != null)
            {
                mn.RefreshBasePosition();
                mn.SetNotes(isLeft);
            }
        }
    }

    void UpdateSprayCans()
    {
        float half = Screen.width * 0.5f;
        bool hoverLeft = Input.mousePosition.x < half;

        float leftTarget = hoverLeft ? hoverY : idleY;
        float rightTarget = hoverLeft ? idleY : hoverY;

        if (leftCan)
        {
            leftTween?.Kill();
            leftTween = leftCan.DOMoveY(leftTarget, tweenTime).SetEase(Ease.OutBack);
        }

        if (rightCan)
        {
            rightTween?.Kill();
            rightTween = rightCan.DOMoveY(rightTarget, tweenTime).SetEase(Ease.OutBack);
        }
    }

    void OnDestroy()
    {
        leftTween?.Kill();
        rightTween?.Kill();

        if (leftCan) leftCan.DOKill();
        if (rightCan) rightCan.DOKill();
    }
}
