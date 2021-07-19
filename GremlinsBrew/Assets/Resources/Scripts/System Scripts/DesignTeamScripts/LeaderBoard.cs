using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LeaderBoard : MonoBehaviour
{
    private Transform entryContainer;
    private Transform entryTemplate;
    private List<Transform> LeaderBoardEntryTransformList;
    public GameObject leaderBoardEntryContainer;
    public GameObject leaderBoardEntryTemplate;
    public GameObject leaderBoardPodiumTemplate;
    public GameObject HealthPotion;
    public GameObject PoisonPotion;
    public GameObject LovePotion;
    public GameObject ManaPotion;
    public GameObject Day;
    public GameObject Gold;
    Highscores leaderBoard;
    public static int currentid = -1;

    private void Awake()
    {
        //Mannaul reset of LeaderBoard
        /*
         PlayerPrefs.SetString("LeaderBoardTable", "");
        */
        string jsonString = PlayerPrefs.GetString("LeaderBoardTable");
        leaderBoard = JsonUtility.FromJson<Highscores>(jsonString);
        Day.GetComponent<Text>().text = PlayerPrefs.GetInt("currentDay").ToString();
        Gold.GetComponent<Text>().text = PlayerPrefs.GetInt("currentGold").ToString();
        HealthPotion.GetComponent<Text>().text = PlayerPrefs.GetInt("Health").ToString();
        PoisonPotion.GetComponent<Text>().text = PlayerPrefs.GetInt("Poison").ToString();
        LovePotion.GetComponent<Text>().text = PlayerPrefs.GetInt("Love").ToString();
        ManaPotion.GetComponent<Text>().text = PlayerPrefs.GetInt("Mana").ToString();

        // Sort entry list by Score
        for (int i = 0; i < leaderBoard.leaderBoardEntryList.Count; i++)
        {
            for (int j = i + 1; j < leaderBoard.leaderBoardEntryList.Count; j++)
            {
                if (leaderBoard.leaderBoardEntryList[j].score > leaderBoard.leaderBoardEntryList[i].score)
                {
                    // Swap
                    HighscoreEntry tmp = leaderBoard.leaderBoardEntryList[i];
                    leaderBoard.leaderBoardEntryList[i] = leaderBoard.leaderBoardEntryList[j];
                    leaderBoard.leaderBoardEntryList[j] = tmp;
                }
            }
            leaderBoard.leaderBoardEntryList[i].rank = i+1;
        }

        entryContainer = leaderBoardEntryContainer.transform;
        entryTemplate = leaderBoardEntryTemplate.transform;
        entryTemplate.gameObject.SetActive(false);

        //Display LeaderBoard
        LeaderBoardEntryTransformList = new List<Transform>();
        if (leaderBoard.leaderBoardEntryList.Count < 10)
        {
            foreach (HighscoreEntry LeaderBoardEntry in leaderBoard.leaderBoardEntryList)
            {
                CreateLeaderBoardEntryTransform(LeaderBoardEntry, entryContainer, LeaderBoardEntryTransformList);
            }
        }
        else
        {
            bool check = true;
            for (int i = 0; i < 9; i++)
            {
                CreateLeaderBoardEntryTransform(leaderBoard.leaderBoardEntryList[i], entryContainer, LeaderBoardEntryTransformList);
            }
            Debug.Log("current ID: " + currentid);
            for (int i = 10; i < leaderBoard.leaderBoardEntryList.Count; i++)
            {
                Debug.Log("check: " + i);
                if (leaderBoard.leaderBoardEntryList[i].id == currentid)
                {
                    Debug.Log("found" + currentid);
                    CreateLeaderBoardEntryTransform(leaderBoard.leaderBoardEntryList[i], entryContainer, LeaderBoardEntryTransformList);
                    check = false;
                    break;
                }
            }
            if (check)
            {
                CreateLeaderBoardEntryTransform(leaderBoard.leaderBoardEntryList[9], entryContainer, LeaderBoardEntryTransformList);
            }
        }
    }

    public static void AddLeaderBoardEntry(int score, string player1, string player2)
    {
        // Create LeaderBoardEntry
        HighscoreEntry leaderBoardEntry = new HighscoreEntry { score = score, player1 = player1, player2 = player2 };

        // Load saved LeaderBoard
        string jsonString = PlayerPrefs.GetString("LeaderBoardTable");
        Highscores leaderBoard = JsonUtility.FromJson<Highscores>(jsonString);

        if (leaderBoard == null)
        {
            // There's no stored table, create a Table
            leaderBoard = new Highscores()
            {
                leaderBoardEntryList = new List<HighscoreEntry>()
            };
        }
        int newid = leaderBoard.leaderBoardEntryList.Count;
        leaderBoardEntry.id = newid;
        currentid = newid;
        // Add new entry to LeaderBoard
        leaderBoard.leaderBoardEntryList.Add(leaderBoardEntry);

        // Save updated Highscores to LeaderBoard
        string json = JsonUtility.ToJson(leaderBoard);
        PlayerPrefs.SetString("LeaderBoardTable", json);
        PlayerPrefs.Save();
    }

    private void CreateLeaderBoardEntryTransform(HighscoreEntry LeaderBoardEntry, Transform container, List<Transform> transformList)
    {
        Debug.Log("Creating Leaderboard Entery...");
        float templateHeight = 27.1f;
        //entryTransform.gameObject.SetActive(true);
        string player1 = LeaderBoardEntry.player1;
        string player2 = LeaderBoardEntry.player2;

        if (LeaderBoardEntry.rank == 1)
        {         
            Transform entryTransform = Instantiate(entryTemplate, container);
            RectTransform entryRectTransform = entryTransform.GetComponent<RectTransform>();
            entryRectTransform.anchoredPosition = new Vector2(0, -11 +61);
            //Set Rank
            entryTransform.Find("posText").transform.Translate(-24, 61-61, 0);
            entryTransform.Find("posText").GetComponent<Text>().text = LeaderBoardEntry.rank.ToString();
            //Set Score
            int score = LeaderBoardEntry.score;
            entryTransform.Find("scoreText").transform.Translate(24, 61-61, 0);
            entryTransform.Find("scoreText").GetComponent<Text>().text = score.ToString();
            //Set Name
            entryTransform.Find("p1Text").GetComponent<Text>().text = player1;
            entryTransform.Find("p2Text").GetComponent<Text>().text = player2;
            entryTransform.gameObject.SetActive(true);
            Debug.Log("Adding: Entry");
            transformList.Add(entryTransform);
            if (LeaderBoardEntry.id == currentid)
            {
                entryTransform.Find("posText").GetComponent<Text>().color = Color.green;
                entryTransform.Find("scoreText").GetComponent<Text>().color = Color.green;
                entryTransform.Find("p1Text").GetComponent<Text>().color = Color.green;
                entryTransform.Find("p2Text").GetComponent<Text>().color = Color.green;
            }
        }
        else if (LeaderBoardEntry.rank == 2)
        {
            Transform entryTransform = Instantiate(entryTemplate, container);
            RectTransform entryRectTransform = entryTransform.GetComponent<RectTransform>();
            entryRectTransform.anchoredPosition = new Vector2(0, -11 + 27);
            //Set Rank
            entryTransform.Find("posText").transform.Translate(-12, 27-27, 0);
            entryTransform.Find("posText").GetComponent<Text>().text = LeaderBoardEntry.rank.ToString();
            //Set Score
            int score = LeaderBoardEntry.score;
            entryTransform.Find("scoreText").transform.Translate(19, 27-27, 0);
            entryTransform.Find("scoreText").GetComponent<Text>().text = score.ToString();
            //Set Name
            entryTransform.Find("p1Text").GetComponent<Text>().text = player1;
            entryTransform.Find("p2Text").GetComponent<Text>().text = player2;
            entryTransform.gameObject.SetActive(true);
            Debug.Log("Adding: Entry");
            transformList.Add(entryTransform);
            if (LeaderBoardEntry.id == currentid)
            {
                entryTransform.Find("posText").GetComponent<Text>().color = Color.green;
                entryTransform.Find("scoreText").GetComponent<Text>().color = Color.green;
                entryTransform.Find("p1Text").GetComponent<Text>().color = Color.green;
                entryTransform.Find("p2Text").GetComponent<Text>().color = Color.green;
            }
        }
        else if (LeaderBoardEntry.rank < 10)
        {
            Transform entryTransform = Instantiate(entryTemplate, container);
            RectTransform entryRectTransform = entryTransform.GetComponent<RectTransform>();
            entryRectTransform.anchoredPosition = new Vector2(0, -11 - templateHeight * (transformList.Count-2));
            //Set Rank Text
            entryTransform.Find("posText").GetComponent<Text>().text = LeaderBoardEntry.rank.ToString();
            //Set Score Text
            int score = LeaderBoardEntry.score;
            entryTransform.Find("scoreText").GetComponent<Text>().text = score.ToString();
            //Set Name Text
            entryTransform.Find("p1Text").GetComponent<Text>().text = player1;
            entryTransform.Find("p2Text").GetComponent<Text>().text = player2;
            entryTransform.gameObject.SetActive(true);
            Debug.Log("Adding: Entry");
            transformList.Add(entryTransform);
            if (LeaderBoardEntry.id == currentid)
            {
                entryTransform.Find("posText").GetComponent<Text>().color = Color.green;
                entryTransform.Find("scoreText").GetComponent<Text>().color = Color.green;
                entryTransform.Find("p1Text").GetComponent<Text>().color = Color.green;
                entryTransform.Find("p2Text").GetComponent<Text>().color = Color.green;
            }
        }
        else
        {
            Transform entryTransform = Instantiate(entryTemplate, container);
            RectTransform entryRectTransform = entryTransform.GetComponent<RectTransform>();
            entryRectTransform.anchoredPosition = new Vector2(0, -11 - templateHeight * 7);
            //Set Rank Text
            entryTransform.Find("posText").GetComponent<Text>().text = LeaderBoardEntry.rank.ToString();
            //Set Score Text
            int score = LeaderBoardEntry.score;
            entryTransform.Find("scoreText").GetComponent<Text>().text = score.ToString();
            //Set Name Text
            entryTransform.Find("p1Text").GetComponent<Text>().text = player1;
            entryTransform.Find("p2Text").GetComponent<Text>().text = player2;
            entryTransform.gameObject.SetActive(true);
            Debug.Log("Adding: Entry");
            transformList.Add(entryTransform);
            if (LeaderBoardEntry.id == currentid)
            {
                entryTransform.Find("posText").GetComponent<Text>().color = Color.green;
                entryTransform.Find("scoreText").GetComponent<Text>().color = Color.green;
                entryTransform.Find("p1Text").GetComponent<Text>().color = Color.green;
                entryTransform.Find("p2Text").GetComponent<Text>().color = Color.green;
            }
        }

    }

    private class Highscores
    {
        public List<HighscoreEntry> leaderBoardEntryList;
    }
    /*
         A single High score entry
    */
    [System.Serializable]
    private class HighscoreEntry
    {
        public int id;
        public int rank;
        public int score;
        public string player1;
        public string player2;
    }
}
