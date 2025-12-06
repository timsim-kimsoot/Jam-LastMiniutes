using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class OsuClickMinigame : MinigameBase
{
    [Header("Refs")]
    [SerializeField] GameObject hitPrefab;
    [SerializeField] Transform hitContainer;

    [Header("Spawn Bounds")]
    [SerializeField] float minX = -7f;
    [SerializeField] float maxX = 7f;
    [SerializeField] float minY = -3f;
    [SerializeField] float maxY = 3f;

    [Header("Timing")]
    [SerializeField] float pointInterval = 0.4f;
    [SerializeField] float noteLifetime = 1.2f;
    [SerializeField] float leadIn = 0.3f;
    [SerializeField] float MissPenaltySeconds = 0.5f;

    [Header("Sequence")]
    [SerializeField] Vector2Int sequenceLengthRange = new Vector2Int(2, 8);
    [SerializeField] float pauseEasy = 0.75f;
    [SerializeField] float pauseHard = 0.3f;

    [Header("Hands")]
    [SerializeField] GameObject Hands;
    [SerializeField] Sprite normalSprite;
    [SerializeField] Sprite hitSprite;
    [SerializeField] float hitDuration = 0.1f;

    readonly List<OsuNote> activeNotes = new List<OsuNote>();
    readonly List<int> sequenceLengths = new List<int>();

    int totalPoints;

    int spawnedPoints;
    int despawnedPoints;
    int hitPoints;

    int currentSequenceIndex;
    int notesInCurrentSequence;

    float nextSpawnTime;
    float seqPause;
    bool allSpawned;

    void Awake()
    {
        Init(0f);
    }

    public override void Init(float difficulty)
    {
        base.Init(difficulty);

        float ratio = timeLimit <= 10f ? 0.6f : 0.4f;
        float usableTime = timeLimit * ratio;

        float basePointsF = usableTime / pointInterval;
        int basePoints = Mathf.Max(1, Mathf.FloorToInt(basePointsF));

        int extra = Mathf.FloorToInt(difficulty * 1.5f);
        totalPoints = Mathf.Clamp(basePoints + extra, 1, 60);

        float d01 = Mathf.Clamp01(difficulty / 5f);
        seqPause = Mathf.Lerp(pauseEasy, pauseHard, d01);

        BuildSequences();

        despawnedPoints = 0;
        hitPoints = 0;
        allSpawned = false;
        currentSequenceIndex = 0;
        notesInCurrentSequence = 0;
        nextSpawnTime = Time.time + leadIn;
    }

    void BuildSequences()
    {
        sequenceLengths.Clear();
        int remaining = totalPoints;

        int minLen = Mathf.Max(1, sequenceLengthRange.x);
        int maxLen = Mathf.Max(minLen, sequenceLengthRange.y);

        while (remaining > 0)
        {
            int maxForThis = Mathf.Min(maxLen, remaining);
            int len = Random.Range(minLen, maxForThis + 1);
            sequenceLengths.Add(len);
            remaining -= len;
        }
    }

    void Update()
    {
        base.Update();
        if (!running) return;

        HandleSpawning();
    }

    void HandleSpawning()
    {
        if (allSpawned) return;
        if (Time.time < nextSpawnTime) return;
        if (currentSequenceIndex >= sequenceLengths.Count)
        {
            allSpawned = true;
            return;
        }

        int seqLen = sequenceLengths[currentSequenceIndex];

        SpawnPoint();
        notesInCurrentSequence++;
        spawnedPoints++;
        if (notesInCurrentSequence < seqLen)
        {
            nextSpawnTime += pointInterval;
        }
        else
        {
            currentSequenceIndex++;
            notesInCurrentSequence = 0;

            if (currentSequenceIndex < sequenceLengths.Count)
                nextSpawnTime += seqPause;
            else
                allSpawned = true;
        }
    }

    void SpawnPoint()
    {
        if (hitPrefab == null) return;

        Vector3 pos = new Vector3(
            Random.Range(minX, maxX),
            Random.Range(minY, maxY),
            0f
        );

        Transform parent = hitContainer != null ? hitContainer : transform;

        GameObject obj = Instantiate(hitPrefab, pos, Quaternion.identity, parent);
        OsuNote note = obj.GetComponent<OsuNote>();
        if (note == null) note = obj.AddComponent<OsuNote>();

        note.SetLifetime(noteLifetime);
        note.OnHit += HandleHit;
        note.OnMiss += HandleMiss;

        activeNotes.Add(note);
    }

    void HandleHit(OsuNote note)
    {
        hitPoints++;
        activeNotes.Remove(note);
        despawnedPoints++;

        var hsr = Hands.GetComponent<SpriteRenderer>();
        if (Hands != null)
        {
            hsr.sprite = hitSprite;

            hsr.transform.DOKill();
            hsr.transform.localScale = Vector3.one;

            hsr.transform
                .DOPunchScale(new Vector3(0.25f, 0.25f, 0f), 0.15f, 10, 1f)
                .OnComplete(() =>
                {
                    DOVirtual.DelayedCall(hitDuration, () =>
                    {
                        if (hsr != null) hsr.sprite = normalSprite;
                    });
                });
        }

        if (allSpawned && despawnedPoints == spawnedPoints)
            Win();
    }

    void HandleMiss(OsuNote note)
    {
        despawnedPoints++;
        activeNotes.Remove(note);
        timer = Mathf.Max(0f, timer - MissPenaltySeconds);
    }

    void OnDestroy()
    {
        for (int i = 0; i < activeNotes.Count; i++)
        {
            if (activeNotes[i] != null)
            {
                activeNotes[i].OnHit -= HandleHit;
                activeNotes[i].OnMiss -= HandleMiss;
            }
        }
    }
}