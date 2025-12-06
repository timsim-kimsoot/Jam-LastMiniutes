using UnityEngine;

public class Bus_Crowd : MonoBehaviour
{
    public float speed = 3f;
    public float range = 5f;
    private bool moving = false;
    private Vector3 startPos;
    public float Width => GetComponent<SpriteRenderer>().bounds.size.x;

    void Start()
    {
        startPos = transform.position;
    }

    void Update()
    {
        if (!moving) return;

        float offset = Mathf.Sin(Time.time * speed) * range;
        transform.position = startPos + new Vector3(offset, 0f, 0f);
    }

    public void StartMoving() => moving = true;
    public void StopMoving() => moving = false;
}
