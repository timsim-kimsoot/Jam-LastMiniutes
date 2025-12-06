using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;

public class MosquitoNote : MonoBehaviour
{
    [Header("Spawn")]
    [SerializeField] float spawnScaleMultiplier = 1.5f;
    [SerializeField] float spawnScaleDuration = 0.2f;

    [Header("Motion")]
    [SerializeField] float bounceDuration = 0.25f;
    [SerializeField] float wobbleOffset = 0.1f;
    [SerializeField] float wobbleDuration = 0.18f;

    [SerializeField] float fallDistance = 5f;
    [SerializeField] float fallDuration = 0.35f;

    Vector3 baseScale;
    Tween currentTween;
    public bool IsFinished { get; private set; }
    private bool HighNote = false;

    [Header("Audio")]
    [SerializeField] AudioSource audioSource;
    [SerializeField] List<AudioClip> leftSpawnClips = new();
    [SerializeField] List<AudioClip> rightSpawnClips = new();

    void Awake()
    {
        baseScale = transform.localScale;

        transform.localScale = baseScale * spawnScaleMultiplier;
        transform.DOScale(baseScale, spawnScaleDuration)
                 .SetEase(Ease.OutBack);

        PlayBounceAndWobble();
    }

    public void RefreshBasePosition()
    {
        if (IsFinished) return;

        if (currentTween != null)
            currentTween.Kill();

        PlayBounceAndWobble();
    }
    public void SetNotes(bool isLeft)
    {
        HighNote = isLeft;
        PlaySpawnSound();
    }

    void PlaySpawnSound()
    {
        if (audioSource == null) return;

        List<AudioClip> list = HighNote ? leftSpawnClips : rightSpawnClips;
        if (list == null || list.Count == 0) return;

        var clip = list[Random.Range(0, list.Count)];
        if (clip != null)
            audioSource.PlayOneShot(clip);
    }

    void PlayBounceAndWobble()
    {
        Vector3 targetPos = transform.position;
        Vector3 bouncePos = targetPos + new Vector3(0f, wobbleOffset * 1.5f, 0f);

        Vector3 startPos = transform.position;

        currentTween = DOTween.Sequence()
            .Append(transform.DOMove(bouncePos, bounceDuration)
                        .SetEase(Ease.OutBack)) 
            .Append(transform.DOMove(targetPos, bounceDuration * 0.8f)
                        .SetEase(Ease.InOutSine))
            .Append(transform.DOMove(targetPos + new Vector3(0f, wobbleOffset, 0f), wobbleDuration)
                        .SetEase(Ease.OutSine))
            .Append(transform.DOMove(targetPos, wobbleDuration)
                        .SetEase(Ease.InSine));
    }

    public void PlayHitAnimation()
    {
        if (IsFinished) return;
        IsFinished = true;

        if (currentTween != null)
            currentTween.Kill();

        var col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

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
