using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoinCounterScript : MonoBehaviour
{
    private GameObject coinParent;
    private GameObject coinCountedPrefab;

    private GameObject spendParticle; //Particle for spending coins

    private int coinsToSpawn = 0, timer = 0;

    private Vector3 startHeight, targetHeight;
    private readonly float heightScale = 0.5f;

    // Start is called before the first frame update
    void Start()
    {
        coinCountedPrefab = Resources.Load("Prefabs/CoinCountedPrefab") as GameObject;
        spendParticle = Resources.Load("Prefabs/Particles/CoinSpendParticle") as GameObject;

        startHeight = transform.position;
        targetHeight = startHeight;
    }

    void Update()
    {
        if (timer <= 0)
        {
            //Spawns coins on an interval
            if (coinsToSpawn > 0)
            {
                coinsToSpawn--;
                NewCoinCount();
                timer = 20;

            }
        }
        else
        {
            timer--;
        }

        //Move the spawner up for the more coins there are
        if (transform.position != targetHeight)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetHeight, 0.1f);
        }
    }

    public int CoinsToSpawn
    {
        set
        {
            coinsToSpawn = value;
        }

        get
        {
            return coinsToSpawn;
        }
    }

    //Remove coins and show the spend particle
    public void SpendCoins(int amount)
    {
        for (int i = 0; i < amount; i++)
        {
            GameObject coin = GameControllerScript.local.Coins[0], particle = Instantiate(spendParticle);

            // Coin spend particle
            particle.transform.position = coin.transform.position;
            Destroy(particle, 4f);

            //Destory the coin object
            Destroy(coin, 0.2f);

            //Remove from the list
            GameControllerScript.local.Coins.RemoveAt(0);
        }

        CalculateHeight(GameControllerScript.local.Coins.Count);
    }

    public void NewCoinCount()
    {
        if (coinParent == null)
        {
            coinParent = GameObject.FindGameObjectWithTag("CoinEconomy");
        }

        GameObject coin = Instantiate(coinCountedPrefab, transform.position, transform.rotation);
        coin.transform.SetParent(coinParent.transform);

        GameControllerScript.local.Coins.Add(coin);

        transform.Translate(Vector3.up * heightScale);

        CalculateHeight(GameControllerScript.local.Coins.Count);
    }

    private void CalculateHeight(int totalCoins)
    {
        targetHeight = startHeight + Vector3.up * totalCoins * heightScale;
    }


}
