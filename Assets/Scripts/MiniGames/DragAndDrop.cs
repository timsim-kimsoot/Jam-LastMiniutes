using System.Collections.Generic;
using UnityEngine;

public class CoinsCollect : MinigameBase
{
    [Header("Refs")]
    [SerializeField] Transform itemsParent;
    [SerializeField] Transform goal;
    [SerializeField] float snapDistance = 2f;

    [Header("Items")]
    [SerializeField] List<GameObject> itemPrefabs;

    [Header("Spawn Count")]
    [SerializeField] int minSpawnCount = 3;
    [SerializeField] int maxSpawnCount = 8;

    [Header("Spawn Bounds")]
    [SerializeField] float minX = -7f;
    [SerializeField] float maxX = 7f;
    [SerializeField] float minY = -3f;
    [SerializeField] float maxY = 9f;

    readonly List<GameObject> spawnedItems = new List<GameObject>();

    int itemsRemaining;
    int itemsToSpawnThisRun;

    public override void Init(float difficulty)
    {
        base.Init(difficulty);

        spawnedItems.Clear();

        float tNorm = Mathf.InverseLerp(3f, 12f, timeLimit);
        float dNorm = Mathf.Clamp01(difficulty / 5f);
        float factor = Mathf.Clamp01(0.5f * tNorm + 0.5f * dNorm);

        int dynamicMax = Mathf.RoundToInt(Mathf.Lerp(minSpawnCount + 1, maxSpawnCount, factor));
        dynamicMax = Mathf.Clamp(dynamicMax, minSpawnCount, maxSpawnCount);

        itemsToSpawnThisRun = Random.Range(minSpawnCount, dynamicMax + 1);
        itemsRemaining = itemsToSpawnThisRun;

        SpawnItems();
    }

    void SpawnItems()
    {
        for (int i = 0; i < itemsToSpawnThisRun; i++)
        {
            if (itemPrefabs == null || itemPrefabs.Count == 0)
                break;

            int prefabIndex = Random.Range(0, itemPrefabs.Count);
            GameObject prefab = itemPrefabs[prefabIndex];

            float x = Random.Range(minX, maxX);
            float y = Random.Range(minY, maxY);
            float z = itemsParent != null ? itemsParent.position.z : 0f;

            Vector3 pos = new Vector3(x, y, z);
            Quaternion rot = Quaternion.Euler(0f, 0f, Random.Range(0f, 360f));

            GameObject item = Instantiate(prefab, pos, rot, itemsParent);

            var drag = item.GetComponent<DragObject>();
            if (drag == null)
                Debug.LogWarning($"STUPID! {item.name} has no DragObject component!");

            spawnedItems.Add(item);
        }
    }

    void Update()
    {
        base.Update();

        if (!running) return;

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
                    Win();
                    break;
                }
            }
        }
    }
}
