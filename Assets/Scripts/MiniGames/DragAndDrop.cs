using System.Collections.Generic;
using UnityEngine;

public class DragAndDrop : MinigameBase
{
    [SerializeField] Transform itemsParent;
    [SerializeField] Transform goal;
    [SerializeField] float snapDistance = 2f;
    [SerializeField] int itemsToSpawn = 3;

    [SerializeField] List<GameObject> itemPrefabs;
    [SerializeField] List<Transform> spawnPoints;

    readonly List<GameObject> spawnedItems = new List<GameObject>();

    int itemsRemaining;
    bool gameStarted;

    private void Awake()
    {
        StartGame();
    }
    public override void StartGame()
    {
        gameStarted = true;
        Debug.Log("Start");

        foreach (var obj in spawnedItems)
        {
            if (obj != null)
                Destroy(obj);
        }
        spawnedItems.Clear();
        SpawnItems();
    }

    void SpawnItems()
    {
        itemsRemaining = Mathf.Min(itemsToSpawn, spawnPoints.Count);

        List<Transform> availablePoints = new List<Transform>(spawnPoints);

        for (int i = 0; i < itemsRemaining; i++)
        {
            int prefabIndex = Random.Range(0, itemPrefabs.Count);
            GameObject prefab = itemPrefabs[prefabIndex];

            int pointIndex = Random.Range(0, availablePoints.Count);
            Transform point = availablePoints[pointIndex];
            availablePoints.RemoveAt(pointIndex);

            GameObject item = Instantiate(
                prefab,
                point.position,
                Quaternion.identity,
                itemsParent
            );
            Debug.Log("Spawned");

            if (item.GetComponent<DragObject>() == null)
            {
                Debug.LogWarning($"STUPID! {item.name} has no DragObject component!");
            }

            spawnedItems.Add(item);
        }
    }

    void Update()
    {
        if (!gameStarted) return;

        for (int i = spawnedItems.Count - 1; i >= 0; i--)
        {
            GameObject item = spawnedItems[i];
            if (item == null)
            {
                spawnedItems.RemoveAt(i);
                continue;
            }

            float distToGoal = Vector3.Distance(item.transform.position, goal.position);

            if (distToGoal <= snapDistance)
            {
                item.transform.position = goal.position;

                var drag = item.GetComponent<DragObject>();
                if (drag != null)
                    drag.enabled = false;

                spawnedItems.RemoveAt(i);
                itemsRemaining--;

                if (itemsRemaining <= 0)
                {
                    gameStarted = false;
                    FinishGame();
                    break;
                }
            }
        }
    }
}
