using UnityEngine;

public class DragObject : MonoBehaviour
{
    private bool dragging = false;
    private Vector3 offset;

    void Update()
    {
        if (dragging)
        {
            transform.position = Camera.main.ScreenToWorldPoint(Input.mousePosition) + offset;
        }
    }

    private void OnMouseDown()
    {
        Debug.Log("down");
        offset = transform.position - Camera.main.ScreenToWorldPoint(Input.mousePosition);
        dragging = true;
    }

    private void OnMouseUp()
    {
        Debug.Log("up");
        dragging = false;
    }
}
