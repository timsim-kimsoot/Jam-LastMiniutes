using UnityEngine;

public class DragAndDrop : MonoBehaviour
{
    [SerializeField] GameObject draggedObject;          
    [SerializeField] GameObject goal;     
    [SerializeField] float snapDistance = 0.5f;


    void Update()
    {
        float distToGoal = Vector3.Distance(draggedObject.transform.position, goal.transform.position);
        Debug.Log(distToGoal);

        if (snapDistance > distToGoal )
        {
            Debug.Log("Snap");
            draggedObject.transform.position = goal.transform.position;
        }
    }
}
