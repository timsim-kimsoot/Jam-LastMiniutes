using System.Collections.Generic;
using UnityEngine;

public class PrinterMinigame : MinigameBase
{
    [Header("Refs")]
    [SerializeField] SpriteRenderer printerSprite;
    [SerializeField] Collider2D printerBodyCollider;
    [SerializeField] Transform paper;
    [SerializeField] Transform paperStartPoint;
    [SerializeField] Transform paperEndPoint;

    [Header("Jam Settings")]
    [SerializeField] Vector2Int jamCountRange = new Vector2Int(1, 3);
    [SerializeField] float shockPenaltySeconds = 1f;

    Camera cam;

    float printDuration;
    float printElapsed;
    bool printingComplete;
    bool isJammed;

    readonly List<float> jamTimes = new();
    int jamIndex;

    Vector3 paperStartPos;
    Vector3 paperEndPos;
    Color normalColor;

    private void Awake()
    {
        Init(0f);
    }
    public override void Init(float difficulty)
    {
        base.Init(difficulty);

        if (cam == null)
            cam = Camera.main;

        if (printerSprite != null)
            normalColor = printerSprite.color;

        if (paperStartPoint != null)
            paperStartPos = paperStartPoint.position;
        else if (paper != null)
            paperStartPos = paper.position;

        if (paperEndPoint != null)
            paperEndPos = paperEndPoint.position;
        else if (paper != null)
            paperEndPos = paper.position;

        if (paper != null)
            paper.position = paperStartPos;

        printDuration = Mathf.Max(1f, timeLimit - 5f);

        GenerateJamTimes(difficulty);

        printElapsed = 0f;
        printingComplete = false;
        isJammed = false;
        jamIndex = 0;
    }

    void GenerateJamTimes(float difficulty)
    {
        jamTimes.Clear();

        float t = Mathf.Clamp01(difficulty / 3f);
        float rawCount = Mathf.Lerp(jamCountRange.x, jamCountRange.y, t);
        int count = Mathf.Clamp(Mathf.RoundToInt(rawCount), jamCountRange.x, jamCountRange.y);

        if (count <= 0 || printDuration <= 0f)
            return;

        float segment = printDuration / (count + 1);

        for (int i = 1; i <= count; i++)
        {
            float baseTime = segment * i;
            float offset = Random.Range(-segment * 0.25f, segment * 0.25f);
            float jamTime = Mathf.Clamp(baseTime + offset, 0.2f, printDuration - 0.2f);
            jamTimes.Add(jamTime);
        }

        jamTimes.Sort();
    }

    void Update()
    {
        base.Update();
        if (!running) return;

        HandleHover();
        HandleInput();

        if (!printingComplete)
            UpdatePrinting();
    }

    void HandleHover()
    {
        if (printerBodyCollider == null) return;
        if (cam == null) cam = Camera.main;

        Vector3 world = cam.ScreenToWorldPoint(Input.mousePosition);
        Vector2 p = world;

        //if (printerBodyCollider.OverlapPoint(p))
        //    Debug.Log("hover!");
    }

    void HandleInput()
    {
        if (!Input.GetMouseButtonDown(0)) return;
        if (!running) return;
        if (printingComplete) return;

        if (cam == null) cam = Camera.main;

        Vector3 world = cam.ScreenToWorldPoint(Input.mousePosition);
        Vector2 p = world;

        if (printerBodyCollider != null && printerBodyCollider.OverlapPoint(p))
            HitPrinter();
    }

    void UpdatePrinting()
    {
        if (isJammed) return;

        printElapsed += Time.deltaTime;
        float progress = Mathf.Clamp01(printElapsed / printDuration);

        if (paper != null)
            paper.position = Vector3.Lerp(paperStartPos, paperEndPos, progress);

        if (jamIndex < jamTimes.Count && printElapsed >= jamTimes[jamIndex])
        {
            isJammed = true;
            jamIndex++;

            if (ColorUtility.TryParseHtmlString("#A4A4A4", out var jamColor))
                SetPrinterColor(jamColor);
            else
                SetPrinterColor(Color.gray);
        }

        if (progress >= 1f)
        {
            printingComplete = true;
            isJammed = false;
            SetPrinterColor(normalColor);
            Win();
        }
    }

    void HitPrinter()
    {
        if (!running) return;
        if (printingComplete) return;

        if (isJammed)
        {
            isJammed = false;
            SetPrinterColor(normalColor);
        }
        else
        {
            timer = Mathf.Max(0f, timer - shockPenaltySeconds);
        }
    }

    void SetPrinterColor(Color c)
    {
        if (printerSprite != null)
            printerSprite.color = c;
    }
}
