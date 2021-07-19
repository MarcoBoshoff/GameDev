using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using InControl;

public class QTE_Hug : QTEScript
{
    //private Vector2[] targets;
    private bool[] in_range;

    private Dictionary<PlayerScript, float> presses = new Dictionary<PlayerScript, float>();

    [SerializeField]
    private Image GremlineHead;

    [SerializeField]
    private RectTransform myRect, area;
    private float areaTarget;
    [SerializeField]
    private Image[] buttonPrompt = new Image[2];
    [SerializeField]
    private Sprite xBtn, yBtn, aBtn, thumbstick;
    [SerializeField]
    private Sprite[] keyboard = new Sprite[2];
    private GamepadEnum toPress = GamepadEnum.X;

    public float ScreenWidth = 6f;

    public GameObject gremlinHeadPrefab;

    public GameObject[] ThumbstickPrompt = new GameObject[2];
    public float[] PromptTimer = new float[2] { 5, 5 };
    public float[] FadeTimer = new float[2] { 1, 1 };
    //Override for any values before setup starts
    public override void SetupStarting()
    {
        in_range = new bool[2];
        in_range[0] = false;
        in_range[1] = false;

        toPress = GamepadEnum.A;

        buttonPrompt[0].sprite = GameControllerScript.BtnPromptSprite(GameControllerScript.local.Player1, BtnPromptEnum.InteractBtn);
        buttonPrompt[1].sprite = GameControllerScript.BtnPromptSprite(GameControllerScript.local.Player2, BtnPromptEnum.InteractBtn);

        areaTarget = Random.Range(-0.35f, 0.35f);
        Vector3 pos = area.position;
        pos.x += areaTarget * (Screen.height / ScreenWidth);
        area.position = pos;

        TutorialScript.Trigger(TutorialTrigger.HugQTEStarted);
    }

    //Override for any values after setup has finished -> ie, after all the axes targets have been added
    public override void SetupFinished()
    { }

    //Receive Movement Input
    public override void MovementInput(PlayerScript player, Vector2 movement)
    {
        QTEPlayer qtePlayer = attachedPlayers[player];

        if (qtePlayer != null)
        {
            if (movement.magnitude > 1)
            {
                movement = movement.normalized;
            }

            //Vector3 newPos = (myRect.position + Vector3.right * movement.x);
            //newPos += transform.up * movement.y;

            int num = qtePlayer.myNum;

            int pullback = (num * 2) - 1;

            //float dist = Mathf.Min(Vector2.Distance(movement, targets[num]), 1);


            qtePlayer.percent = Mathf.Clamp(qtePlayer.percent + (movement.x * 0.03f), -1, 1);
            qtePlayer.percent = Mathf.MoveTowards(qtePlayer.percent, pullback, 0.005f);

            if (Mathf.Abs(qtePlayer.percent - areaTarget) <= 0.4f) { in_range[num] = true; }

            //qtePlayer.representation.transform.position = transform.position + (transform.right * dir * 2) * (dist+0.2f);
            qtePlayer.representation.position = myRect.position + (Vector3.right * qtePlayer.percent * (Screen.height / ScreenWidth));
        }
    }

    public override void ButtonPressed(PlayerScript player, GamepadEnum btn)
    {
        if (in_range[attachedPlayers[player].myNum] && btn == toPress)
        {
            presses[player] = 0.7f;
        }
    }

    //Just the update method
    protected override void QteUpdate(int playerCount)
    {
        if (attachedPlayers.Count == 2)
        {
            buttonPrompt[0].enabled = (in_range[0] && in_range[1]);
            buttonPrompt[1].enabled = (in_range[0] && in_range[1]);

            GremlineHead.enabled = false;

            Vector3 middle = Vector3.zero;
            foreach (KeyValuePair<PlayerScript, QTEPlayer> qp in attachedPlayers)
            {
                middle += qp.Key.transform.position;
            }
            middle /= 2;

            bool finished = true;
            foreach (KeyValuePair<PlayerScript, QTEPlayer> qp in attachedPlayers)
            {
                if (presses[qp.Key] == 0)
                {
                    finished = false;
                }
                else
                {
                    presses[qp.Key] = Mathf.MoveTowards(presses[qp.Key], 0, Time.deltaTime);
                }

                Transform t = qp.Key.transform;

                middle.y = t.position.y;

                if (Vector3.Distance(middle, t.position) > 0.45f)
                {
                    qp.Key.ApplyMovement(middle.x - t.position.x, middle.z - t.position.z);
                }
            }

            if (finished) { End(true); }
        }
        else
        {
            foreach (KeyValuePair<PlayerScript, QTEPlayer> qp in attachedPlayers)
            {
                int otherPlayerNumber = (Mathf.Abs(qp.Key.PLAYERNUM - 2));
                if (GameControllerScript.PlayerColors != null)
                {
                    GremlineHead.color = GameControllerScript.PlayerColors[otherPlayerNumber];
                }
                else
                {
                    Debug.Log(otherPlayerNumber);
                    GremlineHead.color = GameControllerScript.local.playerColorsTesting[otherPlayerNumber];
                }
            }
            GremlineHead.enabled = true;
        }

        in_range[0] = false;
        in_range[1] = false;

        if (PromptTimer[0] > 0)
        {
            PromptTimer[0] -= 1 * Time.deltaTime;
        }
        else
        {
            FadeTimer[0] -= 1f * Time.deltaTime;
            Color c = ThumbstickPrompt[0].GetComponent<Image>().color;
            c.a = FadeTimer[0];
            ThumbstickPrompt[0].GetComponent<Image>().color = c;
        }
        if (PromptTimer[1] > 0)
        {
            PromptTimer[1] -= 1 * Time.deltaTime;
        }
        else
        {
            FadeTimer[1] -= 1 * Time.deltaTime;
            Color c = ThumbstickPrompt[1].GetComponent<Image>().color;
            c.a = FadeTimer[1];
            ThumbstickPrompt[1].GetComponent<Image>().color = c;
        }
        if (FadeTimer[0] <= 0)
        {
            //ThumbstickPrompt[0].SetActive(false);
        }
        if (FadeTimer[1] <= 0)
        {
            //ThumbstickPrompt[1].SetActive(false);
        }
    }


    public override QTE_Info INFO()
    {
        float goal = 0; //Has it reached its goal?

        my_info._setup(GetComponent<RectTransform>(), goal, attachedPlayers.Count, pref_num); //Update the class that'll be passed to the interactor

        return my_info; //Return that class
    }

    //New player added, so add a text prompt above them
    protected override void PlayerAdded(PlayerScript player)
    {
        QTEPlayer p = attachedPlayers[player];
        presses[player] = 0;
        if (p.representation == null)
        {
            int dir = (p.myNum * 2) - 1;

            GameObject pointer = Instantiate(gremlinHeadPrefab);
            p.representation = pointer.GetComponent<RectTransform>();
            p.representation.transform.parent = this.transform;
            p.representation.transform.forward = this.transform.forward;

            p.percent = (float)dir;

            pointer.transform.GetChild(0).GetComponent<Image>().color = player.myColor;

            ThumbstickPrompt[p.myNum].SetActive(true);
            if (player.gamePad == true)
            {
                ThumbstickPrompt[p.myNum].GetComponent<Image>().sprite = thumbstick;
            }
            else
            {
                //ThumbstickPrompt[p.myNum].GetComponent<Image>().sprite = Keyboard;

                ThumbstickPrompt[p.myNum].GetComponent<Image>().sprite = (player.PLAYERNUM == 1) ? (keyboard[0]) : (keyboard[1]);
            }
        }

        if (attachedPlayers.Count == 2)
        {
            foreach (KeyValuePair<PlayerScript, QTEPlayer> qp in attachedPlayers)
            {
                qp.Key.myAnimator.SetTrigger("Hug");
            }
        }
    }
}
