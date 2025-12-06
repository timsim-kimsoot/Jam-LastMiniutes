using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Printer : MinigameBase
{
    [Header("Refs")]
    [SerializeField] Transform PowerButton;
    [SerializeField] GameObject SmashPrefab;

    readonly List<GameObject> spawnedItems = new List<GameObject>();

    int itemsRemaining;
    public override void Init(float difficulty)
    {
        base.Init(difficulty);

        spawnedItems.Clear();

        SpawnItems();
    }

    void SpawnItems()
    {
       
    }

    void Update()
    {
        base.Update();

        if (!running) return;
    }
}
