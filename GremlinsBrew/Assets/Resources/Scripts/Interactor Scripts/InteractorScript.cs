using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

[RequireComponent(typeof(QTEPrefsScript))]
public class InteractorScript : MonoBehaviour
{
    //Moving the interactor (not currently utilized)
    private bool BEINGMOVED = false;
    public bool canBeMoved = true;

    public Interactables MYTYPE;
    protected int gridSpacesNeeded = 1;

    protected GameObject successParticle, spoiltParticle, purifiedParticle, corruptedParticle, loveheartParticle, spoiltSmokeParticle;
    protected GameObject tagIndicatorPrefab, electricalParticle;
    protected GameObject isPureParticle, isCorruptedParticle, isLovelyParticle;

    private GameObject mySnapPrefab = null, myDropTrail, myTagIndicator;

    private float cooldown = 0;
    protected float cooldownAmount = 0.7f;

    private float promptUpdateTimer = 0, promptUpdateTarget = 0.4f;

    [HideInInspector]
    public Camera mainCam;
    protected Collider col;
    private float boxHeight = -1;

    [SerializeField]
    protected Dictionary<PlayerScript, PlayerStats> availablePlayers = new Dictionary<PlayerScript, PlayerStats>(); //Players currently in range

    private PlayerScript[] lastInteractedPlayers; //Players to last interact with the station

    private QTEPrefsScript prefs; //Preferences for the QTE (used in inspector)
    [HideInInspector]
    public QTEScript currentQTE = null; //The current QTE being used by this interactor

    [SerializeField]
    protected Dictionary<ResourceType, ResourceEffect> container = new Dictionary<ResourceType, ResourceEffect>(); //Contains the ingredient
    protected string[] storeOptionNames;

    public static GameObject promptParent;

    // FMOD
    protected string essenceSuccessEventPath, essenceFailEventPath; // success and fail
    
    // Start is called before the first frame update
    void Start()
    {
        Init(Interactables.None, "Prefabs/GridSystem/SnapCauldron", 1);
        promptUpdateTimer = Random.Range(0f, promptUpdateTarget); //So all interactors update their prompts at different times
    }

    void OnDestroy()
    {
        
    }

    //Initialize the interactor
    protected void Init(Interactables interactableType, string p, int spacesNeeded)
    {
        // gets references to success and fail audio event paths
        essenceSuccessEventPath = FMOD_ControlScript.successEventPath;
        essenceFailEventPath = FMOD_ControlScript.failEventPath;

        MYTYPE = interactableType;

        col = GetComponent<Collider>();
        mySnapPrefab = Resources.Load(p) as GameObject;

        InteractorScript.promptParent = GameObject.FindGameObjectWithTag("PromptParent");

        gridSpacesNeeded = spacesNeeded;

        //Prefabs
        successParticle = Resources.Load("Prefabs/Particles/SuccessParticle") as GameObject;
        spoiltParticle = Resources.Load("Prefabs/Particles/SpoiltParticle") as GameObject;
        purifiedParticle = Resources.Load("Prefabs/Particles/EssencePurifyParticle") as GameObject;
        corruptedParticle = Resources.Load("Prefabs/Particles/EssenceCorruptParticle") as GameObject;
        loveheartParticle = Resources.Load("Prefabs/Particles/LoveParticleContainer") as GameObject;
        spoiltSmokeParticle = Resources.Load("Prefabs/Particles/SpoiltSmokeParticle") as GameObject;

        tagIndicatorPrefab = Resources.Load("Prefabs/Interactors/InteractorTagIndicator") as GameObject;
        electricalParticle = Resources.Load("Prefabs/Particles/ElectricalParticle") as GameObject; //Credit to: https://www.youtube.com/watch?v=o1EunoF-11c

        isPureParticle = Resources.Load("Prefabs/Particles/IsPureParticle") as GameObject;
        isCorruptedParticle = Resources.Load("Prefabs/Particles/IsCorruptedParticle") as GameObject;
        isLovelyParticle = Resources.Load("Prefabs/Particles/IsLovelyParticle") as GameObject; 

        prefs = GetComponent<QTEPrefsScript>();
        mainCam = Camera.main;

        //Assigns default dictionary values
        ResetContainerDictionary();
    }

    //Adds the ingredient to the container
    protected void Contain(ResourceType t)
    {
        container[t] = ResourceEffect.Normal;
    }

    protected void ResetContainerDictionary()
    {
        ResourceType[] typesArray = (ResourceType[])Enum.GetValues(typeof(ResourceType));

        foreach (ResourceType t in typesArray)
        {
            container[t] = ResourceEffect.Null;
        }
    }

    //Destroys this interactor
    public void DestroyMe()
    {
        //Free up the grid points
        for (int i = 0; i < gridPoints.Length; i++)
        {
            gridPoints[i].Occupied = false;
        }

        //Remove the machine
        Destroy(this.gameObject);
    }

    float fallPercentage = 1;
    float fallHeight = 12;
    public void Drop()
    {
        fallPercentage = 0;
        transform.position = (transform.position + Vector3.up * fallHeight);

        if (myDropTrail == null)
        {
            myDropTrail = Instantiate(Resources.Load("Prefabs/Interactors/InteractorDropTrail") as GameObject, transform.position, Quaternion.identity);
            myDropTrail.transform.parent = this.transform;
        }

    }

    //Bounces the interactor on the ground when spawned
    private float FancyLerp(float start, float end, float percent)
    {
        if (percent < 0.6f)
        {
            percent = (percent) / 0.6f;
        }
        else if (percent < 0.8f)
        {
            percent = 1 - (Mathf.Sin(((percent - 0.6f) / 0.8f) * Mathf.PI * 0.5f) * 0.2f);
        }
        else
        {
            percent = Mathf.Sin(percent * Mathf.PI * 0.5f);
        }

        return Mathf.Lerp(start, end, percent);
    }

    void FixedUpdate()
    {
        UpdatePromptFade(); //Updates the prompts fading in when players are nearby

        OverridenUpdate(); //This called so child interactors can have an update method (there's lots of inheritence at work in this script)

        //If falling
        if (fallPercentage < 1 && GameControllerScript.local.BUYMENU.gameObject.activeSelf == false && GameControllerScript.local.DayEndMenu.activeSelf == false)
        {
            bool smallerThan0_6 = (fallPercentage < 0.6f);

            fallPercentage = Mathf.MoveTowards(fallPercentage, 1, 0.015f);
            Vector3 pos = transform.position;
            pos.y = FancyLerp((Vector3.up * fallHeight).y, 1, fallPercentage);
            transform.position = pos;

            if (smallerThan0_6 && fallPercentage > 0.6f) {

                //Small shake of the camera
                GameControllerScript.ShakeCameraMini();

                // sfx for when iteractor lands on the ground
                FMOD_ControlScript.PlaySoundOneShot(FMOD_ControlScript.fmodData.interactorLandEventPath, transform.position); 

                if (myDropTrail != null) { Destroy(myDropTrail, 2f); }
            }
        }

        //Cooldown for interaction
        if (cooldown > 0)
        {
            cooldown = Mathf.MoveTowards(cooldown, 0, Time.deltaTime);
        }

        //Cancels QTE at night
        if (currentQTE != null)
        {
            if (GameControllerScript.DayNight.nightAmount < 1 || canBeMoved == false)
            {
                OnQTEUpdate(currentQTE.INFO());
            }
            else
            {
                currentQTE.StopEarly();
            }
        }

        //Updates the prompt
        promptUpdateTimer += Time.fixedDeltaTime;
        if (promptUpdateTimer >= promptUpdateTarget)
        {
            promptUpdateTimer -= promptUpdateTarget;

            UpdatePrompt();
        }
    }

    //Can be overriden. Is called while a QTE is in progress
    protected virtual void OnQTEUpdate(QTE_Info info) { }

    //Can be overriden. General updating
    protected virtual void OverridenUpdate() { }

    /// <summary>
    /// Should be called in Update()
    /// </summary>
    protected void UpdatePromptFade()
    {
        foreach (KeyValuePair<PlayerScript, PlayerStats> s in availablePlayers)
        {
            PlayerStats stats = s.Value;
            Sprite currentSprite = GameControllerScript.BtnPromptSprite(s.Key,stats.currentBtnPrompt);           

            bool doShowPrompt = (currentSprite != null) && s.Key.myState != PlayerState.QTE;
            doShowPrompt = doShowPrompt && cooldown == 0 && (GameControllerScript.NightProgress == 0 || !canBeMoved);

            if (doShowPrompt)
            {
                if (stats.promptSrite == null)
                {
                    stats.RecreatePrompt(s.Key, this);
                    //RemovePlayer(s.Key);
                    break;
                }

                //Continude displaying this sprite
                stats.promptSrite.Display(currentSprite);

                stats.PromptPosition(s.Key.transform.position, this.transform.position);

                //stats.PromptAlpha((doShowPrompt) ? 1 : 0);
            }
        }

        //Debug.Log("availablePlayers size: " + availablePlayers.Count);
        //Debug.Log("abandonedPlayers size: " + abandonedPlayers.Count);
    }

    //DO NOT OVERRIDE THIS METHOD
    private void UpdatePrompt()
    {
        //Prompt becomes visibale if it has a player targetting it
        if (prefs != null && cooldown == 0 && (GameControllerScript.NightProgress == 0 || !canBeMoved))
        {
            foreach (KeyValuePair<PlayerScript, PlayerStats> playerPair in availablePlayers)
            {
                PlayerScript player = playerPair.Key;
                PlayerStats stats = playerPair.Value;

                if (player.myState == PlayerState.QTE)
                {
                    continue; //Skip this player if they're in a QTE already
                }

                //stats.TextTarget = "";
                stats.currentBtnPrompt = BtnPromptEnum.None;

                if (stats.holdingItem != null)
                {
                    int so = StoreOption(player, stats);

                    //If this interactor has a storing option
                    if (so >= 0) {
                        //stats.TextTarget += String.Format("{0} = {1}", butR, storeOptionNames[so]);
                        stats.currentBtnPrompt = BtnPromptEnum.InteractBtn;
                    }
                }
                else
                {
                    int i = Interact1Option(stats);
                    if (i > -1)
                    {
                        stats.currentBtnPrompt = prefs.BtnPromptOf(i);
                    }
                    i = Interact2Option(stats);
                    if (i > -1)
                    {
                    }
                }
            }
        }
    }

    public void CreateParticle(GameObject particle, float time)
    {
        Destroy(Instantiate(particle, transform.position, Quaternion.identity), time);
    }

    /// <summary>
    /// Virtual method can be overriden (such as by Cauldron to allow storing). Returns true if storing was successful
    /// </summary>
    /// <param name="store"></param>
    /// <param name="Player"></param>
    /// <param name="storeOption"></param>
    /// <returns>Storing was successful</returns>
    protected virtual bool Store(ItemScript store, PlayerScript Player, int storeOption)
    {
        return false; //Player is unable to store the resource here
    }

    //DO NOT OVERRIDE THIS SCRIPT
    public bool StoreRequest(ItemScript store, PlayerScript player)
    {
        if (availablePlayers.ContainsKey(player)) {
            bool s = Store(store, player, StoreOption(player, availablePlayers[player]));

            UpdatePrompt();

            return s;
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    /// Override to return which store option is going to be used based on the player's stats
    /// (The player holding an item already is assumed)
    /// </summary>
    /// <param name="p"></param>
    /// <param name="stats"></param>
    /// <returns>And integer for the number of the option (defaults to -1)</returns>
    protected virtual int StoreOption(PlayerScript p, PlayerStats stats)
    {
        return -1; //No storing options
    }

    //TODO: Have a list in QTE prefs, and simple call a different one based on the interact options?
    protected virtual int Interact1Option(PlayerStats stats)
    {
        return -1;
    }

    protected virtual int Interact2Option(PlayerStats stats)
    {
        return -1;
    }

    //The QTE was a success
    protected virtual void Success(int pref_num) { }

    //The QTE was a failure
    protected virtual void Fail(int pref_num) { }

    //Player quit the QTE - defaults to fail method
    protected virtual void StoppedQTE(int pref_num) { Fail(pref_num); }

    //DON'T OVERRIDE THIS METHOD
    public void SuccessRequest(int pref_num, PlayerScript[] players)
    {
        cooldown = cooldownAmount;
        lastInteractedPlayers = players;
        Success(pref_num);
        UpdatePrompt();

        Debug.Log("cooldown = " + cooldown);

        SetCurrentQTE(null, players);
    }

    //DON'T OVERRIDE THIS METHOD
    public void FailRequest(int pref_num, PlayerScript[] players)
    {
        cooldown = cooldownAmount;
        lastInteractedPlayers = players;
        Fail(pref_num);
        UpdatePrompt();

        SetCurrentQTE(null, players);
    }

    //DON'T OVERRIDE THIS METHOD
    public void StopRequest(int pref_num, PlayerScript[] players)
    {
        StoppedQTE(pref_num);
        lastInteractedPlayers = players;
        UpdatePrompt();

        Debug.Log("Here");
        SetCurrentQTE(null, players);
    }

    /// <summary>
    ///Adds player to the list of available players for interaction
    /// </summary>
    /// <param name="p"></param>
    public void AddPlayer(PlayerScript p)
    {

        if ((GameControllerScript.NightProgress < 1 || !canBeMoved))
        {
            if (availablePlayers.ContainsKey(p))
            {
                availablePlayers[p].UpdateStats(p);
            }
            else
            {
                //availablePlayers[p] = new PlayerStats(p, this);
                availablePlayers.Add(p, new PlayerStats(p));
            }
        }
        UpdatePrompt();
    }

    //Removes player from the array
    public void RemovePlayer(PlayerScript p)
    {
        if (availablePlayers.ContainsKey(p))
        {
            //Moves the player stats to the abandonded player dictionary, where it'll remove it once the text has faded
            availablePlayers.Remove(p);
        }

        UpdatePrompt();
    }

    public void UpdateStats(PlayerScript p)
    {
        if (availablePlayers.ContainsKey(p))
        {
            availablePlayers[p].UpdateStats(p);
        }

        UpdatePrompt();
    }

    public PlayerScript[] LastInteractedPlayers
    {
        get
        {
            return lastInteractedPlayers;
        }

        set
        {
            lastInteractedPlayers = value;
        }
    }

    /// <summary>
    /// Immediatelly triggers the interaction, usually when an ingredient is placed
    /// </summary>
    /// <param name="player"></param>
    protected void ImmediateInterface(PlayerScript player)
    {
        player.SetMyQTE(this.InterfaceInteractor(true, player));
    }

    /// <summary>
    /// Interacts with the interactor, returns a quick time object if applicable
    /// </summary>
    /// <param name="type1">Whether is interaction 1 or 2</param>
    /// <returns></returns>
    public QTEScript InterfaceInteractor(bool interact1, PlayerScript player)
    {
        Debug.Log("Checking for QTE creation");

        if (availablePlayers.ContainsKey(player) && prefs != null && player.myState != PlayerState.QTE 
                && cooldown == 0 && (GameControllerScript.NightProgress == 0 || !canBeMoved))
        {
            int pref_num = (interact1) ? Interact1Option(availablePlayers[player]) : Interact2Option(availablePlayers[player]);

            //Break if there is a restriction stopping use of that interaction for the moment
            if (pref_num == -1) { return null; }

            Debug.Log("Checks passed. Creating Quick Time Event.");

            //Ask the qte prefs script (attached to this object) to create a new QTE object for us, of the type specified by pref_num
            QTEScript newQTE = prefs.CreateQTE(mainCam, pref_num, player.transform.position, this, player);
            if (newQTE == null) { return newQTE; }

            newQTE.mainCam = mainCam;

            SetCurrentQTE(newQTE, new PlayerScript[]{ player });
            return newQTE;
        }

        //Else, default to drop spitting out items? Or something else?

        Debug.Log("No QTE :(");

        return null;
    }

    protected virtual void SetCurrentQTE(QTEScript v, PlayerScript[] player)
    {
        currentQTE = v;
    }

    public Vector3 ClosestPoint(Vector3 pos)
    {
        if (col == null)
        {
            col = GetComponent<Collider>();
        }
        
        return col.ClosestPoint(pos);
    }

    public float BoxHeight
    {
        get
        {
            if (boxHeight < 0)
            {
                boxHeight = GetComponent<Collider>().bounds.extents.y;
            }

            return boxHeight;
        }
    }

    //*************************************************
    //Moving the interactor
    public GameControllerScript.GridPoint[] gridPoints = null;
    void LateUpdate()
    {
        //GameObject snap = this.SnapObj;
    }

    public InteractorScript TagInteractorMove(int gridSpots)
    {
        if (BEINGMOVED) { return null; }
        if (gridSpots > 0 && gridSpots != this.gridSpacesNeeded) { return null; }

        //TODO: Show tag somehow, and add to a list for player??

        myTagIndicator = Instantiate(tagIndicatorPrefab, transform.position + Vector3.up, Quaternion.identity);

        BEINGMOVED = true;
        return this;
    }

    public void UntagInteractor()
    {
        BEINGMOVED = false;
        if (myTagIndicator != null) { Destroy(myTagIndicator); }
    }

    //How many grids spaces this interactor takes up
    public int GridSpacesNeeded
    {
        get => gridSpacesNeeded;
    }

    //*************************************************

        /// <summary>
        /// This class is current information about the player, so the prompt can display accordingly
        /// </summary>
    [SerializeField]
    protected class PlayerStats
    {
        public PlayerScript player;
        public ItemScript holdingItem;
        public bool isPotionBottle;

        public PromptSpriteScript promptSrite;
        public BtnPromptEnum currentBtnPrompt = BtnPromptEnum.InteractBtn;

        public Vector3 UP5 = Vector3.up * 3.5f;

        private GameObject spriteObjPrefab = Resources.Load("Prefabs/PromptSprite") as GameObject;
        public PlayerStats(PlayerScript p)
        {
            player = p;
            UpdateStats(p);
        }

        public void RecreatePrompt(PlayerScript p, InteractorScript interactor)
        {

            promptSrite = Instantiate(spriteObjPrefab, interactor.transform.position + UP5, Quaternion.identity).GetComponent<PromptSpriteScript>();


            //Assigns the text popup prompt
            if (promptSrite != null)
            {
                promptSrite.transform.forward = Camera.main.transform.forward;
                promptSrite.transform.SetParent(InteractorScript.promptParent.transform);
                promptSrite.name = interactor.name + "'s prompt for " + p.name;
            }
        }

        //For prompt display
        public void UpdateStats(PlayerScript p)
        {
            holdingItem = (p.IsHolding()) ? p.Holding.GetComponent<ItemScript>() : null;
            isPotionBottle = (holdingItem != null && holdingItem.tag.Equals("PotionBottle"));
        }

        //Prompt position
        public void PromptPosition(Vector3 playerPos, Vector3 interactorPos)
        {
            Vector3 pos = ((interactorPos + playerPos) / 2) + UP5;

            promptSrite.transform.position = Vector3.MoveTowards(promptSrite.transform.position, pos, 0.5f);
        }

        public void PromptPosition(Vector3 interactorPos)
        {
            promptSrite.transform.position = Vector3.MoveTowards(promptSrite.transform.position, interactorPos + UP5, 0.5f);
        }
    }
}

//What type of button is it?
public enum BtnPromptEnum
{
    None, InteractBtn
}
