using EZCameraShake;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using InControl;
using Random = UnityEngine.Random;

[RequireComponent(typeof(ScoreScript))]
public class GameControllerScript : MonoBehaviour
{
    public static GameControllerScript local; //Local references
    public static System.Random random;

    public bool dynamicCamera = true;
    public float cameraDistance = 12;
    private Vector3 camMoveBack;

    public static String[] PlayerNames;
    public static int[] PlayerOutfits;
    public static Color[] PlayerColors;
    public Color[] playerColorsTesting;
    public static Material[] playerCostumeMat;

    //Grid System*********************************************
    public int gridSpacing;
    public Vector2 gridTopLeft;
    private GridPoint[,] GridSystem;
    public int gridIndentAt = -1;
    //********************************************************

    //Platform quadrants**********************************
    private List<PlatformGroup> platformGroups;
    public bool[] CustomGrid;
    public int[] StartingInteractors;
    public Vector2Int cgSize; //Custom grid width & height
    //****************************************************

    //FMOD Variables******************************************
    private FMOD_ControlScript fmod_link;
    private FMOD.Studio.ParameterInstance dayNightCycleParam;
    public float dayNightCycleParamOut = 0;
    private FMOD.Studio.EventInstance music; // for local reference to bg music
    public int MasterVolume = 100;
    //********************************************************

    //Scoring & Curse System**********************************
    public ScoreScript ScoreControl;
    public CurseSystemScript curseSystem;
    public UIScript UI;
    public static int dailyScore = 0;
    //********************************************************

    public BuySelection BUYMENU; //MENUS!
    public static CustomerControlScript customerControl;

    //Night & Day + time stuff **********
    [SerializeField]
    private DayNightScript dayNightScript;
    public static float NightProgress = 0, secondsThisDay = 0; //0 for day, 1 for night, inbetween for transitioning
    private static float previousProgress = 0;
    public static int minutesThisDay = 0;
    public bool CanRepair = true;
    private int currentRepairCost = 0;
    //***********************************

    //Flashing red lights
    private Light[] lanternLights;
    private float redFlash = 1, redFlashTarget = 1;
    public Color redFlash1, redFlash0;

    //Displays unison juice text
    private float unison = 0;

    [SerializeField]
    public List<GameObject> existingInteractors;
    private GameObject[] breakagePoints;
    public static bool dayUITrigger = false;

    public int STARTLIVES = 3, DayCap = 7;
    private int lives; //Lives
    public int maxPotions; //Maximum number of potions

    //TODO: Make this a serialized dictionary like the tutorial
    public float PoisonPotionPrice = 10f, LovePotionPrice = 10f, ManaPotionPrice = 10f, FourthPotionPrice = 10f;

    //Coins
    public List<GameObject> Coins = new List<GameObject>();
    public CoinCounterScript coinCounter;
    public int startCoins = 10;

    //bool for tutorials that pauses/unpauses the game
    public bool tutorialActive, gamePaused, PlayerPause;

    //PREFABS
    public static GameObject platformPrefab;
    public static GameObject juicyTextPrefab;

    //Player References
    public PlayerScript Player1, Player2;
    public Transform DepositChute;
    public BedScript bedScript1, bedScript2;
    public VendingControlScript vendingControlScript;
    public Transform QTEUIParent, BooksParent;

    //Camera shake
    public float shake_distance = 20, shake_roughness = 7, shake_fadeout = 1.2f;

    //Layers
    public LayerMask environmentLayer, wallInvisLayer;

    //Particles
    public GameObject overlayCamera, PauseMenu, DayEndMenu, PaxGameOver;
    public Dictionary<ResourceType, GameObject> overlayParticleLookup = new Dictionary<ResourceType, GameObject>();

    //Highlight Colours
    public Color interactorHighlight, resourceHighlight;

    //************************************************************************************************************
    //DICTIONARIES
    //************************************************************************************************************

    //TODO: Matt... We do have a soldItems counter...
    public Dictionary<ResourceType, int> soldItems = new Dictionary<ResourceType, int>();


    [HideInInspector]
    //public Dictionary<string, ResourceType[]> resourceClass = new Dictionary<string, ResourceType[]>();
    public List<ResourceType> currentUnlockedPotion;
    public List<ResourceType> PotionsCatalog;

    [HideInInspector]
    public Dictionary<ResourceType, GameObject> gamePrefabs = new Dictionary<ResourceType, GameObject>();

    [HideInInspector]
    public Dictionary<ResourceType, float> PotionPrice = new Dictionary<ResourceType, float>();

    [HideInInspector]
    private Dictionary<Interactables, GameObject> Interactors = new Dictionary<Interactables, GameObject>();

    public Sprite btnSprite_Interact;
    public Sprite key1Sprite_Interact;
    public Sprite key2Sprite_Interact;
    public Sprite PSSprite_Interact;

    public static int[] PSController = {0,0}; //Which controller is being used
    //************************************************************************************************************
    //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^


    //==========================
    //=== START ===
    void Start()
    {
        GameControllerScript.local = this;
        GameControllerScript.random = new System.Random();

        lives = STARTLIVES;

        PauseMenu.SetActive(true);

        //Populate Dictionaries
        GameControllerScript.ResetStockLevels();
        GameControllerScript.PopulatePrefabsDictionary();
        GameControllerScript.PopulateInteractorDictionary();
        GameControllerScript.PopulatePotionsCatalog();
        GameControllerScript.PopulatePotionPriceDictionary();

        //Player references
        Player1 = GameObject.FindGameObjectWithTag("Player 1").GetComponent<PlayerScript>();
        Player2 = GameObject.FindGameObjectWithTag("Player 2").GetComponent<PlayerScript>();

        //Score System
        ScoreControl = GetComponent<ScoreScript>();
        ScoreControl.Init(Player1, Player2);

        GameControllerScript.customerControl = GetComponent<CustomerControlScript>();

        //Lanterns
        GameObject[] temp = GameObject.FindGameObjectsWithTag("LanternLight");
        lanternLights = new Light[temp.Length];
        for (int i = 0; i < temp.Length; i++)
        {
            lanternLights[i] = temp[i].GetComponent<Light>();
        }

        //Static prefabs
        GameControllerScript.platformPrefab = Resources.Load("Prefabs/GridSystem/PlatformPrefab") as GameObject;
        GameControllerScript.juicyTextPrefab = Resources.Load("Prefabs/JuicyText") as GameObject;
        Instantiate(Resources.Load("Prefabs/Ron/Ron") as GameObject);

        //Particle Prefabs
        overlayParticleLookup[ResourceType.HealthPotion] = Resources.Load("Prefabs/Particles/BoughtHealthParticle") as GameObject;
        overlayParticleLookup[ResourceType.PoisonPotion] = Resources.Load("Prefabs/Particles/BoughtPoisonParticle") as GameObject;
        overlayParticleLookup[ResourceType.LovePotion] = Resources.Load("Prefabs/Particles/BoughtLoveParticle") as GameObject;

        //Existing interactors
        existingInteractors = new List<GameObject>();
        GameObject[] interactorsInGame = GameObject.FindGameObjectsWithTag("Interactable");
        foreach (GameObject existing in interactorsInGame) { existingInteractors.Add(existing); }

        //Breakage Points
        breakagePoints = GameObject.FindGameObjectsWithTag("Breakage");

        //Vending Lines
        vendingControlScript.Setup();

        //Initialize the grid
        GameControllerScript.INITGRID();

        //Start the tutorial
        if (!tutorialActive)
        {
            TutorialScript.expire = true; //Turns off all future tutorial prompts
        }
        else
        {
            TutorialScript.NewTutorial(TutorialType.Day);
        }

        coinCounter.CoinsToSpawn += startCoins;

        //curseSystem = GetComponent<CurseSystemScript>();
        //curseSystem.Init(this);

        //Buy Menus
        BUYMENU.Init();
        //BUYMENU.Init();

        //FMOD LINK
        fmod_link = GameObject.FindGameObjectWithTag("FMODControl").GetComponent<FMOD_ControlScript>();
        fmod_link.Init(); //Initialize the fmod communicator

        // Starts Background Music
        music = FMOD_ControlScript.FEbgMusic;
        if (!FMOD_ControlScript.PlaybackState(music))
        {
            music.start();
        }

        FMOD_ControlScript.FEbgMusic.getParameter("DayNightCycle", out dayNightCycleParam); // parameter for changing from day to night in FMOD

    }
    //=== UPDATE ===   
    void Update()
    {
        TrackDay();
        //WinCondition();

        if (InputManager.ActiveDevice.Command.WasPressed || Input.GetKeyDown(KeyCode.Escape))
        {
            PlayerPause = true;

            PauseGame();
        }

        if (dynamicCamera) { MoveCamera(); }

        //Managing flashing red lights
        if (lives == 0)
        {
            if (redFlash == redFlashTarget)
            {
                redFlashTarget = Mathf.Abs(redFlashTarget - 1); //Swap between 0 & 1
            }

            redFlash = Mathf.MoveTowards(redFlash, redFlashTarget, 0.05f);

            Color c = Color.Lerp(redFlash0, redFlash1, redFlash);

            foreach (Light lantern in lanternLights)
            {
                lantern.color = c;
            }
        }

        //Update static night progress
        GameControllerScript.NightProgress = dayNightScript.nightAmount;

        bool movingPlatformGroups = false;
        //Platforms
        foreach (PlatformGroup pq in GameControllerScript.local.platformGroups)
        {
            movingPlatformGroups = pq.Update() || movingPlatformGroups;
        }

        if (movingPlatformGroups)
        {
            foreach (GameObject interactor in existingInteractors)
            {
                InteractorScript interactorScript = interactor.GetComponent<InteractorScript>();
                if (interactorScript.gridPoints != null)
                {
                    float height = GameControllerScript.GetGridHeight(interactorScript.gridPoints[0]);

                    Vector3 pos = interactor.transform.position;
                    pos.y = height + interactor.GetComponent<InteractorScript>().BoxHeight;
                    interactor.transform.position = pos;
                }
            }
        }

        //PAX Debug Keys
        //Restart Game Level
        if (Input.GetKeyDown(KeyCode.F10))
        {
            StopSounds();
            FMOD_ControlScript.SetPauseParam(0);
            SceneManager.LoadScene(2);

        }
        //Restart Game
        if (Input.GetKeyDown(KeyCode.F11))
        {
            StopSounds();
            FMOD_ControlScript.SetPauseParam(0);
            FMOD_ControlScript.StopAudio(FMOD_ControlScript.FEbgMusic);
            SceneManager.LoadScene(0);
        }
        //Quit or Alt+F4
        if (Input.GetKeyDown(KeyCode.F12))
        {
            StopSounds();
            FMOD_ControlScript.StopAudio(FMOD_ControlScript.FEbgMusic);
            Application.Quit();
        }
    }

    //Stop QTEs. QTES create sound so this forces them to stop.
    public void StopSounds()
    {
        if (Player1.GetComponent<PlayerScript>().QTE != null)
        {
            Player1.GetComponent<PlayerScript>().QTE.StopEarly();
        }
        if (Player2.GetComponent<PlayerScript>().QTE != null)
        {
            Player2.GetComponent<PlayerScript>().QTE.StopEarly();
        }
    }
    void FixedUpdate()
    {
        if (unison > 0)
        {
            unison = Mathf.MoveTowards(unison, 0, Time.fixedDeltaTime);
        }
    }

    public static void TrackDay()
    {
        secondsThisDay += Time.deltaTime;
        if (secondsThisDay >= 60)
        {
            secondsThisDay -= 60;
            minutesThisDay++;
        }

        //Day
        if (NightProgress == 1 && previousProgress != 1)
        {
            //Now night

            FMOD_ControlScript.actionsp1 = 0;
            FMOD_ControlScript.actionsp2 = 0;
            minutesThisDay = 0;
            secondsThisDay = 0;
            UpdatePlayerMelodies();
        }

        // transition day music to night music and vice versa (if NightProgress = 0 then it becomes day. If it is 1 then it becomes night
        else if (NightProgress == 0 && previousProgress != 0)
        {
            dayUITrigger = true;

            // transition night music to day music
        }

        local.dayNightCycleParam.setValue(NightProgress);

        previousProgress = NightProgress;
    }

    //Shows the night menu...
    public static void ShowNightMenu()
    {
        local.StopSounds();
        if (local.DayEndMenu != null)
        {
            //local.gamePaused = true;
            local.DayEndMenu.SetActive(true);

            NightMenuScript D = local.DayEndMenu.GetComponent<NightMenuScript>();
            D.buttonSelected[0] = 0;
            D.buttonSelected[1] = 0;

            D.UpdatePotionIcons();

        }
    }

    //Pauses the game
    public static void PauseGame()
    {
        local.StopSounds();
        if (!local.gamePaused && local.PlayerPause)
        {
            local.gamePaused = true;
            local.PauseMenu.SetActive(true);
            

            PauseMenuScript p = local.PauseMenu.GetComponent<PauseMenuScript>();
            p.UpdateControlUI();
            p.buttonSelected = 0;
            p.Highlight();

            // pause menu pop up sfx
            // turn main music down
            FMOD_ControlScript.SetPauseParam(1);
        }
        else
        {
            local.PauseMenu.GetComponent<PauseMenuScript>().OptionsMenuActive = false;
            local.PauseMenu.GetComponent<PauseMenuScript>().PauseMenu.SetActive(true);
            local.PauseMenu.GetComponent<PauseMenuScript>().OptionsMenu.SetActive(false);
            local.PauseMenu.GetComponent<PauseMenuScript>().HidePause();

            local.gamePaused = false;
            local.PlayerPause = false;
            local.PauseMenu.SetActive(false);

            // pause menu close sfx
            // turn main music up
            FMOD_ControlScript.SetPauseParam(0);
        }
    }

    //==========================

    //==========================
    // === POPULATE LISTS ===
    //==========================
    private static void PopulatePrefabsDictionary()
    {
        local.gamePrefabs[ResourceType.Flower] = Resources.Load("Prefabs/ResourcePrefabs/FlowerPrefab") as GameObject;
        local.gamePrefabs[ResourceType.Mushroom] = Resources.Load("Prefabs/ResourcePrefabs/MushroomPrefab") as GameObject;
        local.gamePrefabs[ResourceType.GlassBottle] = Resources.Load("Prefabs/PotionBottlePrefab") as GameObject;
        local.gamePrefabs[ResourceType.PwCrystal] = Resources.Load("Prefabs/ResourcePrefabs/CrystalPrefab") as GameObject;
    }

    private static void PopulateInteractorDictionary()
    {
        local.Interactors[Interactables.Cauldron] = Resources.Load("Prefabs/Interactors/Cauldron") as GameObject;
        local.Interactors[Interactables.Deposit] = Resources.Load("Prefabs/Interactors/DepositPad") as GameObject;
        local.Interactors[Interactables.EssenceStation] = Resources.Load("Prefabs/Interactors/EssenceTable") as GameObject;
        local.Interactors[Interactables.HugStation] = Resources.Load("Prefabs/Interactors/HuggingStation") as GameObject;
        local.Interactors[Interactables.Crusher] = Resources.Load("Prefabs/Interactors/GrinderStation") as GameObject;
        local.Interactors[Interactables.TeslaStation] = Resources.Load("Prefabs/Interactors/TeslaCoils") as GameObject;
        local.Interactors[Interactables.Generator] = Resources.Load("Prefabs/Interactors/Generator") as GameObject;
    }

    private static void PopulatePotionPriceDictionary()
    {
        local.PotionPrice[ResourceType.HealthPotion] = local.FourthPotionPrice;
        local.PotionPrice[ResourceType.PoisonPotion] = local.PoisonPotionPrice;
        local.PotionPrice[ResourceType.LovePotion] = local.LovePotionPrice;
        local.PotionPrice[ResourceType.ManaPotion] = local.ManaPotionPrice;
    }

    private static void PopulatePotionsCatalog()
    {


        local.currentUnlockedPotion = new List<ResourceType>() { ResourceType.HealthPotion };

        local.PotionsCatalog = new List<ResourceType>()
        {
            ResourceType.PoisonPotion,ResourceType.LovePotion, ResourceType.ManaPotion //Electrified heart?
        };
    }

    private static void ResetStockLevels()
    {
        ResourceType[] typesArray = (ResourceType[])Enum.GetValues(typeof(ResourceType));

        foreach (ResourceType t in typesArray)
        {
            //TODO: Remove potion bottles from the vending line
            local.soldItems[t] = 0; //Set sold items to 0
        }
    }

    public static void ChangeInputSprite()
    {
        local.btnSprite_Interact = null;
    }

    public int LIVES
    {
        get => lives;
        set => lives = value;
    }


    //==========================
    // === CONTROL FUNCTIONS ===
    //==========================

    public static InteractorScript SpawnInteractorInstant(Interactables type, Vector3 pos, GridPoint[] gridPoints)
    {
        pos.y = 0;
        GameObject newMachine = Instantiate(local.Interactors[type], pos, Quaternion.identity);
        newMachine.transform.SetParent(GameObject.FindGameObjectWithTag("InteractorParent").transform);

        local.existingInteractors.Add(newMachine);

        InteractorScript machineScript = newMachine.GetComponent<InteractorScript>();
        newMachine.transform.Translate(0, machineScript.BoxHeight, 0);
        machineScript.gridPoints = gridPoints;

        return machineScript;
    }

    public static InteractorScript SpawnInteractorDrop(Interactables type, Vector3 pos, GridPoint[] gridPoints)
    {
        InteractorScript machineScript = SpawnInteractorInstant(type, pos, gridPoints);
        machineScript.Drop();

        return machineScript;
    }

    private void MoveCamera()
    {
        Vector3 pos1 = Player1.transform.position, pos2 = Player2.transform.position;
        Vector3 avgPos = (pos1 + pos2) / 2;

        //avgPos += Vector3.forward * -2f;

        //float dist = (Vector3.Distance(pos1, pos2) * 0.75f) + 5;

        //transform.position = avgPos - (transform.forward) * Mathf.Max(minCameraDistance, dist);

        camMoveBack = transform.forward * -cameraDistance;

        Vector3 targetPos = DepositChute.transform.position;
        targetPos.y = 0;
        avgPos.y = 0;

        float dist = Vector3.Distance(targetPos, avgPos);

        targetPos = Vector3.MoveTowards(targetPos, avgPos, Mathf.Min(dist / 5, 1.2f));

        //transform.position = Vector3.MoveTowards(transform.position, targetPos + Vector3.back*3 + camMoveBack, 0.05f);
        transform.position = targetPos + Vector3.back * 2 + camMoveBack;
    }

    public static void ShakeCamera()
    {
        CameraShaker.Instance.ShakeOnce(local.shake_distance, local.shake_roughness, 0.5f, local.shake_fadeout);

        int childCount = local.BooksParent.childCount;
        for (int i = 0; i < childCount; i++)
        {
            Rigidbody book = local.BooksParent.GetChild(i).GetComponent<Rigidbody>();
            if (book != null)
            {
                book.isKinematic = false;
                Vector3 force = new Vector3(UnityEngine.Random.Range(-2, 2), UnityEngine.Random.Range(1, 3), UnityEngine.Random.Range(-2, 2)).normalized;
                book.AddForce(force * 5, ForceMode.Impulse);
            }
        }
    }

    public static void ShakeCameraMini()
    {
        CameraShaker.Instance.ShakeOnce(local.shake_distance / 2, local.shake_roughness / 3, 0.4f, local.shake_fadeout);

        int childCount = local.BooksParent.childCount;
        for (int i = 0; i < childCount; i++)
        {
            Rigidbody book = local.BooksParent.GetChild(i).GetComponent<Rigidbody>();
            if (book != null)
            {
                Vector3 force = new Vector3(UnityEngine.Random.Range(-2, 2), UnityEngine.Random.Range(1, 3), UnityEngine.Random.Range(-2, 2)).normalized;
                book.AddForce(force * 2, ForceMode.Impulse);
            }
        }
    }

    public static void RepairMachine()
    {
        FMOD_ControlScript.PlaySoundOneShot(FMOD_ControlScript.repairEventPath); // sfx repair damage
        local.lives = (int)Mathf.MoveTowards(local.lives, 3, 1);

        //float increase = Mathf.Min(2, local.currentRepairCost * 0.5f);
        local.currentRepairCost += 3;

        //Return books to their position
        int childCount = local.BooksParent.childCount;
        for (int i = 0; i < childCount; i++)
        {
            local.BooksParent.GetChild(i).GetComponent<ObjReturnScript>().ReturnToStart();
        }
    }

    //Update core game variables
    public static void StockUpdate(List<PlayerScript> playersInvolved, ResourceType resource)
    {
        //Increase the score and resources accordingly
        //Debug.Log(resource.ToString() + " added");
        local.vendingControlScript.AddToVendingLine(resource);

        local.UI.PromptStock(resource, false);

        local.ScoreControl.UpdateMade(playersInvolved[0], resource);

        UpdateFMOD_DangerousStock(); //Informs FMOD music
    }

    public static Sprite BtnPromptSprite(PlayerScript p, BtnPromptEnum btnPrompt)
    {
        if (btnPrompt == BtnPromptEnum.None) { return null; }
        if (p.gamePad == true)
        {
            if (PSController[p.PLAYERNUM-1] == 0)
            {
                return local.btnSprite_Interact;
            }
            else
            {
                return local.PSSprite_Interact;
            }
        }
        else
        {
            return (p.PLAYERNUM == 1) ? (local.key1Sprite_Interact) : (local.key2Sprite_Interact);
        }

    }

    public static void UnisonAttempt()
    {
        if (local.unison > 0)
        {
            local.unison = 0;

            Instantiate(GameControllerScript.juicyTextPrefab, local.Player1.transform.position, Quaternion.identity).GetComponent<TextMesh>().text = "UNISON";
            Instantiate(GameControllerScript.juicyTextPrefab, local.Player2.transform.position, Quaternion.identity).GetComponent<TextMesh>().text = "UNISON";
        }
        else
        {
            local.unison = 1f;
        }
    }

    public static void TimeMod(float t)
    {
        local.dayNightScript.timeModSec = t;
    }

    public static bool Paused
    {
        get
        {
            return local.gamePaused
                || (TutorialScript.localTute.show && TutorialScript.localTute.currentTrigger == TutorialTrigger.PressAnyKey);
        }
    }

    public static Transform OtherPlayer(Transform t)
    {
        return (t == local.Player1.transform) ? local.Player2.transform : local.Player1.transform;
    }

    //==========================
    //FMOD Counters
    private static void UpdateFMOD_DangerousStock()
    {
        float i = 0;
        ResourceType[] typesArray = (ResourceType[])Enum.GetValues(typeof(ResourceType));

        //Debug.Log("foreach (ResourceType t in typesArray). Commented out to stop error messages in Game Controller script");
        foreach (ResourceType t in typesArray)
        {
            if (local.vendingControlScript.StockContainPotionCheck(t) && StockLevel(t) <= FMOD_ControlScript.local.StockWarningLevel)
            {
                i++;
            }
        }

        i = Mathf.Min(1, i / FMOD_ControlScript.local.StockNumFullIntensity);

        FMOD_ControlScript.perDangerousStock.setValue(i);
    }

    public static void AddActionsThisMinute(bool player1, int i)
    {
        if (player1)
        {
            FMOD_ControlScript.actionsp1 += i;
        }
        else
        {
            FMOD_ControlScript.actionsp2 += i;
        }
        UpdatePlayerMelodies();
    }

    private static void UpdatePlayerMelodies()
    {
        FMOD_ControlScript.player1Melody.setValue(FMOD_ControlScript.actionsp1 / (float)minutesThisDay);
        FMOD_ControlScript.player2Melody.setValue(FMOD_ControlScript.actionsp2 / (float)minutesThisDay);
    }
    //==========================

    public static DayNightScript DayNight
    {
        get
        {
            return local.dayNightScript;
        }
    }

    public static void DayStarting()
    {
        local.BUYMENU.ResetPrices();

        //Apply new debuffs
        //if (local.dayNightScript.currentDay > 50)
        //{
        //    local.curseSystem.NewDebuff();

        //}
    }

    public static void ClearItems()
    {
        GameObject[] pickups = GameObject.FindGameObjectsWithTag("Pickup");
        foreach (GameObject pickup in pickups)
        {
            if (pickup.layer == 0)
            {
                continue;
            }
            Destroy(pickup);
        }
    }

    public static void DayFinished()
    {
        dailyScore = (int)local.ScoreControl.ResetDaily(local.Player1, local.Player2, local.currentUnlockedPotion.Count, local.STARTLIVES - local.lives);
        //Debug.Log("I HAF WECIEVED DIS: " + dailyScore);

        if (local.DayCap > 0 && DayNight.currentDay >= (local.DayCap - 1))
        {
            PaxEnd(); //Day limit reached
        }
        else
        {
            ShowNightMenu();
            //Undo any debuffs
            //if (local.dayNightScript.currentDay > 50)
            //{
            //    local.curseSystem.Reset();
            //}

            //Eject resources into space
            GameObject[] pickups = GameObject.FindGameObjectsWithTag("Pickup");
            foreach (GameObject pickup in pickups)
            {
                if (pickup.layer == 0)
                {
                    continue;
                }
                pickup.GetComponent<ItemScript>().FlyAway();
            }
        }
    }

    public static int StockLevel(ResourceType t)
    {
        //Mate, this must return -1 so the scoring is calculated correctly
        if (local.currentUnlockedPotion.Contains(t) == false) { return -1; }

        return local.vendingControlScript.StockCount(t);
    }

    /// <summary>
    /// Potions with stock >= 4 && potions with stock >= 5 added together
    /// </summary>
    /// <returns></returns>
    public static int OverstockedPotions()
    {
        int overstocked = 0;
        foreach (ResourceType potion in local.currentUnlockedPotion)
        {
            int tempStock = local.vendingControlScript.StockCount(potion);
            for (int i = 4; i <= local.maxPotions; i++)
            {
                if (tempStock >= i) {
                    overstocked += (potion == ResourceType.HealthPotion) ? 2 : 1;
                }
            }
        }

        return overstocked;
    }

    //*******  
    //**       **   **  *******    **    *******  *********   ******  ******
    //**       **   **  **       ******  **   **  **  **  **  **      **  **
    //**       **   **  *******    **    **   **  **  **  **  *****   ****
    //* *      **   **       **    **    **   **  **  **  **  **      **  **
    //*******  *******  *******    **    *******  **  **  **  ******  **  **

    //TODO: Change this to check potion value for each potion
    public bool CustomerBuy(ResourceType potion, int amount)
    {
        if (vendingControlScript.StockCount(potion) >= amount)
        {
            int price = 1;

            if (ScoreControl.PotionCost.ContainsKey(potion))
            {
                price = ScoreControl.PotionCost[potion];
            }
            else
            {
                //Debug.LogError($"Potion cost for {potion.ToString()} not set in ScoreScript!");
            }

            coinCounter.CoinsToSpawn += amount * price; //Add the coins
            vendingControlScript.DropItem(potion, amount); //Drop the actual potion
            soldItems[potion]++; //Increases items sold level

            UI.PromptStock(potion, true);

            //GameObject temp = overlayParticleLookup[potion];
            //if (temp != null)
            //{
            //    Destroy(Instantiate(temp, overlayCamera.transform.position + new Vector3(0, -7, 10), Quaternion.identity), 10f);
            //}

            ScoreControl.UpdateSold(potion);
            UpdateFMOD_DangerousStock(); //Updates FMOD music
            FMOD_ControlScript.PlaySoundOneShot(FMOD_ControlScript.coinSpawnEventPath, transform.position);
            return true;
        }

        return false;
    }

    /// <summary>
    /// Removes the amount of coins for the machine purchase (if it has enough), otherwise returns false
    /// </summary>
    /// <param name="price">Number of coins to remove</param>
    /// <returns></returns>
    public static bool MakePurchase(int price)
    {
        if (price < 0)
        {
            local.coinCounter.CoinsToSpawn -= price;
            return true;
        }
        else if (local.Coins.Count >= price)
        {
            local.coinCounter.SpendCoins(price);
            return true;
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    /// This function will be called by the customer to reduce the lives of the vending machine
    /// </summary>
    public static void Damage()
    {
        local.lives--;
        ShakeCamera();

        FMOD_ControlScript.PlaySoundOneShot(FMOD_ControlScript.damageEventPath); // Sound when world is damaged

        int failsafe = 0;
        while (failsafe < 10)
        {
            int r = UnityEngine.Random.Range(0, local.breakagePoints.Length);

            if (local.breakagePoints[r].GetComponent<BreakagePointScript>().Damaged((int)local.currentRepairCost))
            {
                break;
            }
            failsafe++;
        }

        TutorialScript.NewTutorial(TutorialType.DamageTaken);

        if (local.lives == 1)
        {
            //Debug.Log("Critical Damage!!!");
        }
        else if (local.lives < 1)
        {
            local.ScoreControl.ResetDaily(local.Player1, local.Player2, local.currentUnlockedPotion.Count, local.STARTLIVES - local.lives);
            Lose();
        }
    }

    public static void PaxEnd()
    {
        local.ScoreControl.WinScoreAdjustment(local.currentUnlockedPotion.Count);

        local.UI.gameObject.SetActive(false);
        local.gamePaused = true;
        local.StopSounds();
        if (local.PaxGameOver != null)
        {
            //local.gamePaused = true;
            local.PaxGameOver.SetActive(true);

            PaxGameEnd D = local.PaxGameOver.GetComponent<PaxGameEnd>();
            D.buttonSelected[0] = 0;
            D.buttonSelected[1] = 0;
        }
        
    }

    //Changed to Public so DayEnd Menu can access this function. Saves us needing to modify 2 sets of ScoreControl.
    public static void Lose()
    {
        local.ScoreControl.FinalScore(local.Coins.Count, local.Player1.playerName, local.Player2.playerName, local.STARTLIVES - local.lives);

        //Reset names
        if (GameControllerScript.PlayerNames != null)
        {
            GameControllerScript.PlayerNames[0] = "AAA";
            GameControllerScript.PlayerNames[1] = "ZZZ";
        }

        local.StopSounds();
        SceneManager.LoadScene(3);
    }



    //******* ****** ******  ****
    //**      **  **   **    **  **
    //**  **  ****     **    **  **
    //**   ** **  **   **    **  **
    //******* **  ** ******  ****
    public class GridPoint
    {
        private bool occupied; //Whether this grid point has a machine placed onto it
        public Vector3 worldPos;
        public PlatformGroup platformGroup;
        public List<GridPoint> adjacents = new List<GridPoint>();
        public Interactables targetInteractor;

        public GridPoint(Vector3 wp, GameControllerScript.PlatformGroup group)
        {
            platformGroup = group;
            worldPos = wp;
        }

        /// <summary>
        /// Spawn an interactor onto the grid point
        /// </summary>
        /// <param name="type"></param>
        public InteractorScript SpawnInteractor(Interactables type)
        {
            Occupied = true;
            return GameControllerScript.SpawnInteractorInstant(type, worldPos, new GridPoint[] { this });
        }

        public PlatformGroup Group()
        {
            if (platformGroup == null)
            {
                //Debug.Log(" -> Creating group");
                platformGroup = new PlatformGroup(GameControllerScript.random.Next(0, 300), worldPos, this);
            }

            return platformGroup;
        }

        public bool Occupied
        {
            get
            {
                return occupied;
            }
            set
            {
                occupied = value;
                //if (cube != null)
                //{
                //    cube.GetComponent<Renderer>().material.color = (occupied) ? Color.red : Color.white;
                //}
            }
        }
    }

    //**********************************
    //Static methods for the grid system
    //**********************************
    public static GridPoint[] FindMeRandomGridPoints(int amount)
    {

        List<GridPoint[]> possibilities = new List<GridPoint[]>();

        foreach (GridPoint point in local.GridSystem)
        {
            GridPoint[] allPoints = new GridPoint[amount];

            if (point != null && point.Occupied == false)
            {
                if (amount == 1 && point.adjacents.Count == 0)
                {
                    allPoints[0] = point;
                    possibilities.Add(allPoints);
                }
                else if (amount == 2)
                {
                    foreach (GridPoint adjacent in point.adjacents)
                    {
                        if (!adjacent.Occupied)
                        {
                            allPoints[1] = adjacent;
                            allPoints[0] = point;
                            possibilities.Add(allPoints);
                            break;
                        }
                    }
                }
            }
        }

        if (possibilities.Count > 0)
        {
            return possibilities[Random.Range(0, possibilities.Count)];
        }

        return null;
    }

    public static GridPoint[] FindMeSpecificGridPoints(int amount, Interactables interactorRequesting)
    {

        List<GridPoint[]> possibilities = new List<GridPoint[]>();

        foreach (GridPoint point in local.GridSystem)
        {
            GridPoint[] allPoints = new GridPoint[amount];

            if (point != null && point.Occupied == false)
            {
                if (amount == 1 && point.adjacents.Count == 0 && interactorRequesting == point.targetInteractor)
                {
                    allPoints[0] = point;
                    possibilities.Add(allPoints);
                }
                else if (amount == 2 && interactorRequesting == point.targetInteractor)
                {
                    foreach (GridPoint adjacent in point.adjacents)
                    {
                        if (!adjacent.Occupied)
                        {
                            allPoints[1] = adjacent;
                            allPoints[0] = point;
                            possibilities.Add(allPoints);
                            break;
                        }
                    }
                }
            }
        }

        if (possibilities.Count > 0)
        {
            return possibilities[Random.Range(0, possibilities.Count)];
        }

        return null;
    }

    /// <summary>
    /// Gets the height of the platform quadrant, depending on the index of the grid point
    /// </summary>
    /// <returns></returns>
    private static float GetGridHeight(GridPoint point)
    {
        return point.platformGroup.currentHeight;
    }

    //private static GridPoint ClosestGridPoint(Vector3 test)
    //{
    //    GridPoint closest = null;
    //    float dist = Mathf.Infinity;

    //    foreach (GridPoint point in local.GridSystem)
    //    {
    //        float temp = Vector3.Distance(point.worldPos, test);

    //        if (temp < dist)
    //        {
    //            dist = temp;
    //            closest = point;
    //        }
    //    }

    //    return closest;
    //}
    //**********************************


    //Initialize the grid - please do this before trying to reference it :)
    private static void INITGRID()
    {
        GameObject gridParent = GameObject.FindGameObjectWithTag("GridParent");

        //Variables for the method so it doesn't have to keep referencing the game controller
        int sizeX = local.cgSize.x, sizeZ = local.cgSize.y;
        Vector2 topLeft = local.gridTopLeft;
        int spacing = local.gridSpacing;

        //List of platform groups
        GameControllerScript.local.platformGroups = new List<PlatformGroup>();
        local.GridSystem = new GridPoint[sizeX, sizeZ];

        if (local.CustomGrid.Length < (sizeX * sizeZ))
        {
            //Debug.LogError("Grid length mismatch. Have you 'Applied Grid'?");
        }
        else
        {
            //Goes through the list set in the inspector, and creates grid point if it was ticked
            for (int z = 0; z < sizeZ; z++)
            {
                for (int x = 0; x < sizeX; x++)
                {
                    int i = x + (z * sizeX);

                    //Position of this grid point
                    Vector3 worldPos = new Vector3(topLeft.x + (x * spacing), 0, topLeft.y - (z * spacing * 0.75f));
                    if (local.gridIndentAt >= 0 && z == local.gridIndentAt) { worldPos.x += spacing / 2; }

                    if (local.CustomGrid[i])
                    {
                        //Debug.Log(String.Format("======New Point ({0},{1})======", x, z));

                        PlatformGroup myGroup = null;

                        //Finds adjacent grid points
                        if (z > 0)
                        {
                            GridPoint point = local.GridSystem[x, z - 1];
                            if (point != null)
                            {
                                myGroup = point.Group();
                            }
                        }

                        //Finds adjacent grid points
                        if (x > 0)
                        {
                            GridPoint point = local.GridSystem[x - 1, z];
                            if (point != null)
                            {

                                PlatformGroup temp = point.Group();
                                if (myGroup != null)
                                {
                                    if (temp.MergeInto(myGroup))
                                    {
                                        local.platformGroups.Remove(temp);
                                    }
                                }
                                else
                                {
                                    myGroup = point.Group();
                                }
                            }
                        }

                        //Creates the grid point
                        GridPoint newPoint = new GridPoint(worldPos, myGroup);
                        //Adds grid point to array
                        local.GridSystem[x, z] = newPoint;

                        //The below stuff handles adding the grid points into groups
                        if (myGroup == null)
                        {
                            myGroup = newPoint.Group(); //Make sure it creates its own group if it hasn't been assigned
                            local.platformGroups.Add(myGroup);
                        }
                        else
                        {
                            myGroup.AddTo(worldPos, newPoint);
                        }

                        //Debug.Log("In summary, my group is: i" + myGroup.i);
                    }

                    //The starting stations as setup in the inspector
                    if (local.StartingInteractors[i] > 0)
                    {
                        switch (local.StartingInteractors[i])
                        {
                            case 1:
                                Renderer r = ((CauldronScript)local.GridSystem[x, z].SpawnInteractor(Interactables.Cauldron)).BodyMeshRenderer;
                                if (GameControllerScript.PlayerColors != null)
                                {
                                    //TODO: Cauldron Colour fix
                                    //r.material.SetColor("_Color", Color.Lerp(r.material.GetColor("_Color"), GameControllerScript.PlayerColors[0], 0.65f)); //= Color.Lerp(r.material.color, GameControllerScript.PlayerColors[0], 0.65f);
                                }
                                break;
                            case 2:
                                Renderer r2 = ((CauldronScript)local.GridSystem[x, z].SpawnInteractor(Interactables.Cauldron)).BodyMeshRenderer;
                                if (GameControllerScript.PlayerColors != null)
                                {
                                    //r2.material.SetColor("_Color", Color.Lerp(r2.material.GetColor("_Color"), GameControllerScript.PlayerColors[0], 0.65f));
                                }
                                break;
                            case 3:
                                GridPoint point = local.GridSystem[x, z];
                                point.SpawnInteractor(Interactables.EssenceStation);
                                point.targetInteractor = Interactables.EssenceStation;
                                break;
                            case 4:
                                point = local.GridSystem[x, z];
                                point.targetInteractor = Interactables.Crusher;
                                break;
                            case 5:
                                point = local.GridSystem[x, z];
                                point.targetInteractor = Interactables.HugStation;
                                break;
                            case 6:
                                point = local.GridSystem[x, z];
                                point.targetInteractor = Interactables.Generator;
                                break;
                            case 7:
                                point = local.GridSystem[x, z];
                                point.targetInteractor = Interactables.TeslaStation;
                                break;
                            default:
                                break;
                        }
                    }
                }
            }

            //Debug.Log("Platform Groups: " + local.platformGroups.Count);

            //local.platformGroups[0].Drop();
        }

        //Here the grid points store their adjacent points
        for (int z = 0; z < sizeZ; z++)
        {
            for (int x = 0; x < sizeX; x++)
            {
                GridPoint thisPoint = local.GridSystem[x, z];

                if (thisPoint == null) { continue; }

                if (x > 0)
                {
                    GridPoint left = local.GridSystem[x - 1, z];
                    if (left != null)
                    {
                        thisPoint.adjacents.Add(left);
                        left.adjacents.Add(thisPoint);
                    }
                }

                if (z > 0)
                {
                    GridPoint up = local.GridSystem[x, z-1];
                    if (up != null)
                    {
                        thisPoint.adjacents.Add(up);
                        up.adjacents.Add(thisPoint);
                    }
                }
            }
        }
    }

    //****************************
    //Platform groups
    public class PlatformGroup
    {
        public int i = -1;
        public List<GridPoint> points; //All the grid points this group is made up of
        public List<Transform> platforms; //The actual gameobjects representing each grid point
        public float heightTarget = 0.1f, currentHeight = 0f;

        public PlatformGroup(int i, Vector3 worldPos, GridPoint newPoint)
        {
            points = new List<GridPoint>();
            platforms = new List<Transform>();
            this.i = i;
            AddTo(worldPos, newPoint);
        }

        public void AddTo(Vector3 worldPos, GridPoint p)
        {
            GameObject plat = Instantiate(GameControllerScript.platformPrefab, worldPos, Quaternion.identity);
            plat.transform.SetParent(GameObject.FindGameObjectWithTag("PlatformParent").transform);
            platforms.Add(plat.transform);

            points.Add(p);
        }

        //Causes the group to drop to the ground height level
        public void Drop()
        {
            heightTarget = 0.1f;
        }

        //Causes the group to rise to sky height level
        public void Rise()
        {
            heightTarget = 15f;
        }

        public bool Update()
        {
            bool changed = false;

            if (currentHeight != heightTarget)
            {
                currentHeight = Mathf.MoveTowards(currentHeight, heightTarget, 0.2f);

                foreach (Transform platform in platforms)
                {
                    Vector3 pos = platform.position;
                    pos.y = currentHeight;
                    platform.position = pos;
                }
                changed = true;
            }

            return changed;
        }

        public bool MergeInto(PlatformGroup newGroup)
        {
            if (newGroup != this) {
                foreach (Transform p in platforms)
                {
                    newGroup.platforms.Add(p);
                }
                foreach (GridPoint point in points)
                {
                    newGroup.points.Add(point);
                    point.platformGroup = newGroup;
                }

                platforms.Clear();
                points.Clear();
                return true;
            }

            return false;
        }
    }
    //****************************
}






//    ******  ******  **  **  **  **********
//    **      **  **  **  **  **  **  **  **
//    *****   **  **  **  **  **  **  **  **
//    **      **  **  **  **  **  **      **
//    ******  **  ******  ******  **      **
[System.Serializable]
public enum Interactables
{
    //IF YOU ADD MORE INTERACTABLES, MAKE SURE YOU ADD THEM TO THE END PLEASE :)
    None, Bed, RepairSpot, Farm,
    Cauldron, Deposit, EssenceStation, HugStation, Generator, Crusher, TeslaStation
};

[System.Serializable]
public class ResourceCompound
{
    public ResourceType resourceType = ResourceType.Flower;
    public ResourceEffect resourceEffect = ResourceEffect.Null;

    public ResourceCompound(ResourceType t, ResourceEffect e)
    {
        resourceType = t;
        resourceEffect = e;
    }

    public static bool IsPotion(ResourceType t)
    {
        return GameControllerScript.local.currentUnlockedPotion.Contains(t);
        //return IsOfClass(t, "potion");
    }

    //public static bool IsOfClass(ResourceType t, string classToCheck)
    //{
    //    ResourceType[] typesArray = GameControllerScript.local.resourceClass[classToCheck];

    //    foreach (ResourceType check in typesArray)
    //    {
    //        if (t == check)
    //        {
    //            return true;
    //        }
    //    }

    //    return false;
    //}
}


[System.Serializable]
public enum ResourceType
{
    Empty, GlassBottle, Flower, Mushroom, Lovefruit, PwCrystal, Trash, Coin,
    SpoiltPotion, HealthPotion, ManaPotion, PoisonPotion, LovePotion, FearPotion, DragonBreathPotion,
    Book, BookOfBlokes, Beachball, RonThePlushie
};

[System.Serializable]
public enum ResourceEffect
{
    Null, Normal, Charged , Crushed ,Purified, Corrupted, Lovable, Unusable
};

// Marco = Array[Marco[]]; But, is he in a 2D array, or a multidimensional Array??

