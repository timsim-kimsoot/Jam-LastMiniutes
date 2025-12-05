using System.Collections.Generic;
using UnityEngine;

public class OsuClick : MinigameBase
{
    [SerializeField] Camera cam;
    [SerializeField] GameObject targetPrefab;
    [SerializeField] Transform targetsParent;

    [SerializeField] Vector2 spawnMin = new Vector2(-5f, -3f);
    [SerializeField] Vector2 spawnMax = new Vector2(5f, 3f);

    [SerializeField] float minDuration = 5f;
    [SerializeField] float maxDuration = 12f;
    [SerializeField] float targetLifetime = 1.2f;

    [SerializeField] float endSpawnInterval = 0.25f;

    class Target
    {
        public GameObject go;
        public float spawnTime;
    }

    readonly List<Target> activeTargets = new List<Target>();

    float duration;
    float elapsed;
    float nextSpawnAt;

    bool gameStarted;
    bool finished;

    void Awake()
    {
        StartGame();
    }

    public override void StartGame()
    {
        if (cam == null)
            cam = Camera.main;

        finished = false;
        gameStarted = true;

        foreach (var t in activeTargets)
        {
            if (t.go != null)
                Destroy(t.go);
        }
        activeTargets.Clear();

        elapsed = 0f;
        duration = Random.Range(minDuration, maxDuration);
        nextSpawnAt = 0f;
    }

    void Update()
    {
        if (!gameStarted || finished) return;

        float dt = Time.deltaTime;
        elapsed += dt;

        if (elapsed >= duration && activeTargets.Count == 0)
        {
            finished = true;
            gameStarted = false;
            FinishGame();
            return;
        }

        HandleSpawning();

        HandleClick();

        HandleTargetTimeouts();
    }

    void HandleSpawning()
    {
        if (elapsed < nextSpawnAt) return;

        float t = Mathf.Clamp01(elapsed / duration);
        float currentInterval = Mathf.Lerp(0.2f, endSpawnInterval, t);

        SpawnTarget();
        nextSpawnAt = elapsed + currentInterval;
    }

    void SpawnTarget()
    {
        if (targetPrefab == null) return;

        float x = Random.Range(spawnMin.x, spawnMax.x);
        float y = Random.Range(spawnMin.y, spawnMax.y);
        Vector3 pos = new Vector3(x, y, 0f);

        GameObject obj = Instantiate(targetPrefab, pos, Quaternion.identity, targetsParent);

        activeTargets.Add(new Target
        {
            go = obj,
            spawnTime = elapsed
        });
    }

    void HandleClick()
    {
        if (!Input.GetMouseButtonDown(0)) return;

        Vector3 mouseWorld = cam.ScreenToWorldPoint(Input.mousePosition);
        Vector2 mousePos2D = mouseWorld;

        RaycastHit2D hit = Physics2D.Raycast(mousePos2D, Vector2.zero);
        if (hit.collider == null) return;

        for (int i = activeTargets.Count - 1; i >= 0; i--)
        {
            if (activeTargets[i].go == null)
            {
                activeTargets.RemoveAt(i);
                continue;
            }

            if (hit.collider.gameObject == activeTargets[i].go)
            {
                Destroy(activeTargets[i].go);
                activeTargets.RemoveAt(i);
                break;
            }
        }
    }

    void HandleTargetTimeouts()
    {
        for (int i = activeTargets.Count - 1; i >= 0; i--)
        {
            var t = activeTargets[i];
            if (t.go == null)
            {
                activeTargets.RemoveAt(i);
                continue;
            }

            if (elapsed - t.spawnTime >= targetLifetime)
            {
                finished = true;
                gameStarted = false;
                Debug.Log("Osu minigame failed: missed target");

                // Optional: destroy leftovers
                foreach (var other in activeTargets)
                    if (other.go != null)
                        Destroy(other.go);
                activeTargets.Clear();

                FinishGame();
                break;
            }
        }
    }
}
