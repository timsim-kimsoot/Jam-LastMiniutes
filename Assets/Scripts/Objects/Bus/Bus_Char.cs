using UnityEngine;
using System;
using DG.Tweening;

public class Bus_Char : MonoBehaviour
{
    public float fallSpeed = 12f;

    Tween fallTween;
    Action onLanded;

    public void FallTo(float targetY, Action callback)
    {
        onLanded = callback;

        if (fallTween != null && fallTween.IsActive())
            fallTween.Kill();

        float distance = Mathf.Abs(transform.position.y - targetY);
        float duration = Mathf.Max(0.05f, distance / fallSpeed);

        fallTween = transform
            .DOMoveY(targetY, duration)
            .SetEase(Ease.InQuad)
            .OnComplete(() =>
            {
                onLanded?.Invoke();
            });
    }

    void OnDestroy()
    {
        if (fallTween != null && fallTween.IsActive())
            fallTween.Kill();
    }
}
