using System.Collections;
using System.Collections.Generic;
using RotaryHeart.Lib.SerializableDictionary;
using UnityEngine.UI;
using UnityEngine;
using InControl;

public class NightMenuScript : MonoBehaviour
{
    public PotionSpriteDict ButtonBackgrond = new PotionSpriteDict();

    [SerializeField]
    private GameObject DaysCount;
    private float[] y = new float[2], JoyStickMenuDelay = new float[2], MenuDelayMax = new float[2] { 0.25f, 0.25f };  
    private bool[] FirstJoystickMove = new bool[2] { true, true };

    public List<Transform> CatalogButtons = new List<Transform>();
    private RectTransform[] Cursors = new RectTransform[2];
    private Vector3 CursorOGScale, CursorLargeScale;

    [SerializeField]
    private bool[] PlayerSelect = new bool[2], CursorMove = new bool[2] { false, false };
    private bool[] DPADDown = new bool[2] { false, false }, DPADUp = new bool[2] { false, false }, cancelB = new bool[2] { false, false }, AButtonPressed = new bool[2] { false, false };

    public int[] buttonSelected = new int[2] { 0, 0 };
    private buttonScoreBehaviour Button;

    public GameObject PlayerSelectionCursor;
    public GameObject CoinCounter, TotalPotionLabel, HealthPotionLabel, PoisonPotionLabel, LovePotionLabel, ManaPotionLabel;

    public float CursorSpeed = 1f;

    int storedScore = 0;

    // Start is called before the first frame update
    void Start()
    {
        PlayerSelect[0] = false;
        PlayerSelect[1] = false;

        //Cursor Creation
        Cursors[0] = Instantiate(PlayerSelectionCursor).GetComponent<RectTransform>();
        Cursors[1] = Instantiate(PlayerSelectionCursor).GetComponent<RectTransform>();

        Cursors[0].transform.SetParent(this.transform, false);
        Cursors[1].transform.SetParent(this.transform, false);

        CursorOGScale = Cursors[0].transform.localScale;
        CursorLargeScale = new Vector3(1.5f, 1.5f, 1.5f);

        Cursors[0].transform.GetChild(0).GetComponent<Image>().color = (GameControllerScript.PlayerColors != null) ? GameControllerScript.PlayerColors[0] : Color.red;
        Cursors[1].transform.GetChild(0).GetComponent<Image>().color = (GameControllerScript.PlayerColors != null) ? GameControllerScript.PlayerColors[1] : Color.cyan;

        Cursors[0].position = CatalogButtons[0].GetComponent<buttonScoreBehaviour>().IconLocation[0].position;
        Cursors[1].position = CatalogButtons[0].GetComponent<buttonScoreBehaviour>().IconLocation[1].position;

        Cursors[0].transform.rotation = CatalogButtons[0].GetComponent<buttonScoreBehaviour>().IconLocation[0].rotation;
        Cursors[1].transform.rotation = CatalogButtons[0].GetComponent<buttonScoreBehaviour>().IconLocation[1].rotation;
    }

    public void Update()
    {
        GetButtons();
        MenuControls();
        float step = CursorSpeed * Time.deltaTime; // calculate distance to move
        for (int p = 0; p < 2; p++)
        {
            if (CursorMove[p] == true)
            {
                Cursors[p].position = Vector3.MoveTowards(Cursors[p].position, CatalogButtons[buttonSelected[p]].GetComponent<buttonScoreBehaviour>().IconLocation[p].position, step);
                if (Cursors[p].position == CatalogButtons[buttonSelected[p]].GetComponent<buttonScoreBehaviour>().IconLocation[p].position)
                {
                    CursorMove[p] = false;
                }
            }
            else
            {
                Cursors[p].position = CatalogButtons[buttonSelected[p]].GetComponent<buttonScoreBehaviour>().IconLocation[p].position;
            }
        }

        ScoreScript Score = GameControllerScript.local.GetComponent<ScoreScript>();
        CoinCounter.GetComponent<Text>().text = GameControllerScript.local.Coins.Count.ToString();

        HealthPotionLabel.GetComponent<Text>().text = Mathf.Max(0, GameControllerScript.StockLevel(ResourceType.HealthPotion)).ToString();
        PoisonPotionLabel.GetComponent<Text>().text = Mathf.Max(0, GameControllerScript.StockLevel(ResourceType.PoisonPotion)).ToString();
        LovePotionLabel.GetComponent<Text>().text = Mathf.Max(0, GameControllerScript.StockLevel(ResourceType.LovePotion)).ToString();
        ManaPotionLabel.GetComponent<Text>().text = Mathf.Max(0, GameControllerScript.StockLevel(ResourceType.ManaPotion)).ToString();

        //Increase display score
        storedScore = (int)Mathf.MoveTowards(storedScore, GameControllerScript.dailyScore, 8f);
        TotalPotionLabel.GetComponent<Text>().text = storedScore.ToString();
    }

    public void UpdatePotionIcons()
    {
        DaysCount.GetComponent<Text>().text = (GameControllerScript.DayNight.currentDay+1).ToString();

        List<ResourceType> PotionList = GameControllerScript.local.currentUnlockedPotion;

        ButtonBackgrond[ResourceType.HealthPotion].UnlockedPotion.SetActive((PotionList.Contains(ResourceType.HealthPotion)));
        ButtonBackgrond[ResourceType.HealthPotion].DisabledPotion.SetActive(!(PotionList.Contains(ResourceType.HealthPotion)));

        ButtonBackgrond[ResourceType.PoisonPotion].UnlockedPotion.SetActive(PotionList.Contains(ResourceType.PoisonPotion));
        ButtonBackgrond[ResourceType.PoisonPotion].DisabledPotion.SetActive(!PotionList.Contains(ResourceType.PoisonPotion));

        ButtonBackgrond[ResourceType.LovePotion].UnlockedPotion.SetActive(PotionList.Contains(ResourceType.LovePotion));
        ButtonBackgrond[ResourceType.LovePotion].DisabledPotion.SetActive(!PotionList.Contains(ResourceType.LovePotion));

        ButtonBackgrond[ResourceType.ManaPotion].UnlockedPotion.SetActive(PotionList.Contains(ResourceType.ManaPotion));
        ButtonBackgrond[ResourceType.ManaPotion].DisabledPotion.SetActive(!PotionList.Contains(ResourceType.ManaPotion));
    }

    void GetButtons()
    {
        for (int i = 0; i < 2; i++)
        {
            DPADDown[i] = (i == 0) ? Input.GetKeyDown(KeyCode.S) : Input.GetKeyDown(KeyCode.DownArrow);
            DPADUp[i] = (i == 0) ? Input.GetKeyDown(KeyCode.W) : Input.GetKeyDown(KeyCode.UpArrow);
            AButtonPressed[i] = (i == 0) ? Input.GetButtonDown("A1") : Input.GetButtonDown("A2");

            DPADDown[i] = (InputManager.Devices.Count <= i) ? DPADDown[i] : InputManager.Devices[i].DPadDown.WasPressed;
            DPADUp[i] = (InputManager.Devices.Count <= i) ? DPADUp[i] : InputManager.Devices[i].DPadUp.WasPressed;
            AButtonPressed[i] = (InputManager.Devices.Count <= i) ? AButtonPressed[i] : InputManager.Devices[i].Action1.WasPressed;

            if (InputManager.Devices.Count > i)
            {
                y[i] = InputManager.Devices[i].LeftStickY;
            }
            else
            {
                y[i] = 0;
            }

            if (y[i] >= 0.5 || y[i] <= -0.5)
            {
                if (JoyStickMenuDelay[i] < MenuDelayMax[i])
                {
                    JoyStickMenuDelay[i] += 1 * Time.deltaTime;
                }
            }
        }
    }

    public void ResetMenuStick(int i)
    {
        if (y[i] > -0.5 && y[i] < 0.5)
        {
            FirstJoystickMove[i] = true;
        }
    }

    //controller functions for navigating through menus/interactiing with menus
    void MenuControls()
    {
        for (int i = 0; i < 2; i++)
        {
            if (DPADDown[i] || (y[i] <= -0.5 && (JoyStickMenuDelay[i] >= MenuDelayMax[i] || FirstJoystickMove[i] == true)))
            {
                buttonSelected[i] = (buttonSelected[i] < CatalogButtons.Count - 1) ? (buttonSelected[i] + 1) : 0;
                FirstJoystickMove[i] = false;
                JoyStickMenuDelay[i] = 0;

                ResetMenuStick(i);

                FMOD_ControlScript.ChangeSelection(); // plays change selection sfx

                Cursors[i].transform.localScale = CursorOGScale;
                PlayerSelect[i] = false;
                CursorMove[i] = true;

            }
            else if ((DPADUp[i]) || (y[i] >= 0.5 && (JoyStickMenuDelay[i] >= MenuDelayMax[i] || FirstJoystickMove[i] == true)))
            {

                FirstJoystickMove[i] = false;
                JoyStickMenuDelay[i] = 0;
                buttonSelected[i] = (buttonSelected[i] > 0) ? (buttonSelected[i] - 1) : (CatalogButtons.Count - 1);

                ResetMenuStick(i);

                FMOD_ControlScript.ChangeSelection(); // plays change selection sfx

                Cursors[i].transform.localScale = CursorOGScale;
                PlayerSelect[i] = false;
                CursorMove[i] = true;
            }
            if (AButtonPressed[i])
            {
                MenuSelect(CatalogButtons[buttonSelected[i]], i);
                FMOD_ControlScript.ChangeSelection();
            }
        }
    }

    public void MenuSelect(Transform SelectedButton, int index)
    {
        //Button = SelectedButton.GetComponentInChildren<buttonScoreBehaviour>();

        PlayerSelect[index] = !(PlayerSelect[index]);
        Cursors[index].transform.localScale = (PlayerSelect[index]) ? CursorLargeScale : CursorOGScale;


        if ((PlayerSelect[0] == true && PlayerSelect[1] == true) && (buttonSelected[0] == buttonSelected[1]))
        {
            switch (buttonSelected[0])
            {
                case 0:
                    {
                        Cursors[0].transform.localScale = CursorOGScale;
                        Cursors[1].transform.localScale = CursorOGScale;
                        PlayerSelect[0] = false;
                        PlayerSelect[1] = false;
                        BuyMenu();
                        
                        break;
                    }
                case 1:
                    {
                        Cursors[0].transform.localScale = CursorOGScale;
                        Cursors[1].transform.localScale = CursorOGScale;
                        PlayerSelect[0] = false;
                        PlayerSelect[1] = false;
                        //CursorMove[0] = true;
                        //CursorMove[1] = true;
                        NextDay();
                        break;
                    }
                case 2:
                    {
                        Cursors[0].transform.localScale = CursorOGScale;
                        Cursors[1].transform.localScale = CursorOGScale;
                        PlayerSelect[0] = false;
                        PlayerSelect[1] = false;
                        //CursorMove[0] = true;
                        //CursorMove[1] = true;
                        EndSession();
                        break;
                    }
            }

            FMOD_ControlScript.ConfirmSelection();
        }
    }


    public void NextDay()
    {

        GameControllerScript.ClearItems();
        gameObject.SetActive(false);

        //TutorialScript.NewTutorial(TutorialType.Night);

        GameControllerScript.local.GetComponent<ScoreScript>().ResetPotionsMade();
        GameControllerScript.DayNight.ResetToDay();
    }

    public void BuyMenu()
    {
        BuySelection _MenuUI = GameControllerScript.local.BUYMENU;
        //Can only open at night. Else, close menu
        gameObject.SetActive(false);
        _MenuUI.gameObject.SetActive(true);

        if (_MenuUI.gameObject.activeSelf == true)
        {
            TutorialScript.Trigger(TutorialTrigger.OpenBuyMenu);
        }
        
    }

    public void EndSession()
    {
        GameControllerScript.Lose();
    }

    [System.Serializable]
    public class PotionStruct
    {
        public GameObject UnlockedPotion, DisabledPotion;
    }
    [System.Serializable]
    public class PotionSpriteDict : SerializableDictionaryBase<ResourceType, PotionStruct> { }
}
