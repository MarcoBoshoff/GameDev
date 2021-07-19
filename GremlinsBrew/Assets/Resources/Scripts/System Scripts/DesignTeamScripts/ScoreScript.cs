using System.Collections;
using RotaryHeart.Lib.SerializableDictionary;
using System.Collections.Generic;
using UnityEngine;

public class ScoreScript : MonoBehaviour
{
    public PotionCostDict PotionCost = new PotionCostDict();
    //fields
    private float _totalScore, dailyScore;
    float daysMod;
    int day;
    public Dictionary<PlayerScript, ScoreStats> highscoreStats = new Dictionary<PlayerScript, ScoreStats>(); //Initalizing dictionary before using it
    public Dictionary<ResourceType, int> potionsSold = new Dictionary<ResourceType, int>()
    {
        {ResourceType.HealthPotion, 0},
        {ResourceType.PoisonPotion, 0},
        {ResourceType.LovePotion, 0},
        {ResourceType.ManaPotion, 0}
    };
    public Dictionary<ResourceType, int> totalPotionsSold = new Dictionary<ResourceType, int>()
    {
        {ResourceType.HealthPotion, 0},
        {ResourceType.PoisonPotion, 0},
        {ResourceType.LovePotion, 0},
        {ResourceType.ManaPotion, 0}
    };


    // Init is called at Start of the game by Game Controller
    public void Init(PlayerScript p1, PlayerScript p2)
    {
        //Debug.Log("Scoring Initialised");
        day = 0;
        _totalScore = 0.0f;
        daysMod = 1.5f;
        highscoreStats.Add(p1, new ScoreStats());
        highscoreStats.Add(p2, new ScoreStats());
    }

    public float ResetDaily(PlayerScript p1, PlayerScript p2, int actPot, int dmgTaken)
    {
        day++;
        daysMod += 0.1f;
        //update totalpotions sold.
        UpdateTotalSold();

        //Calculate total score
        float result = Calculate(p1, p2, actPot, dmgTaken);
        if (result < 1)
        {
            result = 0;
        }
        _totalScore += result;

        //sold potions
        potionsSold[ResourceType.HealthPotion] = 0;
        potionsSold[ResourceType.PoisonPotion] = 0;
        potionsSold[ResourceType.LovePotion] = 0;
        potionsSold[ResourceType.ManaPotion] = 0;
        //Debug.Log("ResetDaily Complete");
        return _totalScore;
    }

    public void ResetPotionsMade()
    {
        foreach (KeyValuePair<PlayerScript, ScoreStats> scoreStat in highscoreStats)
        {
            highscoreStats[scoreStat.Key].ResetPotionsMade();
        }
    }

    private float Calculate(PlayerScript p1, PlayerScript p2, int actPotCount, int dmgTaken)
    {
        Debug.Log("Score Calculation Begin");
        float score = 0.0f;
        float potsMade = 0.0f;
        float potsSold = 0.0f;
        float activePoints = 100f;
        float lostPoints = 0.0f;

        //points for potions made
        potsMade = highscoreStats[p1].PotionsMadeCalculation() + highscoreStats[p2].PotionsMadeCalculation();

        //points for potions sold
        potsSold =  (potionsSold[ResourceType.HealthPotion] * 10) +
                    (potionsSold[ResourceType.PoisonPotion] * 20) +
                    (potionsSold[ResourceType.LovePotion] * 30) +
                    (potionsSold[ResourceType.ManaPotion] * 40);

        Debug.Log($"potsMade={potsMade} potsSold={potsSold}");

        //modifier for active potions
       if (actPotCount == 2)
        {
            activePoints *= 1.5f;
        }
        else if (actPotCount == 3)
        {
            activePoints *= 1.75f;
        }
        else if (actPotCount == 4)
        {
            activePoints *= 2f;
        }
        score = potsSold + potsMade + (activePoints * daysMod);

        Debug.Log($"Scorev1={score}");

        //points lost due to running out of stock
        if (GameControllerScript.StockLevel(ResourceType.HealthPotion) == 0) //Returns -1 if not unlocked
        {
            lostPoints += 50;
            Debug.Log("No health");
        }
        if (GameControllerScript.StockLevel(ResourceType.PoisonPotion) == 0)
        {
            lostPoints += 100;
            Debug.Log("No poison");
        }
        if (GameControllerScript.StockLevel(ResourceType.LovePotion) == 0)
        {
            lostPoints += 150;
        }
        if (GameControllerScript.StockLevel(ResourceType.ManaPotion) == 0)
        {
            lostPoints += 200;
        }
        lostPoints += dmgTaken * 300;

        Debug.Log($"lostPoints={lostPoints}");

        //score -= lostPoints;
        score *= 1 - (lostPoints / 1500); //Testing this slightly different penalty multiplier
        Debug.Log($"Scorev2={score}");

        if (score % 1 != 0)
        {
            score = Mathf.FloorToInt(score) + 1;
        }

        Debug.Log($"Scorev3={score}");

        //Debug.Log("Score Calculation Complete");
        return score;
    }

    public void WinScoreAdjustment(int pots) // pots is ammount of potions active
    {
        if (pots == 2)
        {
            totalScore += 400;
        }
        else if (pots == 3)
        {
            totalScore += 800;
        }
        else if (pots == 4)
        {
            totalScore += 1200;
        }
    }

    public void FinalScore(int currentGold, string name1, string name2, int damageTaken)
    {
        PlayerPrefs.SetInt("currentGold", currentGold);
        PlayerPrefs.SetInt("currentDay", day);
        SaveTotalSold();
        LeaderBoard.AddLeaderBoardEntry((int)_totalScore, name1, name2);
        //return _totalScore;
    }

    public void UpdateMade(PlayerScript p, ResourceType pot)
    {
        if (highscoreStats[p].potionsMade.ContainsKey(pot))
        {
            highscoreStats[p].potionsMade[pot]++;
        }
        else
        {
            //Debug.Log("Incorrect Resource Type");
        }
    }

    public void UpdateSold(ResourceType pot)
    {
        if (potionsSold.ContainsKey(pot))
        {
            potionsSold[pot]++;
        }
        else
        {
            //Debug.Log("Incorrect Resource Type");
        }
    }

    private void UpdateTotalSold()
    {
       // Debug.Log("UpdateTotalSold Start");
        totalPotionsSold[ResourceType.HealthPotion] += potionsSold[ResourceType.HealthPotion];
        totalPotionsSold[ResourceType.PoisonPotion] += potionsSold[ResourceType.PoisonPotion];
        totalPotionsSold[ResourceType.LovePotion] += potionsSold[ResourceType.LovePotion];
        totalPotionsSold[ResourceType.ManaPotion] += potionsSold[ResourceType.ManaPotion];
        //Debug.Log("UpdateTotalSold Complete");
    }

    private void SaveTotalSold()
    {
        PlayerPrefs.SetInt("Health", TotalMade(ResourceType.HealthPotion));
        PlayerPrefs.SetInt("Poison", TotalMade(ResourceType.PoisonPotion));
        PlayerPrefs.SetInt("Love", TotalMade(ResourceType.LovePotion));
        PlayerPrefs.SetInt("Mana", TotalMade(ResourceType.ManaPotion));
        PlayerPrefs.Save();
    }

    public float totalScore
    {
        get { return _totalScore; }
        set { _totalScore = value; }
    }

    public int TotalMade(ResourceType potion)
    {
        int totalMade = 0;
        foreach (KeyValuePair<PlayerScript, ScoreStats> scoreStat in highscoreStats)
        {
            totalMade += scoreStat.Value.potionsMade[potion];
        }

        return totalMade;
    }
}

//ScoreStats class
public class ScoreStats
{
    public Dictionary<ResourceType, int> potionsMade = new Dictionary<ResourceType, int>()
    {
        {ResourceType.HealthPotion, 0},
        {ResourceType.PoisonPotion, 0},
        {ResourceType.LovePotion, 0},
        {ResourceType.ManaPotion, 0}
    };

    public void ResetPotionsMade()
    {
        //Debug.Log("Reset Potions Made Begin");
        potionsMade[ResourceType.HealthPotion] = 0;
        potionsMade[ResourceType.PoisonPotion] = 0;
        potionsMade[ResourceType.LovePotion] = 0;
        potionsMade[ResourceType.ManaPotion] = 0;
        //Debug.Log("Reset Potions Made Complete");
    }

    public float PotionsMadeCalculation()
    {
        return potionsMade[ResourceType.HealthPotion] * 2 +
               potionsMade[ResourceType.PoisonPotion] * 4 +
               potionsMade[ResourceType.LovePotion] * 6 +
               potionsMade[ResourceType.ManaPotion] * 8;
    }


}

[System.Serializable]
public class PotionCostDict : SerializableDictionaryBase<ResourceType, int> { }
