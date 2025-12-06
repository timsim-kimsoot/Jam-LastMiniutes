using System.Collections.Generic;
using UnityEngine;

public class Bus_Gen : MonoBehaviour
{
    [Header("Crowd Settings")]
    [SerializeField] List<GameObject> crowdPrefabs = new();
    [SerializeField] float unitWidth = 1f;
    [SerializeField] int totalUnits = 20;
    [SerializeField] float yPosition = 0f;

    [Header("Gap Settings")]
    [SerializeField] int minGapWidth = 3;
    [SerializeField] int maxGapWidth = 5;

    public Vector2 GapRange => new Vector2(minGapWidth * unitWidth, maxGapWidth * unitWidth);

    public Vector3 SpawnWall()
    {
        float startX = -totalUnits / 2f * unitWidth;

        int gapWidth = Random.Range(minGapWidth, maxGapWidth + 1);
        int gapStartIndex = Random.Range(0, totalUnits - gapWidth);

        for (int i = 0; i < totalUnits; i++)
        {
            if (i >= gapStartIndex && i < gapStartIndex + gapWidth)
                continue;

            var prefab = crowdPrefabs[Random.Range(0, crowdPrefabs.Count)];
            Vector3 pos = new Vector3(startX + i * unitWidth, yPosition, 0f);
            Instantiate(prefab, pos, Quaternion.identity, transform);
        }

        float gapCenterX = startX + (gapStartIndex + gapWidth / 2f) * unitWidth;
        return new Vector3(gapCenterX, yPosition, 0f);
    }
}