using UnityEngine;
using System;

public class OsuNote : MonoBehaviour
{
    public Action<OsuNote> OnHit;
    public Action<OsuNote> OnMiss;

    [SerializeField] float lifetime = 0.6f;

    float spawnTime;
    bool hit;

    void OnEnable()
    {
        spawnTime = Time.time;
    }

    void Update()
    {
        if (hit) return;

        if (Time.time - spawnTime >= lifetime)
        {
            hit = true;
            OnMiss?.Invoke(this);
            Destroy(gameObject);
        }
    }

    void OnMouseDown()
    {
        if (hit) return;

        hit = true;
        OnHit?.Invoke(this);
        Destroy(gameObject);
    }

    public void SetLifetime(float t)
    {
        lifetime = t;
    }
}
