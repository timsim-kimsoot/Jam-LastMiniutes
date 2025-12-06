using UnityEngine;

public class WobbleAndBreathMotion : MonoBehaviour
{
    [Header("Wobble Motion")]
    [SerializeField] float wobbleAmplitude = 0.05f;
    [SerializeField] float wobbleFrequency = 3f;

    [Header("Breath Motion")]
    [SerializeField] float breathAmplitude = 0.05f;
    [SerializeField] float breathFrequency = 1f;

    private Vector3 initialPosition;
    private Vector3 initialScale;

    void Start()
    {
        initialPosition = transform.localPosition;
        initialScale = transform.localScale;
    }

    void Update()
    {
        float wobbleOffset = Mathf.Sin(Time.time * wobbleFrequency) * wobbleAmplitude;
        float breathOffset = Mathf.Sin(Time.time * breathFrequency) * breathAmplitude;

        transform.localPosition = initialPosition + new Vector3(0f, wobbleOffset, 0f);
        transform.localScale = initialScale * (1f + breathOffset);
    }
}
