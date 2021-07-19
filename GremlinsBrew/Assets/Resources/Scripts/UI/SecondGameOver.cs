using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using InControl;

public class SecondGameOver : MonoBehaviour
{
    //button objects
    private List<MenuButtonStuff> buttonArray = new List<MenuButtonStuff>();
    public Button menuButton;

    //public MenuMusicController menuMusic;

    [HideInInspector]
    public bool gamePad = true; //Switch between KB+M and GamePad
    [HideInInspector]
    public byte PLAYERNUM = 0;

    [HideInInspector]
    public int buttonSelected = 0;
    public int startButtonNumber = 0;

    private bool DPADRight = false, DPADLeft = false, cancelB = false, AButtonPressed = false;

    public float[] x = new float[2], JoyStickMenuDelay = new float[2], MenuDelayMax = new float[2] { 0.25f, 0.25f };
    private bool[] FirstJoystickMove = new bool[2] { true, true };

    private float Delay = 2f;

    void Start()
    {
        buttonArray.Add(new MenuButtonStuff(menuButton));

        UnhighlightButtons();
        buttonSelected = startButtonNumber;
        Highlight();
    }

    void Update()
    {
        Delay = Mathf.MoveTowards(Delay, 0, Time.fixedDeltaTime);
        GetButtons();
        MenuControls();
        if (InputManager.Devices != null)
        {
            gamePad = true;
        }
        else
        {
            gamePad = false;
        }
    }

    void GetButtons()
    {
        DPADRight = false;
        DPADLeft = false;
        AButtonPressed = false;
        cancelB = false;

        if (InputManager.Devices != null && InputManager.Devices.Count > 0)
        {
            for (int i = 0; i < InputManager.Devices.Count; i++)
            {
                InputDevice _myInputDevice = InputManager.Devices[i];

                DPADRight = DPADRight || _myInputDevice.DPadRight.WasPressed;
                DPADLeft = DPADLeft || _myInputDevice.DPadLeft.WasPressed;
                AButtonPressed = AButtonPressed || _myInputDevice.Action1.WasPressed;
                cancelB = cancelB || _myInputDevice.Action2.WasPressed;

                x[i] = _myInputDevice.LeftStickX;
                if (x[i] >= 0.5 || x[i] <= -0.5)
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
        if (x[i] > -0.5 && x[i] < 0.5)
        {
            FirstJoystickMove[i] = true;
        }
    }

    //controller functions for navigating through menus/interactiing with menus
    void MenuControls()
    {
        if (Delay == 0)
        {
            if (InputManager.Devices.Count > 0)
            {
                for (int i = 0; i < InputManager.Devices.Count; i++)
                {
                    if ((InputManager.Devices[i].DPadRight.WasPressed) || (x[i] <= -0.5 && (JoyStickMenuDelay[i] >= MenuDelayMax[i] || FirstJoystickMove[i] == true)))
                    {
                        UnhighlightButtons();
                        buttonSelected = (buttonSelected < buttonArray.Count - 1) ? (buttonSelected + 1) : 0;
                        FirstJoystickMove[i] = false;
                        JoyStickMenuDelay[i] = 0;
                        Highlight();
                        ResetMenuStick(i);

                        FMOD_ControlScript.ChangeSelection();

                    }
                    else if ((InputManager.Devices[i].DPadLeft.WasPressed) || (x[i] >= 0.5 && (JoyStickMenuDelay[i] >= MenuDelayMax[i] || FirstJoystickMove[i] == true)))
                    {
                        UnhighlightButtons();
                        FirstJoystickMove[i] = false;
                        JoyStickMenuDelay[i] = 0;
                        buttonSelected = (buttonSelected > 0) ? (buttonSelected - 1) : (buttonArray.Count - 1);
                        Highlight();

                        ResetMenuStick(i);

                        FMOD_ControlScript.ChangeSelection();
                    }

                    if (Input.GetKeyDown("space") || AButtonPressed)
                    {
                        MenuSelect(true);

                        FMOD_ControlScript.ConfirmSelection();
                    }
                }
            }

            if (Input.GetKeyDown("s"))
            {
                UnhighlightButtons();
                buttonSelected = (buttonSelected < buttonArray.Count - 1) ? (buttonSelected + 1) : 0;
                Highlight();

                FMOD_ControlScript.ChangeSelection();

            }
            else if (Input.GetKeyDown("w"))
            {
                UnhighlightButtons();
                buttonSelected = (buttonSelected > 0) ? (buttonSelected - 1) : (buttonArray.Count - 1);
                Highlight();

                FMOD_ControlScript.ChangeSelection();
            }

            if (Input.GetKeyDown("space"))
            {
                MenuSelect(true);

                FMOD_ControlScript.ConfirmSelection();
            }
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

    //loads main menu
    public void MainMenu()
    {
        FMOD_ControlScript.StopAudio(FMOD_ControlScript.FEbgMusic);
        SceneManager.LoadScene(1);
    }
    //loads main menu
    public void Restart()
    {
        FMOD_ControlScript.StopAudio(FMOD_ControlScript.FEbgMusic);

        // Starts Background Music
        FMOD_ControlScript.FEbgMusic.start();
        SceneManager.LoadScene(2);
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
