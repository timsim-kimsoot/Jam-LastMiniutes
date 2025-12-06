using UnityEngine;
using DG.Tweening;

public class MorphTweenController : MonoBehaviour
{
    [Header("Pole")]
    [SerializeField] Transform poleTarget;
    [SerializeField] Transform poleStart;
    [SerializeField] Transform poleEnd;

    [Header("Road")]
    [SerializeField] Transform roadTarget;
    [SerializeField] Transform roadStart;
    [SerializeField] Transform roadEnd;

    [Header("Timing")]
    [SerializeField] float morphDuration = 1f;

    void Start()
    {
        StartPoleLoop();
        StartRoadLoop();
    }

    void StartPoleLoop()
    {
        poleTarget.position = poleStart.position;
        poleTarget.rotation = poleStart.rotation;
        poleTarget.localScale = poleStart.localScale;

        DOTween.Sequence()
            .Append(ApplyTween(poleTarget, poleEnd))
            .AppendCallback(() =>
            {
                poleTarget.position = poleStart.position;
                poleTarget.rotation = poleStart.rotation;
                poleTarget.localScale = poleStart.localScale;
            })
            .SetLoops(-1, LoopType.Restart);
    }

    void StartRoadLoop()
    {
        roadTarget.position = roadStart.position;
        roadTarget.rotation = roadStart.rotation;
        roadTarget.localScale = roadStart.localScale;

        DOTween.Sequence()
            .Append(ApplyTween(roadTarget, roadEnd))
            .AppendCallback(() =>
            {
                roadTarget.position = roadStart.position;
                roadTarget.rotation = roadStart.rotation;
                roadTarget.localScale = roadStart.localScale;
            })
            .SetLoops(-1, LoopType.Restart);
    }

    Tween ApplyTween(Transform target, Transform goal)
    {
        return DOTween.Sequence()
            .Join(target.DOMove(goal.position, morphDuration).SetEase(Ease.Linear))
            .Join(target.DORotate(goal.rotation.eulerAngles, morphDuration).SetEase(Ease.Linear))
            .Join(target.DOScale(goal.localScale, morphDuration).SetEase(Ease.Linear));
    }
}
