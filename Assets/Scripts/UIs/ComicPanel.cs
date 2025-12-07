using UnityEngine;
using DG.Tweening;

[System.Serializable]
public class ComicPanel
{
    public RectTransform panel;
    public float delay = 0f;
    public float duration = 0.5f;
    public Ease ease = Ease.OutQuad;
}
