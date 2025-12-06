using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;

public class UIButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] float hoverScale = 1.08f;
    [SerializeField] float clickScale = 0.95f;
    [SerializeField] float tweenDuration = 0.12f;

    RectTransform rect;
    Tween currentTween;
    Vector3 baseScale;

    void Awake()
    {
        rect = transform as RectTransform;
        baseScale = rect.localScale;
    }

    void OnDisable()
    {
        currentTween?.Kill();
        rect.localScale = baseScale;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        PlayScaleTween(baseScale * hoverScale);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        PlayScaleTween(baseScale);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        PlayScaleTween(baseScale * clickScale);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        PlayScaleTween(baseScale * hoverScale);
    }

    void PlayScaleTween(Vector3 targetScale)
    {
        currentTween?.Kill();
        currentTween = rect.DOScale(targetScale, tweenDuration)
            .SetEase(Ease.OutQuad);
    }
}
