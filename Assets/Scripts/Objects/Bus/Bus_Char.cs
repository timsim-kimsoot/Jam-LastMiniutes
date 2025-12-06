using UnityEngine;
using System;

public class Bus_Char : MonoBehaviour
{
    public float fallSpeed = 5f;
    private bool falling = false;
    private float targetY;
    private Action onLanded;

    public void FallTo(float targetY, Action callback)
    {
        this.targetY = targetY;
        this.onLanded = callback;
        falling = true;
    }

    void Update()
    {
        if (!falling) return;

        transform.position = Vector3.MoveTowards(transform.position,
            new Vector3(transform.position.x, targetY, transform.position.z),
            fallSpeed * Time.deltaTime);

        if (Mathf.Approximately(transform.position.y, targetY))
        {
            falling = false;
            onLanded?.Invoke();
        }
    }
}
