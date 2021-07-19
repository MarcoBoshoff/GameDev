using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using InControl;

public class CharacterSelectionScript : MonoBehaviour
{
    public MainMenuScript mainMenu;

    public CustomizeCosumeScript[] p1Costumes;
    public CustomizeCosumeScript[] p2Costumes;
    public CustomizeCosumeScript[][] pDisplays;
    public Image[] ConfirmButton1 = new Image[2];
    public Image[] ConfirmButton2 = new Image[2];
    public Animator[] animators;

    public Color[] colorArray;
    public Material[] colouredMatArray;
    public Image[] colorIcons1, colorIcons2;
    public Image[][] colorIcons;
    public Sprite[] sprButton = new Sprite[2];

    public GameObject[] confirmButtons;

    public GameObject[] arrows1, arrows2;
    private GameObject[][] arrows;

    public Text[] namesArray;

    private int[][] selections;
    private int[] selected;
    private int[] lengths;
    //public RectTransform[] cursors;

    [SerializeField]
    private float[] x = new float[2], y = new float[2], JoyStickMenuDelayx = new float[2], JoyStickMenuDelayy = new float[2], MenuDelayMax = new float[2] { 0.25f , 0.25f };
    private bool[] FirstJoystickMoveX = new bool[2] { true, true }, FirstJoystickMoveY = new bool[2] { true, true };

    public float countdown = 0f;

    [FMODUnity.EventRef]
    public string startGameEventPath;

    //Naughty words said by Immature Dumbarses
    private List<string> bannedList = new List<string>() { "ASS", "FUK", "FUC", "TIT", "VAG", "DIC", "CNT", "SEX", "FU ", " FU", "FUU", "NIG", "FAG","GER","GA ", "GOT", "DIK","WTF"};

    // Start is called before the first frame update
    void Start()
    {
        //Array of both player's selections
        selections = new int[2][];
        selections[0] = new int[3]; //Player 1 - Outfits, Colours, Names
        selections[1] = new int[3]; //Player 2 - Outfits, Colours, Names

        lengths = new int[3];
        lengths[0] = p1Costumes.Length;
        lengths[1] = colorArray.Length;
        lengths[2] = 3; //Length of text

        selected = new int[2];
        selected[0] = 0;
        selected[1] = 0;

        //Array of arrow arrays
        arrows = new GameObject[2][];
        arrows[0] = arrows1;
        arrows[1] = arrows2;

        colorIcons = new Image[2][];
        colorIcons[0] = colorIcons1;
        colorIcons[1] = colorIcons2;

        pDisplays = new CustomizeCosumeScript[2][];
        pDisplays[0] = p1Costumes;
        pDisplays[1] = p2Costumes;

        //Preset colours
        if (GameControllerScript.PlayerColors == null)
        {
            GameControllerScript.PlayerColors = new Color[2];
            GameControllerScript.PlayerColors[0] = colorArray[0];
            GameControllerScript.PlayerColors[1] = colorArray[4];
        }

        //Preset outfits;
        if (GameControllerScript.PlayerOutfits == null)
        {
            GameControllerScript.PlayerOutfits = new int[2];
            GameControllerScript.PlayerOutfits[0] = 0;
            GameControllerScript.PlayerOutfits[1] = 0;

            GameControllerScript.playerCostumeMat = new Material[2];
            GameControllerScript.playerCostumeMat[0] = colouredMatArray[0];
            GameControllerScript.playerCostumeMat[1] = colouredMatArray[4];
        }

        if (GameControllerScript.PlayerNames == null)
        {
            GameControllerScript.PlayerNames = new string[2];
            GameControllerScript.PlayerNames[0] = "AAA";
            GameControllerScript.PlayerNames[1] = "ZZZ";
        }
        UpdateAName(0, 0);
        UpdateAName(1, 0);

        for (int p_num = 0; p_num < 2; p_num++)
        {
            selections[p_num][0] = 0; //Costume

            //Colour
            selections[p_num][1] = 0;
            for (int i = 0; i < colorArray.Length; i++)
            {
                Color c = colorArray[i];
                if (GameControllerScript.PlayerColors[p_num] == c) {
                    selections[p_num][1] = i;
                    break;
                }
            }

            //Name index
            selections[p_num][2] = 0;

            UpdateDisplay(p_num, selected[p_num]);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (InputManager.Devices != null)
        {
            for (int i = 0; i < InputManager.Devices.Count; i++)
            {
                InputDevice pDevice = InputManager.Devices[i];

                x[i] = pDevice.LeftStickX;
                y[i] = pDevice.LeftStickY;

                
                if (x[i] >= 0.5 || x[i] <= -0.5)
                {
                    if (JoyStickMenuDelayx[i] < MenuDelayMax[i])
                    {
                        JoyStickMenuDelayx[i] += 1 * Time.deltaTime;
                    }
                }
                if (y[i] >= 0.5 || y[i] <= -0.5)
                {
                    if (JoyStickMenuDelayy[i] < MenuDelayMax[i])
                    {
                        JoyStickMenuDelayy[i] += 1 * Time.deltaTime;
                    }
                }

                if (selected[i] < 3)
                {

                    int _s = selected[i];
                    bool XThumbstickDelay = (JoyStickMenuDelayx[i] >= MenuDelayMax[i] || FirstJoystickMoveX[i] == true);
                    bool YThumbstickDelay = (JoyStickMenuDelayy[i] >= MenuDelayMax[i] || FirstJoystickMoveY[i] == true);
                    //Right
                    if (pDevice.DPadRight.WasPressed || (x[i] >= 0.5 && XThumbstickDelay))
                    {
                        FirstJoystickMoveX[i] = false;
                        JoyStickMenuDelayx[i] = 0;
                        selections[i][_s]++;
                        while (selections[i][_s] >= lengths[_s])
                        {
                            selections[i][_s] -= lengths[_s];
                        }

                        UpdateDisplay(i, selected[i]);

                        FMOD_ControlScript.ChangeSelection(); // change selection sfx
                    }
                    //Left
                    else if (pDevice.DPadLeft.WasPressed || (x[i] <= -0.5 && XThumbstickDelay))
                    {
                        FirstJoystickMoveX[i] = false;
                        JoyStickMenuDelayx[i] = 0;
                        selections[i][_s]--;
                        while (selections[i][_s] < 0)
                        {
                            selections[i][_s] += lengths[_s];
                        }

                        UpdateDisplay(i, selected[i]);

                        FMOD_ControlScript.ChangeSelection(); // change selection sfx
                    }
                    else
                    {
                        if (x[i] > -0.5 && x[i] < 0.5)
                        {
                            FirstJoystickMoveX[i] = true;
                        }

                    }

                    if (_s == 2)
                    {
                        if (pDevice.DPadDown.WasPressed || (y[i] >= 0.5 && YThumbstickDelay))
                        {
                            FirstJoystickMoveY[i] = false;
                            JoyStickMenuDelayy[i] = 0;

                            UpdateAName(i, -1);

                            FMOD_ControlScript.ChangeSelection(); // change selection sfx
                        }
                        else if (pDevice.DPadUp.WasPressed || (y[i] <= -0.5 && YThumbstickDelay))
                        {
                            FirstJoystickMoveY[i] = false;
                            JoyStickMenuDelayy[i] = 0;

                            UpdateAName(i, 1);

                            FMOD_ControlScript.ChangeSelection(); // change selection sfx
                        }
                        if (y[i] > -0.5 && y[i] < 0.5)
                        {
                            FirstJoystickMoveY[i] = true;
                        }
                    }

                    //Select (A key)
                    if (pDevice.Action1.WasPressed)
                    {
                        if (selected[i] != 1 || selected [Mathf.Abs(i-1)] <= 1 || selections[0][1] != selections[1][1])
                        {
                            selected[i] = (int)Mathf.MoveTowards(selected[i], 3, 1);
                            if (selected[i] == 3)
                            {
                                //cursors[0].gameObject.SetActive(false);
                                animators[i].SetBool("IsDancing", true);

                                confirmButtons[i].SetActive(true);
                            }
                            UpdateDisplay(i, selected[i]);

                            FMOD_ControlScript.ConfirmSelection();
                        }
                    }
                }
                //Go back (B)
                if (pDevice.Action2.WasPressed)
                {
                    if (selected[i] > 0)
                    {
                        selected[i] = (int)Mathf.MoveTowards(selected[i], 0, 1); ;
                        if (selected[i] == 2)
                        {
                            //cursors[0].gameObject.SetActive(true);
                            animators[i].SetBool("IsDancing", false);
                            confirmButtons[i].SetActive(false);
                        }
                        UpdateDisplay(i, selected[i]);

                        FMOD_ControlScript.CancelSelection();
                    }
                    else
                    {
                        if (mainMenu != null)
                        {
                            mainMenu.gameObject.SetActive(true);
                            mainMenu.spotlights.SetActive(false);
                            mainMenu.ChangeState(1);

                            this.gameObject.SetActive(false);

                            confirmButtons[0].SetActive(false);
                            confirmButtons[1].SetActive(false);
                        }
                        else
                        {
                            SceneManager.LoadScene(1);
                        }
                    }
                }
            }
        }

        //Player 1
        if (selected[0] < 3)
        {
            int _s = selected[0];
            //Right
            if (Input.GetKeyDown(KeyCode.D))
            {
                selections[0][_s]++;
                while (selections[0][_s] >= lengths[_s])
                {
                    selections[0][_s] -= lengths[_s];
                }

                UpdateDisplay(0, selected[0]);

                FMOD_ControlScript.ChangeSelection();
            }
            //Left
            else if (Input.GetKeyDown(KeyCode.A))
            {
                selections[0][_s]--;
                while (selections[0][_s] < 0)
                {
                    selections[0][_s] += lengths[_s];
                }

                UpdateDisplay(0, selected[0]);

                FMOD_ControlScript.ChangeSelection();
            }
            if (_s == 2)
            {
                if (Input.GetKeyDown(KeyCode.W))
                {
                    UpdateAName(0, -1);
                    FMOD_ControlScript.ChangeSelection();
                }
                if (Input.GetKeyDown(KeyCode.S))
                {
                    UpdateAName(0, 1);
                    FMOD_ControlScript.ChangeSelection();
                }
            }
            //Select (A key)
            if (Input.GetKeyDown(KeyCode.E))
            {
                if (selected[0] != 1 || selected[1] <= 1 || selections[0][1] != selections[1][1])
                {
                    selected[0] = (int)Mathf.MoveTowards(selected[0], 3, 1);
                    if (selected[0] == 3)
                    {
                        //cursors[0].gameObject.SetActive(false);
                        animators[0].SetBool("IsDancing", true);
                        confirmButtons[0].SetActive(true);
                    }
                    UpdateDisplay(0, selected[0]);

                    FMOD_ControlScript.ConfirmSelection();
                }
            }
        }
        //Go back (B)
        if (Input.GetKeyDown(KeyCode.Q))
        {
            if (selected[0] > 0)
            {
                selected[0] = (int)Mathf.MoveTowards(selected[0], 0, 1); ;
                if (selected[0] == 2)
                {
                    //cursors[0].gameObject.SetActive(true);
                    animators[0].SetBool("IsDancing", false);
                    confirmButtons[0].SetActive(false);
                }
                UpdateDisplay(0, selected[0]);

                FMOD_ControlScript.CancelSelection();
            }
            else
            {
                if (mainMenu != null)
                {
                    mainMenu.gameObject.SetActive(true);
                    mainMenu.spotlights.SetActive(false);
                    mainMenu.ChangeState(1);

                    this.gameObject.SetActive(false);
                }
                else
                {
                    SceneManager.LoadScene(1);
                }
            }
        }


        //Player 2
        if (selected[1] < 3)
        {
            int _s = selected[1];
            //Right
            if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                selections[1][_s]++;
                while (selections[1][_s] >= lengths[_s])
                {
                    selections[1][_s] -= lengths[_s];
                }

                UpdateDisplay(1, selected[1]);

                FMOD_ControlScript.ChangeSelection();
            }
            //Left
            else if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                selections[1][_s]--;
                while (selections[1][_s] < 0)
                {
                    selections[1][_s] += lengths[_s];
                }

                UpdateDisplay(1, selected[1]);

                FMOD_ControlScript.ChangeSelection();
            }
            if (_s == 2)
            {
                if (Input.GetKeyDown(KeyCode.UpArrow))
                {
                    UpdateAName(1, -1);
                    FMOD_ControlScript.ChangeSelection();
                }
                if (Input.GetKeyDown(KeyCode.DownArrow))
                {
                    UpdateAName(1, 1);
                    FMOD_ControlScript.ChangeSelection();
                }
            }
            //Select (A key)
            if (Input.GetKeyDown(KeyCode.RightShift))
            {
                if (selected[1] != 1 || selected[0] <= 1 || selections[0][1] != selections[1][1])
                {
                    selected[1] = (int)Mathf.MoveTowards(selected[1], 3, 1);
                    if (selected[1] == 3)
                    {
                        //cursors[1].gameObject.SetActive(false);
                        animators[1].SetBool("IsDancing", true);
                        confirmButtons[1].SetActive(true);
                    }
                    UpdateDisplay(1, selected[1]);

                    FMOD_ControlScript.ConfirmSelection();
                }
            }
        }
        //Go back (B)
        if (Input.GetKeyDown(KeyCode.RightControl))
        {
            if (selected[1] > 0)
            {
                selected[1] = (int)Mathf.MoveTowards(selected[1], 0, 1); ;
                if (selected[1] == 2)
                {
                    //cursors[1].gameObject.SetActive(true);
                    animators[1].SetBool("IsDancing", false);
                    confirmButtons[1].SetActive(false);
                }
                UpdateDisplay(1, selected[1]);

                FMOD_ControlScript.CancelSelection();
            }
            else
            {
                if (mainMenu != null)
                {
                    mainMenu.gameObject.SetActive(true);
                    mainMenu.spotlights.SetActive(false);
                    mainMenu.ChangeState(1);

                    this.gameObject.SetActive(false);
                }
                else
                {
                    SceneManager.LoadScene(1);
                }
            }
        }

        if (selected[0] == 3 && selected[1] == 3)
        {
            countdown += Time.deltaTime;
            if (countdown >= 1f)
            {
                FMOD_ControlScript.PlaySoundOneShot(startGameEventPath); // this needs to be moved to where the players select
                LoadNextScene();
            }
        }
        else
        {
            countdown = 0f;
        }

        for (int b = 0; b < 2; b++)
        {
            ConfirmButton1[b].sprite = (InputManager.Devices.Count >= 1) ? sprButton[0] : sprButton[1];
            ConfirmButton2[b].sprite = (InputManager.Devices.Count == 2) ? sprButton[0] : sprButton[2];
        }
    }

    // handles relevant audio changes, loads next scene
    private void LoadNextScene()
    {
        FMOD_ControlScript.StopAudio(FMOD_ControlScript.menuMusic);

        SceneManager.LoadScene(2);
    }

    private void UpdateDisplay(int player_number, int selectedType)
    {
        DisplayCostume(player_number);

        SetDisplayToColor(player_number);
        //cursors[player_number].position = pos;

        GameObject[] arrowArray = arrows[player_number];
        for (int i = 0; i < arrowArray.Length; i++)
        {
            arrowArray[i].SetActive(i == selectedType);
        }

        if (selectedType == 2)
        {
            RectTransform rect = arrowArray[selectedType].GetComponent<RectTransform>();
            rect.anchoredPosition = new Vector3(110 * selections[player_number][2] + 50, 0, 0);
        }

        int othersColorInt = -1, otherPlayer = Mathf.Abs(player_number-1);
        if (selected[otherPlayer] > 1)
        {
            othersColorInt = selections[otherPlayer][1];
        }

        for (int i = 0; i < colorArray.Length; i++)
        {
            int index = i + selections[player_number][1];
            if (index >= colorArray.Length) { index -= colorArray.Length; }
            if (index < 0) { index += colorArray.Length; }

            Color c = colorArray[index];

            if (othersColorInt == index) { c = Color.Lerp(c, Color.black, 0.7f); }

            colorIcons[player_number][i].color = c;
            
        }

        namesArray[0].enabled = (selected[0] == 2);
        namesArray[1].enabled = (selected[1] == 2);

    }

    private void UpdateAName(int p, int modifier)
    {
        char[] chars = GameControllerScript.PlayerNames[p].ToCharArray();
        if (modifier != 0) {
            int index = selections[p][2];
            
            //Access specific index of string
            if (modifier > 0)
            {
                if (chars[index] == 'Z')
                {
                    chars[index] = ' ';
                }
                else if (chars[index] == ' ')
                {
                    chars[index] = 'A';
                }
                else
                {
                    chars[index]++;
                }
            }
            else
            {
                if (chars[index] == 'A')
                {
                    chars[index] = ' ';
                }
                else if (chars[index] == ' ')
                {
                    chars[index] = 'Z';
                }
                else
                {
                    chars[index]--;
                }
            }

            //Convert back to string
            GameControllerScript.PlayerNames[p] = new string(chars);
            if (bannedList.Contains(GameControllerScript.PlayerNames[p]))
            {
                GameControllerScript.PlayerNames[p] = "   ";
            }
        }

        for (int i = 0; i < chars.Length; i++)
        {
            if (chars[i] == ' ') { chars[i] = '_'; }
        }

        namesArray[p].text = new string(chars);

    }

    private void SetDisplayToColor(int p)
    {
        int pSelectionColour = selections[p][1];

        //Change player's stored colour
        GameControllerScript.PlayerColors[p] = colorArray[pSelectionColour];
        Material colouredMat = colouredMatArray[pSelectionColour];

        GameControllerScript.playerCostumeMat[p] = colouredMat;

        //Apply this colour to the gremlin & current costume as well
        int pSelectionCostume = selections[p][0];
        CustomizeCosumeScript costume = pDisplays[p][pSelectionCostume];
        costume.SetColour(GameControllerScript.PlayerColors[p], colouredMat);          
    }

    private void DisplayCostume(int p)
    {
        GameControllerScript.PlayerOutfits[p] = selections[p][0];

        int length = pDisplays[p].Length;
        for (int i = 0; i < length; i++)
        {
            CustomizeCosumeScript costume = pDisplays[p][i];

            costume.gameObject.SetActive((i == selections[p][0]));
        }
    }
}
