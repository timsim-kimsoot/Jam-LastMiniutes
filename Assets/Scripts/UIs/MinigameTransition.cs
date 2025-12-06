using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;
using DG.Tweening;

public class MinigameTransitionUI : MonoBehaviour
{
    public static MinigameTransitionUI Instance { get; private set; }

    [Header("Transition UI")]
    [SerializeField] CanvasGroup transitionGroup;
    [SerializeField] RectTransform transitionHole;
    [SerializeField] TMP_Text transitionText;

    [Header("Timing")]
    [SerializeField] float fadeDuration = 0.3f;
    [SerializeField] float readyHold = 0.5f;
    [SerializeField] float goHold = 0.35f;

    [Header("Zoom")]
    [SerializeField] float zoomStartScale = 3f;
    [SerializeField] float zoomEndScale = 0.1f;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public IEnumerator PlayReadyIntro()
    {
        if (transitionGroup == null || transitionHole == null)
            yield break;

        transitionGroup.gameObject.SetActive(true);

        transitionGroup.DOKill();
        transitionHole.DOKill();

        transitionGroup.alpha = 1f;
        transitionHole.localScale = Vector3.one * zoomStartScale;

        if (transitionText != null)
            transitionText.text = "READY";

        bool done = false;

        Sequence seq = DOTween.Sequence();

        seq.Append(transitionHole
            .DOScale(1f, fadeDuration)
            .SetEase(Ease.OutBack));

        seq.AppendInterval(readyHold);

        seq.AppendCallback(() =>
        {
            if (transitionText != null)
                transitionText.text = "GO!";
        });

        seq.Append(transitionHole
            .DOScale(zoomEndScale, fadeDuration)
            .SetEase(Ease.InBack));

        seq.AppendInterval(goHold);

        seq.AppendCallback(() =>
        {
            if (transitionText != null)
                transitionText.text = "";
            transitionGroup.alpha = 0f;
        });

        seq.OnComplete(() => done = true);

        while (!done)
            yield return null;
    }

    public void ZoomOutToBlack()
    {
        if (transitionGroup == null || transitionHole == null)
            return;

        transitionGroup.gameObject.SetActive(true);

        transitionGroup.DOKill();
        transitionHole.DOKill();

        transitionGroup.alpha = 1f;
        transitionHole.localScale = Vector3.one * zoomEndScale;

        if (transitionText != null)
            transitionText.text = "";

        transitionHole.localScale = Vector3.one * zoomStartScale;
    }
}
