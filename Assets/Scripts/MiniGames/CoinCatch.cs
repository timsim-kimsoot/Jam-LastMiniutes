using System.Collections;
using UnityEngine;

public class CoinCatch : MinigameBase
{
    [SerializeField] GameObject coinPrefab;
    [SerializeField] int coinsToCatch = 3;
    [SerializeField] float spawnDelay = 0.5f;
    [SerializeField] Transform spawnPoint;

    int coinsCaught = 0;

    public override void Init(float difficulty)
    {
        base.Init(difficulty);
        StartCoroutine(SpawnCoins());
    }

    IEnumerator SpawnCoins()
    {
        for (int i = 0; i < coinsToCatch + 3; i++)
        {
            SpawnCoin();
            yield return new WaitForSeconds(spawnDelay);
        }
    }

    void SpawnCoin()
    {
        GameObject coin = Instantiate(coinPrefab, spawnPoint.position, Quaternion.identity);
        Coins coinScript = coin.GetComponent<Coins>();
        coinScript.OnCaught += HandleCoinCaught;
        coinScript.OnMissed += HandleCoinMissed;
    }

    void HandleCoinCaught()
    {
        coinsCaught++;
        if (coinsCaught >= coinsToCatch)
        {
            Debug.Log("RICH!!");
            Win();
        }
    }

    void HandleCoinMissed()
    {
        Debug.Log("Missed Lmao!");
    }
}
