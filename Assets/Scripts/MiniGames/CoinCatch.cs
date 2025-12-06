using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoinCatch : MinigameBase
{
    [SerializeField] List<GameObject> coinPrefabs;
    [SerializeField] List<float> coinWeights;
    [SerializeField] Wallet wallet;
    [SerializeField] Transform spawnPoint;
    [SerializeField] int baseMaxCoinsToSpawn = 8;
    [SerializeField] float spawnDelay = 0.5f;

    int currentTotalValue;
    int targetTotalValue;
    int maxCoinsToSpawn;

    public override void Init(float difficulty)
    {
        base.Init(difficulty);

        float d = Mathf.Clamp01(difficulty / 4f);
        int minTotal = Mathf.RoundToInt(Mathf.Lerp(3f, 6f, d));
        int maxTotal = Mathf.RoundToInt(Mathf.Lerp(8f, 12f, d));
        targetTotalValue = Random.Range(minTotal, maxTotal + 1);

        maxCoinsToSpawn = baseMaxCoinsToSpawn + Mathf.RoundToInt(difficulty * 2f);

        currentTotalValue = 0;

        StartCoroutine(SpawnCoins());
    }

    IEnumerator SpawnCoins()
    {
        for (int i = 0; i < maxCoinsToSpawn; i++)
        {
            SpawnCoin();
            if (wallet != null)
                wallet.Shake();
            yield return new WaitForSeconds(spawnDelay);
        }
    }

    void SpawnCoin()
    {
        GameObject prefab = GetRandomCoinPrefab();
        if (prefab == null) return;

        GameObject coinObj = Object.Instantiate(
                   prefab,
                   spawnPoint.position,
                   Quaternion.identity,
                   spawnPoint
               );
        Coins coin = coinObj.GetComponent<Coins>();
        if (coin == null) return;

        coin.OnCaught += () => HandleCoinCaught(coin.value);
        coin.OnMissed += HandleCoinMissed;
    }

    GameObject GetRandomCoinPrefab()
    {
        if (coinPrefabs == null || coinPrefabs.Count == 0)
            return null;

        if (coinWeights == null || coinWeights.Count != coinPrefabs.Count)
            return coinPrefabs[Random.Range(0, coinPrefabs.Count)];

        float total = 0f;
        for (int i = 0; i < coinWeights.Count; i++)
        {
            float w = Mathf.Max(coinWeights[i], 0f);
            total += w;
        }

        if (total <= 0f)
            return coinPrefabs[Random.Range(0, coinPrefabs.Count)];

        float r = Random.value * total;
        float accum = 0f;

        for (int i = 0; i < coinPrefabs.Count; i++)
        {
            float w = Mathf.Max(coinWeights[i], 0f);
            accum += w;
            if (r <= accum)
                return coinPrefabs[i];
        }

        return coinPrefabs[coinPrefabs.Count - 1];
    }

    void HandleCoinCaught(int coinValue)
    {
        currentTotalValue += coinValue;

        if (currentTotalValue >= targetTotalValue)
            Win();
    }

    void HandleCoinMissed()
    {
    }
}