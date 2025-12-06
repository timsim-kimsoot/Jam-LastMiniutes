using UnityEngine;
using DG.Tweening;
using System;

public class Coins : MonoBehaviour
{
    public event Action OnCaught;
    public event Action OnMissed;

    [SerializeField] float fountainForce = 4f;
    [SerializeField] float lifetime = 2f;

    private bool isCaught = false;

    void Start()
    {
        Rigidbody2D rb = GetComponent<Rigidbody2D>();

        Vector2 dir = new Vector2(UnityEngine.Random.Range(-0.7f, 0.7f), 1f).normalized;

        rb.AddForce(dir * fountainForce, ForceMode2D.Impulse);

        rb.AddTorque(UnityEngine.Random.Range(-100f, 100f));

        Invoke(nameof(TriggerMissed), lifetime);
    }

    void OnMouseDown()
    {
        if (isCaught) return;
        isCaught = true;
        OnCaught?.Invoke();
        CatchAnimation();
    }

    void CatchAnimation()
    {
        GetComponent<Collider2D>().enabled = false;
        DOTween.Kill(transform);
        transform.DOScale(0f, 0.3f).OnComplete(() => Destroy(gameObject));
    }

    void TriggerMissed()
    {
        if (isCaught) return;
        OnMissed?.Invoke();
        Destroy(gameObject);
    }
}
