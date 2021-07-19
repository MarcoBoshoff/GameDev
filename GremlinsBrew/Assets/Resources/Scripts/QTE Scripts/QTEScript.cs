using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class QTEScript : MonoBehaviour
{
    //IF CREATING A NEW QTE_???Script, remember to also make a new prefab for it in Resources/Prefabs/Quick_Time_Events/
    //Then, create a new type for it below
    //Finally, add that type and prefab to the dictionary in the QTEPrefsScript. Line ~27

    [HideInInspector]
    public Camera mainCam;
    private bool createdText = false;

    [HideInInspector]
    public RectTransform rect;

    //Important variables for returning upon success/failure
    public int pref_num;
    [HideInInspector]
    public InteractorScript myInteractor;

    protected QTE_Info my_info;

    //Prefabs
    protected GameObject textPrefab, playerSparklePrefab, axesPointTargetPrefab, lightGreyBoxPrefab;

    [SerializeField]
    protected Dictionary<PlayerScript, QTEPlayer> attachedPlayers = new Dictionary<PlayerScript, QTEPlayer>(); //All players in QTE

    public int PlayersNeeded = 1;

    [SerializeField]
    protected Text playersText;

    Vector3 startPos;
    protected bool doBubble = true;
    protected float offsetMultiplier = 50;
    protected Vector3 offsetAddition = Vector3.zero;

    //TIMER
    protected float TIMER = 0;

    // Start is called before the first frame update
    public void Init()
    {
        my_info = new QTE_Info();

        //Init
        //SetForward();
        //transform.Translate(-transform.forward.normalized, Space.World);

        //Prefabs
        textPrefab = Resources.Load("Prefabs/Quick_Time_Events/qteText") as GameObject;
        playerSparklePrefab = Resources.Load("Prefabs/Quick_Time_Events/PlayerSparkle") as GameObject;
        axesPointTargetPrefab = Resources.Load("Prefabs/Quick_Time_Events/AxisPointTarget") as GameObject;
        lightGreyBoxPrefab = Resources.Load("Prefabs/Quick_Time_Events/LightGreyBox") as GameObject;

        startPos = rect.position;
        SetupStarting();
    }

    //Override for any values before setup starts
    public virtual void SetupStarting() { }

    //Override for any values after setup has finished
    public virtual void SetupFinished() { }

    // Update is called once per frame
    void Update()
    {
        if (attachedPlayers == null)
        {
            Destroy(gameObject); //Destory if no players
        }

        int playerCount = attachedPlayers.Count; //Number of players in this QTE

        //PlayerScript other = WhichPlayerNotInDict(attachedPlayers);
        if (doBubble)
        {
            Vector3 targetPosition = startPos + offsetAddition;

            QTEScript otherQTE = FindOtherQTE();
            if (otherQTE != null)
            {
                Vector3 otherPos = otherQTE.rect.position;
                float moveAmount = (1 - (Vector2.Distance(targetPosition, otherPos) / 370)) * offsetMultiplier;
                targetPosition += (startPos - otherPos).normalized * moveAmount;
                //rect.position = Vector3.MoveTowards(rect.position,  + startPos, 0.7f);
            }

            if (rect.position != targetPosition)
            {
                rect.position = Vector3.Lerp(rect.position, targetPosition, 0.15f);
                if (Vector3.Distance(rect.position, targetPosition) < 0.5f)
                {
                    rect.position = targetPosition;
                }
            }
        }

        //Call update
        //SetForward();
        QteUpdate(playerCount);

        //GetComponent<RectTransform>().pos

        //Controls the position of this QTE
        //ControlPosition();

        //Reset some values
        foreach (KeyValuePair<PlayerScript, QTEPlayer> p in attachedPlayers)
        {
            p.Value.fireButtonDown[0] = false;
            p.Value.fireButtonDown[1] = false;
        }
    }

    protected QTEScript FindOtherQTE()
    {
        Transform qteParent = GameControllerScript.local.QTEUIParent;
        int childCount = qteParent.childCount;

        for (int i = 0; i < childCount; i++)
        {
            QTEScript temp = qteParent.GetChild(i).GetComponent<QTEScript>();

            if (temp != this) { return temp; }
        }

        return null;


    }

    //protected PlayerScript WhichPlayerNotInDict(Dictionary<PlayerScript, QTEPlayer> dict)
    //{
    //    PlayerScript tempPlayer = GameControllerScript.local.Player1;
    //    if (!dict.ContainsKey(tempPlayer))
    //    {
    //        return tempPlayer;
    //    }

    //    tempPlayer = GameControllerScript.local.Player2;
    //    if (!dict.ContainsKey(tempPlayer))
    //    {
    //        return tempPlayer;
    //    }

    //    return null;
    //}

    //private void ControlPosition()
    //{
    //    transform.position = Vector3.Project(startPos - mainCam.transform.position, mainCam.transform.right) + mainCam.transform.position;
    //    transform.position += mainCam.transform.forward * 10;
    //}

    //Receive Movement Input
    public virtual void MovementInput(PlayerScript player, Vector2 movement) {}

    protected virtual void QteUpdate(int playerCount)
    {
        if (attachedPlayers.Count >= PlayersNeeded)
        {
            End(true); //Ends the QTE
        }
    }

    public int PlayersRemaining
    {
        get
        {
            return PlayersNeeded - attachedPlayers.Count;
        }
    }

    //Override for button pressed
    public virtual void ButtonPressed(PlayerScript player, GamepadEnum btn)
    {
        //attachedPlayers[player].fireButtonDown[fireNum] = true;
    }

    //All these can be overriden
    public virtual void SetFloat(float t) {}

    public virtual void SetBool(bool b) {}

    public virtual void SetInt(int i) {}

    public virtual void AddPoint(Vector3 point) {}
    public virtual void AddPointDual(Vector3 pointL, bool SecondStick, Vector3 pointR, Vector2 Values) { }

    //This is called when a new player has been added
    protected virtual void PlayerAdded(PlayerScript p)
    {
        if (attachedPlayers.Count < PlayersNeeded && !createdText)
        {
            playersText.text = string.Format("Needs {0} players", PlayersNeeded);
        }
    }

    //Override this one
    public virtual void Interrupt(int id, int i)
    {
        if (id == 0) { End(true); }
    }

    //Don't override this one
    public void Interrupt(int id) { Interrupt(id, 0); }

    //Override this to create your own text objects with the passed string
    public virtual void CreateText(string text, PlayerScript p)
    {
        GameObject temp = Instantiate(textPrefab, transform.position, Quaternion.identity) as GameObject;
        temp.transform.parent = this.transform;

        Text textScript = temp.transform.GetChild(0).GetComponent<Text>(); //Example of how to assign text component to a variable
        textScript.text = text;

        temp.transform.forward = mainCam.transform.forward;
    }

    //DON'T OVERRIDE - sets the value whether the player is holding down a fire/interact button (R1/R2)
    public void FireHeld(PlayerScript player, int fireNum)
    {
        attachedPlayers[player].fireButtonDown[fireNum] = true;
    }

    //DON'T OVERRIDE THIS METHOD
    public void AttachPlayer(PlayerScript p)
    {
        attachedPlayers[p] = new QTEPlayer((int)p.PLAYERNUM-1);

        PlayerAdded(p);
    }

    //If the interactor needs to know info about the current QTE
    public virtual QTE_Info INFO()
    {
        float goal = 0; //Has it reached its goal?

        my_info._setup(GetComponent<RectTransform>(), goal, attachedPlayers.Count, pref_num); //Update the class that'll be passed to the interactor

        return my_info; //Return that class
    }

    public virtual void StopEarly()
    {
        End(false);
    }

    /// <summary>
    /// Called when the QTE is finished
    /// </summary>
    /// <param name="success">Whether the QTE was a success or a failure</param>
    protected void End(bool success)
    {
        Debug.Log("Ending QTE");

        PlayerScript[] players = new PlayerScript[attachedPlayers.Count];
        int i = 0;
        foreach (KeyValuePair<PlayerScript, QTEPlayer> pq in attachedPlayers)
        {
            players[i] = pq.Key;
            i++;
        }

        if (success)
        {
            myInteractor.SuccessRequest(pref_num, players);
        }
        else
        {
            myInteractor.FailRequest(pref_num, players);
        }

        Destroy(gameObject);
    }


    //===CLASS FOR ATTACHED PLAYERS===
    protected class QTEPlayer
    {
        public int myNum = -1;
        public QTEPlayer(int handyInt)
        {
            myNum = handyInt;
        }

        //HOLD
        public bool[] fireButtonDown = new bool[2]; //0 = fire1, 1 = fire2

        //AXIS POINTS
        public RectTransform representation = null;
        public RectTransform representation2 = null;

        //Rapid
        public int progress = 0;
        public Text myText;
        public float percent = 0;
    }
}

public class QTE_Info
{
    private float goal = 0;
    private int player_count = 0, pref_num = 0;//, counter;
    private RectTransform myTransform;
    private Image img;

    public float Goal
    {
        get
        {
            return goal;
        }
    }

    public int PlayerCount
    {
        get
        {
            return player_count;
        }
    }

    public int PrefNum
    {
        get
        {
            return pref_num;
        }
    }

    public RectTransform GetRectTransform()
    {
        return myTransform;
    }

    public Image IMG
    {
        get { return img; }
    }

    //public int InfoCounter
    //{
    //    get
    //    {
    //        return counter;
    //    }
    //}

    public void _setup(RectTransform t, float goal, int player_count, int pref)
    {
        this.goal = goal;
        this.player_count = player_count;
        this.myTransform = t;
        this.pref_num = pref;
    }

    public void _update(float goal, int player_count)
    {
        this.goal = goal;
        this.player_count = player_count;
    }

    public void _update(Image img)
    {
        this.img = img;
    }
}

[System.Serializable]
public enum QTEType
{
    None, InstantSuccess, Hold, Memory, Rapid, Thumbstick, Rotate, Cook, Draw, BoltCharge, Catcher, Hug, RotateCrush
};
