using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using InControl;

public class MainMenuScript : MonoBehaviour
{
    public Transform myCam;
    public Vector3 camPosMain, camPosCredits, camPosPlay, camPosOptions;
    private Vector3 target;

    private int state = 1;
    private float playingLoad = 0;
    public GameObject allButtons, creditNames, gameIcon;

    public GameObject characterSelection, spotlights;

    //button objects
    private List<MenuButtonStuff> buttonArray = new List<MenuButtonStuff>();
    public Button startButton;
    public Button optionsButton;
    public Button creditsButton;
    public Button exitButton;

    [HideInInspector]
    public bool gamePad = true; //Switch between KB+M and GamePad
    [HideInInspector]
    public byte PLAYERNUM = 0;

    [HideInInspector]
    public int buttonSelected = 0;
    public int startButtonNumber = 0;

    private bool DPADDown = false, DPADUp = false, cancelB = false, AButtonPressed = false ;
    [SerializeField]
    private float[] y = new float[2], JoyStickMenuDelay = new float[2], MenuDelayMax = new float[2] {0.25f, 0.25f};
    private bool[] FirstJoystickMove = new bool [2] { true , true};

    void Start()
    {
        //gets the button objects connected to canvas
        buttonArray.Add(new MenuButtonStuff(startButton));
        buttonArray.Add(new MenuButtonStuff(optionsButton));
        buttonArray.Add(new MenuButtonStuff(creditsButton));
        buttonArray.Add(new MenuButtonStuff(exitButton));

        UnhighlightButtons();
        buttonSelected = startButtonNumber;
        Highlight();

        target = camPosMain;

        // Resets volume to default
        if (FMOD_ControlScript.GetMasterVolume() < 1)
        {
            FMOD_ControlScript.SetMasterVolume(100);
        }

        // starts music (if it's not already playing)
        if (!FMOD_ControlScript.PlaybackState(FMOD_ControlScript.menuMusic))
        {
            FMOD_ControlScript.menuMusic.start();
        }
    }

    void Update()
    {
        GetButtons();
        // Marc & Lizzie the Wizzie. Name a more iconic duo. Now it's on line 69. Good one Matt!
        MenuControls(); 
        if (InputManager.Devices != null)
        {
            gamePad = true;
        }
        else
        {
            gamePad = false;
        }

        if (state == 0)
        {
            myCam.position = Vector3.MoveTowards(myCam.position, target, 0.9f);

            if (myCam.position == target)
            {
                if (target == camPosMain) { allButtons.SetActive(true); gameIcon.SetActive(true); state = 1; }
                else if (target == camPosCredits) { creditNames.SetActive(true); gameIcon.SetActive(false); state = -1; }
                else if (target == camPosOptions) { state = -2; }
            }
        }
        else if (playingLoad > 0)
        {
            playingLoad = Mathf.MoveTowards(playingLoad, 0, Time.deltaTime);

            myCam.position = Vector3.Lerp(camPosPlay, camPosMain, playingLoad / 2);

            if (playingLoad == 0)
            {
                //SceneManager.LoadScene(2);
                characterSelection.SetActive(true);
                this.gameObject.SetActive(false);
            }
        }
    }

    void GetButtons()
    {

        DPADDown = false;
        DPADUp = false;
        AButtonPressed = false;
        cancelB = false;

        if (InputManager.Devices != null && InputManager.Devices.Count > 0)
        {
            for (int i = 0; i < InputManager.Devices.Count; i++)
            {
                InputDevice _myInputDevice = InputManager.Devices[i];

                DPADDown = DPADDown || _myInputDevice.DPadDown.WasPressed;
                DPADUp = DPADUp || _myInputDevice.DPadUp.WasPressed;
                AButtonPressed = AButtonPressed || _myInputDevice.Action1.WasPressed;
                cancelB = cancelB || _myInputDevice.Action2.WasPressed;

                y[i] = _myInputDevice.LeftStickY;
                if (y[i] >= 0.5 || y[i] <= -0.5)
                {
                    if (JoyStickMenuDelay[i] < MenuDelayMax[i])
                    {
                        JoyStickMenuDelay[i] += 1 * Time.deltaTime;
                    }
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
        if (InputManager.Devices.Count > 0)
        {
            for (int i = 0; i < InputManager.Devices.Count; i++)
            {
                if (state == 1 && playingLoad == 0)
                {
                    if ((InputManager.Devices[i].DPadDown.WasPressed) || (y[i] <= -0.5 && (JoyStickMenuDelay[i] >= MenuDelayMax[i] || FirstJoystickMove[i] == true)))
                    {
                        UnhighlightButtons();
                        buttonSelected = (buttonSelected < buttonArray.Count - 1) ? (buttonSelected + 1) : 0;
                        FirstJoystickMove[i] = false;
                        JoyStickMenuDelay[i] = 0;
                        Highlight();
                        ResetMenuStick(i);

                        FMOD_ControlScript.ChangeSelection(); // plays change selection sfx
                    }
                    else if ((InputManager.Devices[i].DPadUp.WasPressed) || (y[i] >= 0.5 && (JoyStickMenuDelay[i] >= MenuDelayMax[i] || FirstJoystickMove[i] == true)))
                    {
                        UnhighlightButtons();
                        FirstJoystickMove[i] = false;
                        JoyStickMenuDelay[i] = 0;
                        buttonSelected = (buttonSelected > 0) ? (buttonSelected - 1) : (buttonArray.Count - 1);
                        Highlight();

                        ResetMenuStick(i);

                        FMOD_ControlScript.ChangeSelection(); // plays change selection sfx

                    }

                    if (AButtonPressed)
                    {
                        FMOD_ControlScript.ConfirmSelection(); // plays confirm selection sfx
                        MenuSelect(true);
                    }
                    if (cancelB)
                    {
                        // play cancel sfx
                        FMOD_ControlScript.CancelSelection();
                        MenuSelect(false);
                    }
                }
                else if (state < 0)
                {
                    if (Input.anyKeyDown || InputManager.Devices[i].AnyButtonWasPressed)
                    {
                        ChangeState(1);
                    }
                }
            }
        }

        if (state == 1)
        {
            if (Input.GetKeyDown("s"))
            {
                UnhighlightButtons();
                buttonSelected = (buttonSelected < buttonArray.Count - 1) ? (buttonSelected + 1) : 0;
                Highlight();

                FMOD_ControlScript.ChangeSelection(); // plays change selection sfx
            }
            else if (Input.GetKeyDown("w"))
            {
                UnhighlightButtons();
                buttonSelected = (buttonSelected > 0) ? (buttonSelected - 1) : (buttonArray.Count - 1);
                Highlight();

                FMOD_ControlScript.ChangeSelection(); // plays change selection sfx

            }

            if (Input.GetKeyDown("space"))
            {
                FMOD_ControlScript.ConfirmSelection(); // plays confirm selection sfx
                MenuSelect(true);
                    
            }
        }
        else if (state < 0)
        {
            if (Input.anyKeyDown) { ChangeState(1); }
        }
    }

    void MenuSelect(bool isPressed)
    {
        Button currentButton = buttonArray[buttonSelected].myButton;

        if (isPressed)
        {
            currentButton.onClick.Invoke();
        }
    }

    private void UnhighlightButtons()
    {
        foreach (MenuButtonStuff button in buttonArray)
        {
            button.myAnimator.SetTrigger("Normal");
            button.myImage.color = Color.gray;
        }
    }

    public void Highlight()
    {
        MenuButtonStuff highlightedButton = buttonArray[buttonSelected];
        highlightedButton.myAnimator.SetTrigger("Highlighted");
        highlightedButton.myImage.color = Color.white;
    }

    public void ChangeState(int requested)
    {
        if (state != 0)
        {
            switch (requested)
            {
                case -1:
                    target = camPosCredits;
                    break;
                case -2:
                    target = camPosOptions;
                    break;
                default:
                    target = camPosMain;
                    break;
            }
            state = 0;

            allButtons.SetActive(false);
            gameIcon.SetActive(false);
            creditNames.SetActive(false);
        }
    }

    //loads game
    public void StartGame()
    {
        if (playingLoad == 0)
        {
            playingLoad = 2f;
            allButtons.SetActive(false);
        }
    }

    //loads options
    public void Leaderboards()
    {
        SceneManager.LoadScene(4);
    }

    //exits game
    public void Exit()
    {
        // Stop playing music

        //TODO: close application
        Application.Quit();
        Debug.Log("Quitting...");
    }

    //loads main menu
    public void MainMenu()
    {
        //SceneManager.LoadScene(1);
    }

    private class MenuButtonStuff
    {
        public Animator myAnimator;
        public Image myImage;
        public Button myButton;
        public RectTransform rect;

        public MenuButtonStuff(Button b)
        {
            myButton = b;
            myImage = b.GetComponent<Image>();
            myAnimator = b.GetComponent<Animator>();
            rect = b.GetComponent<RectTransform>();
        }
    }
}




