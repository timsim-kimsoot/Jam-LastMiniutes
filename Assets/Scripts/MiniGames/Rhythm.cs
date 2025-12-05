using System.Collections.Generic;
using UnityEngine;

public class MosquitoRhythmMinigame : MinigameBase
{
    [Header("Refs")]
    [SerializeField] Camera cam;
    [SerializeField] GameObject mosquitoPrefab;

    [Header("Random Pattern Settings")]
    [SerializeField] int minSteps = 2;                  // min number of steps
    [SerializeField] int maxSteps = 4;                  // max number of steps
    [SerializeField] Vector2Int stepLengthRange = new Vector2Int(2, 5); // min/max notes per step

    [Header("Timing")]
    [SerializeField] float leadIn = 0.5f;       // wait before first step starts
    [SerializeField] float beatInterval = 0.4f; // time between notes in a step
    [SerializeField] float stepPause = 0.5f;    // pause between steps
    [SerializeField] float noteLifetime = 1.1f; // fail if a note lives longer than this

    [Header("Spawn Areas (normalized screen space)")]
    [Range(0f, 0.5f)] public float leftMinXNorm = 0.05f;
    [Range(0f, 0.5f)] public float leftMaxXNorm = 0.45f;
    [Range(0.5f, 1f)] public float rightMinXNorm = 0.55f;
    [Range(0.5f, 1f)] public float rightMaxXNorm = 0.95f;

    [Range(0f, 1f)] public float minYNorm = 0.15f;
    [Range(0f, 1f)] public float maxYNorm = 0.85f;

    [Header("Lane Layout")]
    [SerializeField, Range(0f, 0.4f)] float laneInnerVerticalPadding = 0.15f; // inner vertical padding inside each band

    // Runtime-generated patterns (e.g. "LLR", "RLLL", etc.)
    [SerializeField] List<string> stepPatterns = new List<string>();

    struct Note
    {
        public MosquitoNote mosquito;
        public bool isLeft;
        public float spawnTime;
        public bool hit;
    }

    readonly List<Note> activeNotes = new List<Note>();

    int currentStepIndex;
    int spawnIndexInStep;
    float nextSpawnTime;
    bool waitingForStepClear;
    bool allStepsSpawned;
    bool started;
    bool finished;

    void Awake()
    {
        // If MinigameManager also calls StartGame(), remove this to avoid double-start.
        StartGame();
    }

    public override void StartGame()
    {
        if (cam == null)
            cam = Camera.main;

        // cleanup old
        foreach (var n in activeNotes)
        {
            if (n.mosquito != null)
                Destroy(n.mosquito.gameObject);
        }
        activeNotes.Clear();

        GenerateRandomPatterns();

        if (stepPatterns == null || stepPatterns.Count == 0)
        {
            Debug.LogError("MosquitoRhythmMinigame: no stepPatterns generated!");
            finished = true;
            started = false;
            FinishGame();
            return;
        }

        currentStepIndex = 0;
        spawnIndexInStep = 0;
        waitingForStepClear = false;
        allStepsSpawned = false;

        nextSpawnTime = Time.time + leadIn;
        started = true;
        finished = false;

        Debug.Log($"[MosquitoRhythm] StartGame: step 0 pattern = \"{stepPatterns[0]}\"");
    }

    void GenerateRandomPatterns()
    {
        stepPatterns.Clear();

        int steps = Random.Range(minSteps, maxSteps + 1);
        for (int s = 0; s < steps; s++)
        {
            int length = Random.Range(stepLengthRange.x, stepLengthRange.y + 1);
            System.Text.StringBuilder sb = new System.Text.StringBuilder(length);
            for (int i = 0; i < length; i++)
            {
                bool left = Random.value < 0.5f;
                sb.Append(left ? 'L' : 'R');
            }

            string pattern = sb.ToString();
            stepPatterns.Add(pattern);
            Debug.Log($"[MosquitoRhythm] Generated step {s} pattern = \"{pattern}\"");
        }
    }

    void Update()
    {
        if (!started || finished) return;

        HandleSpawning();

        if (Input.GetMouseButtonDown(0))
        {
            HandleClick();
        }

        CheckNotes();
        CheckStepProgress();
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
            Debug.Log($"[MosquitoRhythm] Step {currentStepIndex} fully spawned, waiting for player.");
            return;
        }

        char c = pattern[spawnIndexInStep];
        bool isLeft = (c == 'L' || c == 'l');

        SpawnNote(isLeft);

        spawnIndexInStep++;
        if (spawnIndexInStep < pattern.Length)
        {
            nextSpawnTime = Time.time + beatInterval;
        }
        else
        {
            waitingForStepClear = true;
            Debug.Log($"[MosquitoRhythm] Last note of step {currentStepIndex} spawned.");
        }
    }

    void SpawnNote(bool isLeft)
    {
        // Create mosquito at some temporary position (will be placed in UpdateLanePositions)
        GameObject obj = Instantiate(mosquitoPrefab, Vector3.zero, Quaternion.identity);
        MosquitoNote mosquito = obj.GetComponent<MosquitoNote>();

        if (mosquito == null)
            Debug.LogWarning("Mosquito prefab has no MosquitoNote component!");

        Note note = new Note
        {
            mosquito = mosquito,
            isLeft = isLeft,
            spawnTime = Time.time,
            hit = false
        };
        activeNotes.Add(note);

        Debug.Log($"[MosquitoRhythm] Spawn note: step={currentStepIndex}, indexInStep={spawnIndexInStep}, side={(isLeft ? "LEFT" : "RIGHT")}");

        // After adding, re-layout lanes
        UpdateLanePositions();
    }

    void HandleClick()
    {
        if (activeNotes.Count == 0)
        {
            Debug.Log("[MosquitoRhythm] Click but no active notes.");
            return;
        }

        float half = Screen.width * 0.5f;
        bool clickLeft = Input.mousePosition.x < half;

        Debug.Log($"[MosquitoRhythm] Click X={Input.mousePosition.x}, half={half}, side={(clickLeft ? "LEFT" : "RIGHT")}");

        int idx = -1;
        for (int i = 0; i < activeNotes.Count; i++)
        {
            if (!activeNotes[i].hit && activeNotes[i].isLeft == clickLeft)
            {
                idx = i;
                break;
            }
        }

        if (idx == -1)
        {
            Debug.Log("[MosquitoRhythm] Clicked wrong side or no unhit note on that side.");
            return;
        }

        Note n = activeNotes[idx];
        n.hit = true;
        activeNotes[idx] = n;

        Debug.Log($"[MosquitoRhythm] HIT note index={idx}, side={(n.isLeft ? "LEFT" : "RIGHT")}");

        if (n.mosquito != null)
        {
            n.mosquito.PlayHitAnimation();
        }

        // Re-layout remaining un-hit ones in their lanes
        UpdateLanePositions();
    }

    void CheckNotes()
    {
        for (int i = activeNotes.Count - 1; i >= 0; i--)
        {
            Note n = activeNotes[i];

            if (!n.hit && Time.time - n.spawnTime >= noteLifetime)
            {
                Debug.Log($"[MosquitoRhythm] MISS note side={(n.isLeft ? "LEFT" : "RIGHT")} life={Time.time - n.spawnTime:F2}");
                Fail();
                return;
            }

            if (n.hit && (n.mosquito == null || n.mosquito.IsFinished))
            {
                activeNotes.RemoveAt(i);
            }
        }
    }

    void CheckStepProgress()
    {
        if (finished) return;
        if (!waitingForStepClear) return;

        if (activeNotes.Count > 0) return;

        Debug.Log($"[MosquitoRhythm] Step {currentStepIndex} CLEARED.");

        if (currentStepIndex + 1 < stepPatterns.Count)
        {
            currentStepIndex++;
            spawnIndexInStep = 0;
            waitingForStepClear = false;
            nextSpawnTime = Time.time + stepPause;

            Debug.Log($"[MosquitoRhythm] Moving to step {currentStepIndex}, pattern=\"{stepPatterns[currentStepIndex]}\"");
        }
        else
        {
            allStepsSpawned = true;
            finished = true;
            started = false;
            Debug.Log("[MosquitoRhythm] All steps cleared. Minigame complete!");
            FinishGame();
        }
    }

    void UpdateLanePositions()
    {
        if (cam == null) return;

        // Build un-hit lists per side in spawn order
        List<Transform> leftLane = new List<Transform>();
        List<Transform> rightLane = new List<Transform>();

        for (int i = 0; i < activeNotes.Count; i++)
        {
            Note n = activeNotes[i];
            if (n.mosquito == null) continue;
            if (n.hit) continue;

            if (n.isLeft) leftLane.Add(n.mosquito.transform);
            else rightLane.Add(n.mosquito.transform);
        }

        PositionLane(leftLane, true);
        PositionLane(rightLane, false);
    }

    void PositionLane(List<Transform> lane, bool isLeft)
    {
        if (lane == null || lane.Count == 0) return;

        float screenWidth = Screen.width;
        float screenHeight = Screen.height;

        // Horizontal range for this side (normalized -> pixel)
        float minXNorm = isLeft ? leftMinXNorm : rightMinXNorm;
        float maxXNorm = isLeft ? leftMaxXNorm : rightMaxXNorm;
        float minX = minXNorm * screenWidth;
        float maxX = maxXNorm * screenWidth;

        // Vertical global range
        float minY = minYNorm * screenHeight;
        float maxY = maxYNorm * screenHeight;
        float totalHeight = maxY - minY;

        int count = lane.Count;
        float segmentHeight = totalHeight / count;

        for (int i = 0; i < count; i++)
        {
            Transform t = lane[i];
            if (t == null) continue;

            // This mosquito's band (segment)
            float segMinY = minY + segmentHeight * i;
            float segMaxY = segMinY + segmentHeight;

            float innerPad = segmentHeight * laneInnerVerticalPadding;
            float yMinInner = Mathf.Lerp(segMinY, segMaxY, laneInnerVerticalPadding);
            float yMaxInner = Mathf.Lerp(segMinY, segMaxY, 1f - laneInnerVerticalPadding);

            float sx = Random.Range(minX, maxX);
            float sy = Random.Range(yMinInner, yMaxInner);

            // Keep same Z distance from camera
            Vector3 currentWorld = t.position;
            float screenZ = cam.WorldToScreenPoint(currentWorld).z;

            Vector3 screenPos = new Vector3(sx, sy, screenZ);
            Vector3 worldPos = cam.ScreenToWorldPoint(screenPos);

            t.position = worldPos;
        }
    }

    void Fail()
    {
        finished = true;
        started = false;
        Debug.Log("[MosquitoRhythm] FAILED (missed note)!");
        FinishGame();
    }
}
