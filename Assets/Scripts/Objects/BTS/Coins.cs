using UnityEngine;
using DG.Tweening;
using System;

public class Coins : MonoBehaviour
{
    public event Action OnCaught;
    public event Action OnMissed;

    [SerializeField] float fallForce = 4f;
    [SerializeField] float lifetime = 2f;

    [SerializeField] public int value;

    bool isCaught;

    void Start()
    {
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        rb.velocity = Vector2.zero;
        rb.angularVelocity = 0f;

        Vector2 dir = new Vector2(UnityEngine.Random.Range(-0.7f, 0.7f), -1f).normalized;
        rb.AddForce(dir * fallForce, ForceMode2D.Impulse);
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
        var col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

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
