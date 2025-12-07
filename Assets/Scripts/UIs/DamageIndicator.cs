using UnityEngine;
using DG.Tweening;

public class DamageIndicatorUI : MonoBehaviour
{
    public static DamageIndicatorUI Instance;

    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private float flashDuration = 0.1f;

    Tween flashTween;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        if (canvasGroup != null)
            canvasGroup.alpha = 0f;
    }

    public void Flash()
    {
        if (canvasGroup == null) return;

        flashTween?.Kill();

        canvasGroup.alpha = 1f;

        flashTween = canvasGroup
            .DOFade(0f, flashDuration)
            .SetEase(Ease.Linear);
    }
}