using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using InControl;

public class PaxGameEnd : MonoBehaviour
{

    [SerializeField]
    private float[] y = new float[2], JoyStickMenuDelay = new float[2], MenuDelayMax = new float[2] { 0.25f, 0.25f };
    private bool[] FirstJoystickMove = new bool[2] { true, true };

    public List<Transform> CatalogButtons = new List<Transform>();
    private RectTransform[] Cursors = new RectTransform[2];
    private Vector3[] CursorOGScale = new Vector3[2];
    private float Delay = 3f;


    [SerializeField]
    private bool[] PlayerSelect = new bool[2], CursorMove = new bool[2] { false, false };
    private bool[] DPADDown = new bool[2] { false, false }, DPADUp = new bool[2] { false, false }, cancelB = new bool[2] { false, false }, AButtonPressed = new bool[2] { false, false };

    public int[] buttonSelected = new int[2] { 0, 0 };
    private buttonScoreBehaviour Button;

    public GameObject PlayerSelectionCursor;
    public GameObject TotalPotionLabel;
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

        CursorOGScale[0] = Cursors[0].transform.localScale;
        CursorOGScale[1] = Cursors[1].transform.localScale;

        Cursors[0].transform.GetChild(0).GetComponent<Image>().color = (GameControllerScript.PlayerColors != null) ? GameControllerScript.PlayerColors[0] : Color.red;
        Cursors[1].transform.GetChild(0).GetComponent<Image>().color = (GameControllerScript.PlayerColors != null) ? GameControllerScript.PlayerColors[1] : Color.cyan;

        Cursors[0].position = CatalogButtons[0].GetComponent<buttonScoreBehaviour>().IconLocation[0].position;
        Cursors[1].position = CatalogButtons[0].GetComponent<buttonScoreBehaviour>().IconLocation[1].position;

        Cursors[0].transform.rotation = CatalogButtons[0].GetComponent<buttonScoreBehaviour>().IconLocation[0].rotation;
        Cursors[1].transform.rotation = CatalogButtons[0].GetComponent<buttonScoreBehaviour>().IconLocation[1].rotation;
    }

    public void Update()
    {
        Delay = Mathf.MoveTowards(Delay, 0, Time.deltaTime);

        storedScore = (int)Mathf.MoveTowards(storedScore, GameControllerScript.local.ScoreControl.totalScore, 14f);
        TotalPotionLabel.GetComponent<Text>().text = storedScore.ToString();

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


    }

    void GetButtons()
    {
        if (InputManager.Devices != null && InputManager.Devices.Count > 0)
        {
            for (int i = 0; i < InputManager.Devices.Count; i++)
            {
                DPADDown[i] = false;
                DPADUp[i] = false;
                AButtonPressed[i] = false;
                cancelB[i] = false;
                InputDevice _myInputDevice = InputManager.Devices[i];

                DPADDown[i] = DPADDown[i] || _myInputDevice.DPadDown.WasPressed;
                DPADUp[i] = DPADUp[i] || _myInputDevice.DPadUp.WasPressed;
                AButtonPressed[i] = AButtonPressed[i] || _myInputDevice.Action1.WasPressed;
                cancelB[i] = cancelB[i] || _myInputDevice.Action2.WasPressed;

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
        if (Delay == 0)
        {
            if (InputManager.Devices.Count > 0)
            {
                for (int i = 0; i < InputManager.Devices.Count; i++)
                {

                    if (AButtonPressed[i])
                    {
                        MenuSelect(CatalogButtons[buttonSelected[i]], i);
                        FMOD_ControlScript.ChangeSelection();
                    }

                }
            }
            else
            {

                if (Input.GetKeyDown(KeyCode.R))
                {
                    MenuSelect(CatalogButtons[buttonSelected[0]], 0);
                }
                if (Input.GetKeyDown(KeyCode.RightAlt))
                {
                    MenuSelect(CatalogButtons[buttonSelected[1]], 1);
                }
            }
        }
    }
    public void MenuSelect(Transform SelectedButton, int PlayNum)
    {
        //Button = SelectedButton.GetComponentInChildren<buttonScoreBehaviour>();

        Cursors[PlayNum].transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);
        PlayerSelect[PlayNum] = !(PlayerSelect[PlayNum]);

        if ((PlayerSelect[0] == true && PlayerSelect[1] == true) && (buttonSelected[0] == buttonSelected[1]))
        {
            //Cursors[0].transform.localScale = CursorOGScale[0];
            //Cursors[1].transform.localScale = CursorOGScale[1];
            //PlayerSelect[0] = false;
            //PlayerSelect[1] = false;
            FMOD_ControlScript.ConfirmSelection();
            EndSession();   
        }
    }

    public void EndSession()
    {
        GameControllerScript.Lose();
    }
}
