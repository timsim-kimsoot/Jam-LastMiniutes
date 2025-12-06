using UnityEngine;
using DG.Tweening;

public class Wallet : MonoBehaviour
{
    [SerializeField] float duration = 0.35f;
    [SerializeField] float posStrength = 10f;
    [SerializeField] float rotStrength = 12f;
    [SerializeField] int vibrato = 10;
    [SerializeField] float randomness = 90f;

    Tween currentTween;

    public void Shake()
    {
        if (currentTween != null && currentTween.IsActive())
            currentTween.Kill();

        RectTransform rect = transform as RectTransform;

        if (rect != null)
        {
            Sequence s = DOTween.Sequence();
            s.Join(rect.DOShakeAnchorPos(duration, new Vector2(0f, posStrength), vibrato, randomness, false, true));
            s.Join(rect.DOShakeRotation(duration, new Vector3(0f, 0f, rotStrength), vibrato, randomness));
            currentTween = s;
        }
        else
        {
            Sequence s = DOTween.Sequence();
            s.Join(transform.DOShakePosition(duration, new Vector3(0f, posStrength, 0f), vibrato, randomness, false, true));
            s.Join(transform.DOShakeRotation(duration, new Vector3(0f, 0f, rotStrength), vibrato, randomness));
            currentTween = s;
        }
    }

    void OnDisable()
    {
        if (currentTween != null && currentTween.IsActive())
            currentTween.Kill();
    }
}
