using System;
using UnityEngine;

public abstract class MinigameBase : MonoBehaviour
{
    public Action OnFinished;
    public virtual void StartGame() 
    { 
    
    }

    protected void FinishGame()
    {
        OnFinished?.Invoke();
    }
}
