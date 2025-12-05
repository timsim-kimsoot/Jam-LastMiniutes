using UnityEngine;
using System;

public abstract class MinigameBase : MonoBehaviour
{
    public float timeLimit = 5f;
    protected float timer;

    public Action OnWin;
    public Action OnFail;

    protected bool running;

    public virtual void Init(float difficulty)
    {
        timeLimit = Mathf.Max(1f, timeLimit - difficulty * 0.2f);

        timer = timeLimit;
        running = true;
    }

    protected virtual void Update()
    {
        if (!running) return;

        timer -= Time.deltaTime;
        if (timer <= 0f)
        {
            running = false;
            OnFail?.Invoke();
        }
    }

    protected void Win()
    {
        if (!running) return;
        running = false;
        OnWin?.Invoke();
    }

    protected void Fail()
    {
        if (!running) return;
        running = false;
        OnFail?.Invoke();
    }
    public float GetRemainingTime()
    {
        return Mathf.Max(0f, timer);
    }

    public float GetTimeLimit()
    {
        return timeLimit;
    }
}
