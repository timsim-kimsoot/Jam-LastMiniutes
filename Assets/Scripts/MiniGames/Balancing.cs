using UnityEngine;

public class BalanceMinigame : MinigameBase
{
    [SerializeField] Transform balanceObject;

    [SerializeField] float maxAngle = 30f;
    [SerializeField] float minDuration = 5f;
    [SerializeField] float maxDuration = 12f;
    [SerializeField] float easyPhaseDuration = 2f;

    [SerializeField] float baseDriftSpeed = 10f;
    [SerializeField] float maxDriftSpeed = 60f;
    [SerializeField] float playerCounterSpeed = 80f;
    [SerializeField] Vector2 directionFlipInterval = new Vector2(0.4f, 1.2f);

    float tilt;
    float duration;
    float elapsed;
    float driftSign;
    float currentDriftSpeed;
    float nextFlipTime;

    bool gameStarted;
    bool finished;

    private void Awake()
    {
        StartGame();
    }

    public override void StartGame()
    {
        finished = false;
        gameStarted = true;

        elapsed = 0f;
        tilt = 0f;

        duration = Random.Range(minDuration, maxDuration);
        driftSign = Random.value < 0.5f ? -1f : 1f;

        ScheduleNextDirectionFlip();

        if (balanceObject == null)
            balanceObject = transform;

        balanceObject.localRotation = Quaternion.identity;
    }

    void ScheduleNextDirectionFlip()
    {
        nextFlipTime = Time.time + Random.Range(directionFlipInterval.x, directionFlipInterval.y);
    }

    void Update()
    {
        if (!gameStarted || finished) return;

        float dt = Time.deltaTime;

        elapsed += dt;
        if (elapsed >= duration)
        {
            finished = true;
            gameStarted = false;
            FinishGame();
            return;
        }

        float t;
        if (elapsed <= easyPhaseDuration)
        {
            t = 0f;
        }
        else
        {
            float remaining = duration - easyPhaseDuration;
            t = remaining > 0f ? Mathf.Clamp01((elapsed - easyPhaseDuration) / remaining) : 1f;
        }

        currentDriftSpeed = Mathf.Lerp(baseDriftSpeed, maxDriftSpeed, t);

        if (Time.time >= nextFlipTime)
        {
            driftSign = Random.value < 0.5f ? -1f : 1f;
            ScheduleNextDirectionFlip();
        }

        tilt += driftSign * currentDriftSpeed * dt;

        float input = 0f; 
        if (Input.GetMouseButton(0))
            input += 1f;
        if (Input.GetMouseButton(1))
            input -= 1f;


        tilt += input * playerCounterSpeed * dt;
        float hardLimit = maxAngle * 1.2f;
        tilt = Mathf.Clamp(tilt, -hardLimit, hardLimit);

        balanceObject.localRotation = Quaternion.Euler(0f, 0f, tilt);

        if (Mathf.Abs(tilt) >= maxAngle)
        {
            finished = true;
            gameStarted = false;
            Debug.Log("Balance failed!");
            FinishGame();
        }
    }
}
