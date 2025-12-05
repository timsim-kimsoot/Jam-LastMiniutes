using UnityEngine;
using DG.Tweening;

public class MosquitoNote : MonoBehaviour
{
    [SerializeField] float spawnScaleMultiplier = 1.5f;
    [SerializeField] float spawnScaleDuration = 0.2f;

    [SerializeField] float wobbleOffset = 0.1f;
    [SerializeField] float wobbleDuration = 0.4f;

    [SerializeField] float fallDistance = 5f;
    [SerializeField] float fallDuration = 0.35f;

    Vector3 baseLocalPos;
    Vector3 baseScale;
    Tween wobbleTween;
    bool finished;

    public bool IsFinished => finished;

    void Awake()
    {
        baseLocalPos = transform.localPosition;
        baseScale = transform.localScale;

        transform.localScale = baseScale * spawnScaleMultiplier;
        transform.DOScale(baseScale, spawnScaleDuration)
                 .SetEase(Ease.OutBack);

        wobbleTween = transform.DOLocalMove(
                            baseLocalPos + new Vector3(0f, wobbleOffset, 0f),
                            wobbleDuration)
                        .SetEase(Ease.InOutSine)
                        .SetLoops(-1, LoopType.Yoyo);
    }

    public void PlayHitAnimation()
    {
        if (finished) return;
        finished = true;

        if (wobbleTween != null && wobbleTween.IsActive())
            wobbleTween.Kill();

        var col2D = GetComponent<Collider2D>();
        if (col2D != null) col2D.enabled = false;

        Sequence s = DOTween.Sequence();
        s.Join(transform.DOMoveY(transform.position.y - fallDistance, fallDuration)
                        .SetEase(Ease.InQuad));
        s.Join(transform.DORotate(
                    new Vector3(0f, 0f, Random.Range(-180f, 180f)),
                    fallDuration));
        s.Join(transform.DOScale(baseScale * 0.7f, fallDuration));
        s.OnComplete(() => Destroy(gameObject));
    }
}
