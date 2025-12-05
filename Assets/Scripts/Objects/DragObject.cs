using UnityEngine;
using DG.Tweening;

public class DragObject : MonoBehaviour
{
    [Header("Drag")]
    private bool dragging = false;
    private Vector3 offset;
    private Vector3 lastPos;

    [Header("Sway")]
    [SerializeField] float maxTilt = 45f;
    [SerializeField] float tiltDuration = 0.1f;
    [SerializeField] float resetDuration = 0.2f;

    void Update()
    {
        if (dragging)
        {
            Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mouseWorld.z = transform.position.z;

            Vector3 newPos = mouseWorld + offset;
            Vector3 delta = newPos - lastPos;

            transform.position = newPos;

            if (delta.sqrMagnitude > 0.00001f)
            {
                float targetZ = Mathf.Clamp(-delta.x * maxTilt, -maxTilt, maxTilt);

                transform.DOKill(false);
                transform.DORotate(new Vector3(0f, 0f, targetZ), tiltDuration)
                         .SetEase(Ease.OutQuad);
            }

            lastPos = newPos;
        }
        else
        {
            transform.DOKill(false);
            transform.DORotate(Vector3.zero, resetDuration)
                     .SetEase(Ease.OutQuad);
        }
    }

    private void OnMouseDown()
    {
        Debug.Log("down");

        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorld.z = transform.position.z;

        offset = transform.position - mouseWorld;
        dragging = true;
        lastPos = transform.position;
    }

    private void OnMouseUp()
    {
        Debug.Log("up");
        dragging = false;
    }
}
