using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using RotaryHeart.Lib.SerializableDictionary;
using InControl;

public class PauseMenuScript : MonoBehaviour
{
    //Pause Menu States
    public GameObject PauseMenu;
    public GameObject OptionsMenu;
    private bool _optionsMenuActive = false;
    public OptionsSelection OptionSelection = new OptionsSelection();
    public OptionsSelection ControllerSelectUI = new OptionsSelection();

    //Options Menu Variables
    public int OptionbuttonSelected = 0, OptionsListMax = 1;
    public Sprite selected, UnSelected, ControllerSelected, ControllerUnSelected, XboxBack,PSBack,KeyBack;
    public GameObject VolumeVal;
    public Slider BrightnessVal;
    public GameObject BackBtn;

    //button objects
    private List<MenuButtonStuff> buttonArray = new List<MenuButtonStuff>();
    public Button ResumeButton;
    public Button RestartButton;
    public Button OptionsButton;
    public Button QuitButton;
    public MenuMusicController menuMusic;

    [HideInInspector]
    public bool gamePad = true; //Switch between KB+M and GamePad
    [HideInInspector]
    public byte PLAYERNUM = 0;

    public int buttonSelected = 0;

    public GameObject UISprite;
    public Sprite Xbox, XboxPS, Playstation, XKeyboardP2, PKeyboardP2, AllKeyboard;



    private bool DPADDown = false, DPADUp = false, DPADLeft = false, DPADRight = false, cancelB = false, AButtonPressed = false;


    private float[] x = new float[2], y = new float[2], JoyStickMenuDelay = new float[2], MenuDelayMax = new float[2] { 0.25f, 0.25f }, JoyStickXMenuDelay = new float[2], MenuXDelayMax = new float[2] { 0.25f, 0.25f };
    private bool[] FirstJoystickMove = new bool[2] { true, true }, FirstXJoystickMove = new bool[2] { true, true };

    void Start()
    {
        //gets the button objects connected to canvas
        ResumeButton = ResumeButton.GetComponent<Button>();
        RestartButton = RestartButton.GetComponent<Button>();
        OptionsButton = OptionsButton.GetComponent<Button>();
        QuitButton = QuitButton.GetComponent<Button>();

        buttonArray.Add(new MenuButtonStuff(ResumeButton));
        buttonArray.Add(new MenuButtonStuff(RestartButton));
        buttonArray.Add(new MenuButtonStuff(OptionsButton));
        buttonArray.Add(new MenuButtonStuff(QuitButton));

        UnhighlightButtons();
        buttonSelected = 0;

        gameObject.SetActive(false);

    }

    void Update()
    {
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
        DPADDown = false;
        DPADUp = false;
        DPADRight = false;
        DPADLeft = false;

        AButtonPressed = false;
        cancelB = false;

        //Assign Buttons Inputs to the Input Device
        if (InputManager.Devices != null && InputManager.Devices.Count > 0)
        {
            for (int i = 0; i < InputManager.Devices.Count; i++)
            {
                InputDevice _myInputDevice = InputManager.Devices[i];

                DPADDown = DPADDown || _myInputDevice.DPadDown.WasPressed;
                DPADUp = DPADUp || _myInputDevice.DPadUp.WasPressed;
                DPADRight = DPADRight || _myInputDevice.DPadRight.WasPressed;
                DPADLeft = DPADLeft || _myInputDevice.DPadLeft.WasPressed;

                AButtonPressed = AButtonPressed || _myInputDevice.Action1.WasPressed;
                cancelB = cancelB || _myInputDevice.Action2.WasPressed;

                x[i] = _myInputDevice.LeftStickX;
                if (x[i] >= 0.5 || x[i] <= -0.5)
                {
                    if (JoyStickXMenuDelay[i] < MenuXDelayMax[i])
                    {
                        JoyStickXMenuDelay[i] += 1 * Time.deltaTime;
                    }
                }

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

    // Update is called once per frame
    public void UpdateControlUI()
    {
        //Update Sprites for the Pause Menu. Check which controller is connected and which buttons to show
        Sprite CurrentUI = UISprite.GetComponent<Image>().sprite;
        if (GameControllerScript.local.Player1.gamePad == true && GameControllerScript.local.Player2.gamePad == true)
        {
            if (GameControllerScript.PSController[0] == 0 && GameControllerScript.PSController[0] == 0)
            {
                CurrentUI = Xbox;
            }
            else if (GameControllerScript.PSController[0] == 1 && GameControllerScript.PSController[1] == 0)
            {
                CurrentUI = XboxPS;
            }
            else if (GameControllerScript.PSController[0] == 0 && GameControllerScript.PSController[1] == 1)
            {
                CurrentUI = XboxPS;
            }
            else
            {
                CurrentUI = Playstation;
            }
        }
        else
        {
            if (GameControllerScript.local.Player1.gamePad == true && GameControllerScript.local.Player2.gamePad == false)
            {
                if (GameControllerScript.PSController[0] == 0)
                {
                    CurrentUI = XKeyboardP2;
                }
                else
                {
                    
                    CurrentUI = PKeyboardP2;
                }
            }
            else
            {
                CurrentUI = AllKeyboard;
            }
        }
        UISprite.GetComponent<Image>().sprite = CurrentUI;
    }


    public void ResetMenuStick(int i)
    {
        if (y[i] > -0.5 && y[i] < 0.5)
        {
            FirstJoystickMove[i] = true;
        }

        if (x[i] > -0.5 && x[i] < 0.5)
        {
            FirstXJoystickMove[i] = true;
        }
    }

    //controller functions for navigating through menus/interactiing with menus
    void MenuControls()
    {
        if (_optionsMenuActive == false)
        {
            if (InputManager.Devices.Count > 0)
            {
                for (int i = 0; i < InputManager.Devices.Count; i++)
                {
                    if (Input.GetKeyDown("s") || (InputManager.Devices[i].DPadDown.WasPressed) || (y[i] <= -0.5 && (JoyStickMenuDelay[i] >= MenuDelayMax[i] || FirstJoystickMove[i] == true)))
                    {
                        UnhighlightButtons();
                        buttonSelected = (buttonSelected < buttonArray.Count - 1) ? (buttonSelected + 1) : 0;
                        FirstJoystickMove[i] = false;
                        JoyStickMenuDelay[i] = 0;
                        Highlight();
                        ResetMenuStick(i);

                        FMOD_ControlScript.ChangeSelection(); // plays change selection sfx

                    }
                    else if (Input.GetKeyDown("w") || (InputManager.Devices[i].DPadUp.WasPressed) || (y[i] >= 0.5 && (JoyStickMenuDelay[i] >= MenuDelayMax[i] || FirstJoystickMove[i] == true)))
                    {
                        UnhighlightButtons();
                        FirstJoystickMove[i] = false;
                        JoyStickMenuDelay[i] = 0;
                        buttonSelected = (buttonSelected > 0) ? (buttonSelected - 1) : (buttonArray.Count - 1);
                        Highlight();

                        ResetMenuStick(i);

                        FMOD_ControlScript.ChangeSelection(); // plays change selection sfx
                    }

                    if (Input.GetKeyDown("space") || AButtonPressed)
                    {
                        MenuSelect(true);
                    }
                    if (cancelB)
                    {
                        MenuSelect(false);
                    }
                }
            }
            else
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

                if (Input.GetKeyDown("space") || AButtonPressed)
                {
                    MenuSelect(true);
                }
                if (cancelB)
                {
                    MenuSelect(false);
                }
            }
        }
        else
        {
            if (InputManager.Devices.Count > 0)
            {
                for (int i = 0; i < InputManager.Devices.Count; i++)
                {
                    if (Input.GetKeyDown("s") || (InputManager.Devices[i].DPadDown.WasPressed) || (y[i] <= -0.5 && (JoyStickMenuDelay[i] >= MenuDelayMax[i] || FirstJoystickMove[i] == true)))
                    {
                        UnhighlightButtons();
                        OptionbuttonSelected = (OptionbuttonSelected < OptionSelection.Count-1) ? (OptionbuttonSelected + 1) : 0;
                        FirstJoystickMove[i] = false;
                        JoyStickMenuDelay[i] = 0;
                        ResetMenuStick(i);

                        FMOD_ControlScript.ChangeSelection(); // plays change selection sfx

                    }
                    else if (Input.GetKeyDown("w") || (InputManager.Devices[i].DPadUp.WasPressed) || (y[i] >= 0.5 && (JoyStickMenuDelay[i] >= MenuDelayMax[i] || FirstJoystickMove[i] == true)))
                    {
                        UnhighlightButtons();
                        FirstJoystickMove[i] = false;
                        JoyStickMenuDelay[i] = 0;
                        OptionbuttonSelected = (OptionbuttonSelected > 0) ? (OptionbuttonSelected - 1) : (OptionSelection.Count - 1);

                        ResetMenuStick(i);

                        FMOD_ControlScript.ChangeSelection(); // plays change selection sfx
                    }
                    if (Input.GetKeyDown("a") || (InputManager.Devices[i].DPadLeft.WasPressed) || (x[i] <= -0.5 && (JoyStickXMenuDelay[i] >= MenuXDelayMax[i] || FirstXJoystickMove[i] == true)))
                    {
                        UnhighlightButtons();
                        FirstXJoystickMove[i] = false;
                        JoyStickXMenuDelay[i] = 0;
                        ResetMenuStick(i);
                        UpdateOptionHorizontal(true);
                        FMOD_ControlScript.ChangeSelection(); // plays change selection sfx

                    }
                    else if (Input.GetKeyDown("d") || (InputManager.Devices[i].DPadRight.WasPressed) || (x[i] >= 0.5 && (JoyStickXMenuDelay[i] >= MenuXDelayMax[i] || FirstXJoystickMove[i] == true)))
                    {
                        UnhighlightButtons();
                        FirstXJoystickMove[i] = false;
                        JoyStickXMenuDelay[i] = 0;
                        UpdateOptionHorizontal(false);
                        ResetMenuStick(i);

                        FMOD_ControlScript.ChangeSelection(); // plays change selection sfx
                    }
                    if (Input.GetKeyDown(KeyCode.LeftArrow) || InputManager.Devices.Count > 1 && ((InputManager.Devices[1].DPadLeft.WasPressed) || (x[1] <= -0.5 && (JoyStickXMenuDelay[1] >= MenuXDelayMax[1] || FirstXJoystickMove[1] == true))))
                    {
                        UnhighlightButtons();
                        FirstXJoystickMove[1] = false;
                        JoyStickXMenuDelay[1] = 0;
                        ResetMenuStick(1);
                        SpecialP2Option();
                        FMOD_ControlScript.ChangeSelection(); // plays change selection sfx

                    }
                    else if (Input.GetKeyDown(KeyCode.RightArrow) || InputManager.Devices.Count > 1 && ((InputManager.Devices[1].DPadRight.WasPressed) || (x[1] >= 0.5 && (JoyStickXMenuDelay[1] >= MenuXDelayMax[1] || FirstXJoystickMove[1] == true))))
                    {
                        UnhighlightButtons();
                        FirstXJoystickMove[1] = false;
                        JoyStickXMenuDelay[1] = 0;
                        SpecialP2Option();
                        ResetMenuStick(1);

                        FMOD_ControlScript.ChangeSelection(); // plays change selection sfx
                    }
                    if (Input.GetKeyDown("space") || AButtonPressed)
                    {
                        MenuSelect(true);
                    }
                    if (cancelB)
                    {
                        _optionsMenuActive = false;
                        PauseMenu.SetActive(true);
                        OptionsMenu.SetActive(false);
                    }
                }
            }
            else
            {
                if (Input.GetKeyDown("s"))
                {
                    UnhighlightButtons();
                    OptionbuttonSelected = (OptionbuttonSelected < OptionSelection.Count - 1) ? (OptionbuttonSelected + 1) : 0;

                    FMOD_ControlScript.ChangeSelection(); // plays change selection sfx

                }
                else if (Input.GetKeyDown("w"))
                {
                    UnhighlightButtons();
                    OptionbuttonSelected = (OptionbuttonSelected > 0) ? (OptionbuttonSelected - 1) : (OptionSelection.Count - 1);

                    FMOD_ControlScript.ChangeSelection(); // plays change selection sfx
                }
                if (Input.GetKeyDown("a"))
                {
                    UnhighlightButtons();
                    UpdateOptionHorizontal(true);

                    FMOD_ControlScript.ChangeSelection(); // plays change selection sfx

                }
                else if (Input.GetKeyDown("d"))
                {
                    UnhighlightButtons();
                    UpdateOptionHorizontal(true);

                    FMOD_ControlScript.ChangeSelection(); // plays change selection sfx
                }
                if (Input.GetKeyDown("space") || AButtonPressed)
                {
                    MenuSelect(true);
                }
                if (cancelB)
                {
                    _optionsMenuActive = false;
                    PauseMenu.SetActive(true);
                    OptionsMenu.SetActive(false);
                }
            }
            UpdateOptionsMenu();
        }
    }

    private void UpdateOptionHorizontal(bool Left)
    {
        switch (OptionbuttonSelected)
        {
            case 0: // Volume Slider
                {
                    if (Left)
                    {
                        if (GameControllerScript.local.MasterVolume > 0)
                        {
                            GameControllerScript.local.MasterVolume -= 5;
                            FMOD_ControlScript.SetMasterVolume(GameControllerScript.local.MasterVolume);
                        }
                    }
                    else
                    {
                        if (GameControllerScript.local.MasterVolume < 100)
                        {
                            GameControllerScript.local.MasterVolume += 5;
                            FMOD_ControlScript.SetMasterVolume(GameControllerScript.local.MasterVolume);
                        }
                    }

                    VolumeVal.GetComponent<Text>().text = GameControllerScript.local.MasterVolume.ToString();
                    break;
                }
            case 1: // Player 1 UI Buttons Slider
                {
                    ChangeUIGFX(0);
                    UpdateControlUI();
                    break;
                }
        }

       
    }
    private void SpecialP2Option()
    {
        ChangeUIGFX(1);
        UpdateControlUI();
    }

    //Update the selected Option Menu Selection
    void UpdateOptionsMenu()
    {
        switch (OptionbuttonSelected)
        {
            case 0: // Volume Slider
                {
                    foreach (int key in OptionSelection.Keys)
                    {
                        OptionSelection[key].ArrowL.GetComponent<Image>().sprite = UnSelected;
                        OptionSelection[key].ArrowR.GetComponent<Image>().sprite = UnSelected;
                    }
                    OptionSelection[0].ArrowL.GetComponent<Image>().sprite = selected;
                    OptionSelection[0].ArrowR.GetComponent<Image>().sprite = selected;
                    break;
                }
            case 1:
                {
                    foreach (int key in OptionSelection.Keys)
                    {
                        OptionSelection[key].ArrowL.GetComponent<Image>().sprite = UnSelected;
                        OptionSelection[key].ArrowR.GetComponent<Image>().sprite = UnSelected;
                    }
                    OptionSelection[1].ArrowL.GetComponent<Image>().sprite = selected;
                    OptionSelection[1].ArrowR.GetComponent<Image>().sprite = selected;

                    if (GameControllerScript.local.Player1.gamePad == false)
                    {
                        BackBtn.GetComponent<Image>().sprite = KeyBack;
                    }
                    else
                    {
                        if (GameControllerScript.PSController[0] == 0)
                        {
                            BackBtn.GetComponent<Image>().sprite = XboxBack;
                        }
                        else
                        {
                            BackBtn.GetComponent<Image>().sprite = PSBack;
                        }
                    }
                    break;
                }
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

        // play selection sound
        FMOD_ControlScript.PlaySoundOneShot(FMOD_ControlScript.changeSelectionEventPath);
    }

    public void HidePause()
    {
        foreach (MenuButtonStuff button in buttonArray)
        {
            button.myAnimator.SetTrigger("Normal");
            button.myImage.color = Color.gray;
            button.rect.localScale = new Vector3(1, 1, 1);
        }
    }

    void MenuSelect(bool isPressed)
    {
        Button currentButton = buttonArray[buttonSelected].myButton;
        if (_optionsMenuActive == false)
        {
            if (isPressed)
            {
                currentButton.onClick.Invoke();
            }
        }
        else
        {
            switch (OptionbuttonSelected)
            {
                case 1:
                    {
                        Debug.Log("Hello");
                        ChangeUIGFX(0);
                        break;
                    }
            }
        }
    }

    //loads game
    public void ResumeGame()
    {
        FMOD_ControlScript.CancelSelection();
        GameControllerScript.PauseGame();
    }

    //loads options
    public void RestartGame()
    {
        GameControllerScript.local.StopSounds();
        FMOD_ControlScript.SetPauseParam(0);
        SceneManager.LoadScene(2);
    }

    //exits game
    public void Options()
    {
        foreach (MenuButtonStuff button in buttonArray)
        {
            button.myAnimator.SetTrigger("Normal");
            button.rect.localScale = new Vector3(1, 1, 1);
        }
        FMOD_ControlScript.ConfirmSelection();
        _optionsMenuActive = true;
        PauseMenu.SetActive(false);
        OptionsMenu.SetActive(true);
    }

    //loads main menu
    public void Quit()
    {
        FMOD_ControlScript.SetPauseParam(0);
        FMOD_ControlScript.ConfirmSelection();
        FMOD_ControlScript.StopAudio(FMOD_ControlScript.FEbgMusic);
        SceneManager.LoadScene(1);
    }

    //loads main menu
    public void ChangeUIGFX(int i)
    {
        if (GameControllerScript.PSController[i] == 0)
        {
            GameControllerScript.PSController[i] = 1;
            ControllerSelectUI[i].ArrowL.GetComponent<Image>().sprite = ControllerUnSelected;
            ControllerSelectUI[i].ArrowR.GetComponent<Image>().sprite = ControllerSelected;
        }
        else
        {
            GameControllerScript.PSController[i] = 0;
            ControllerSelectUI[i].ArrowL.GetComponent<Image>().sprite = ControllerSelected;
            ControllerSelectUI[i].ArrowR.GetComponent<Image>().sprite = ControllerUnSelected;

        }
    }

    public bool OptionsMenuActive
    {
        get { return _optionsMenuActive; }
        set { _optionsMenuActive = value; }
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
    [System.Serializable]
    public class Selectables
    {
        public GameObject ArrowL, ArrowR;
    }
    [System.Serializable]
    public class OptionsSelection : SerializableDictionaryBase<int, Selectables> { }
}
