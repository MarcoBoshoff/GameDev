using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CauldronScript : InteractorScript
{
    private Dictionary<ResourceType, Material> liquidMats = new Dictionary<ResourceType, Material>(); //The materials that affect the cauldron liquid

    private float drainTimer = 0; //How long until the cauldron auto-drains

    //Ingredients you can put into the cauldron
    private ResourceType[] resourcesAccepting = new ResourceType[]
    {
        ResourceType.Flower,
        ResourceType.Mushroom,
        ResourceType.Lovefruit,
        ResourceType.PwCrystal,
        ResourceType.BookOfBlokes,
        ResourceType.Beachball,
        ResourceType.RonThePlushie
    };

    //Renderers
    public Renderer LiquidMeshRenderer, BodyMeshRenderer;
    private int tempCounter = 0;

    [SerializeField]
    private readonly int storeLimit = 1; //How many ingredients can be stored in the cauldron normally

    [SerializeField]
    private GameObject potionBottlePrefab, uiStorePrefab, mySmoke; //Prefabs
    private List<StoredUiScript> storedIcons = new List<StoredUiScript>(); //Icons showing what is in the 

    [SerializeField]
    private List<PlayerScript> tempAssists = new List<PlayerScript>(); //Stores the assists of the ingredient placed in the cauldron

    // public string path to pass on to the event. Call PlayOneShot(audioEventPath, this.gameObject.transform.position) to play the sound effect once.
    [FMODUnity.EventRef]
    public string stirEventPath, placeInWaterEventPath, cleanEventPath;

    private FMOD.Studio.EventInstance stirAudioEvent;


    void Start()
    {
        //These aren't currently used, but may return again
        storeOptionNames = new string[] { "Brew Item", "Collect potion" }; //Different store option names

        //Cauldron liquid material
        liquidMats[ResourceType.Empty] = Resources.Load("Art/3D/Materials/CauldronEmptyMat", typeof(Material)) as Material;
        liquidMats[ResourceType.SpoiltPotion] = Resources.Load("Art/3D/Materials/CauldronSpoiltMat", typeof(Material)) as Material;
        liquidMats[ResourceType.HealthPotion] = Resources.Load("Art/3D/Materials/CauldronHealthMat", typeof(Material)) as Material;
        liquidMats[ResourceType.ManaPotion] = Resources.Load("Art/3D/Materials/CauldronManaMat", typeof(Material)) as Material;
        liquidMats[ResourceType.LovePotion] = Resources.Load("Art/3D/Materials/CauldronLoveMat", typeof(Material)) as Material;
        liquidMats[ResourceType.PoisonPotion] = Resources.Load("Art/3D/Materials/CauldronPoisonMat", typeof(Material)) as Material;

        Init(Interactables.Cauldron, "Prefabs/GridSystem/SnapCauldron", 1);
        
        stirAudioEvent = FMODUnity.RuntimeManager.CreateInstance(stirEventPath);
    }

    protected override void StoppedQTE(int pref_num) {
        //Fail(pref_num);
    }

    //Limiting when the stir interaction can be done
    protected override int Interact1Option(PlayerStats stats)
    {
        int i = -1;

        if (container.GetPotion() == ResourceType.SpoiltPotion)
        {
            i = 1; // Draining the cauldron
        }
        else if (tempCounter > 0 && container.GetPotion() == ResourceType.Empty)
        {
            i = 0; //Stir QTE
        }

        return i;
    }

    protected override void OnQTEUpdate(QTE_Info info)
    {
        //Moves the stored icons down
        if (storedIcons.Count > 0)
        {
            Vector3 pos = storedIcons[0].transform.position;

            foreach (StoredUiScript icon in storedIcons)
            {
                if (icon != storedIcons[0])
                {
                    icon.transform.position = Vector3.MoveTowards(icon.transform.position, pos, 0.05f);
                }
            }

            info.GetRectTransform().position = QTEPrefsScript.QTEUiSpace(pos);

            if (storedIcons.Count > 0) {
                info.IMG.sprite = storedIcons[storedIcons.Count - 1].GetSprite();
            }
        }
    }

    protected override void SetCurrentQTE(QTEScript v, PlayerScript[] players)
    {
        currentQTE = v;

        // starts or stops stirring sound
        if (v == null)
        {
            // stops stirring sound
            stirAudioEvent.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
        }
        else
        {
            if (!FMOD_ControlScript.PlaybackState(stirAudioEvent))
            {
                Debug.Log("should be playing stir sound");
                stirAudioEvent.start();
            }
        }

    }

    //If the quick time event was successful
    protected override void Success(int pref_num)
    {
        if (pref_num == 0)
        {
            ResourceType potioned = ResourceType.SpoiltPotion; //Which potion has been made

            //Search the combinations
            switch (container.CountNonNulls())
            {

                //No ingredients?
                case 0:
                    potioned = ResourceType.Empty;
                    SetLiquidMesh(ResourceType.Empty);
                    break;

                //Only 1 ingredient
                case 1:
                    //Health Potion - Purified Flower
                    if (container[ResourceType.Flower] == ResourceEffect.Purified)
                    {
                        Debug.Log("Created Health Potion");
                        potioned = ResourceType.HealthPotion;
                        TutorialScript.Trigger(TutorialTrigger.CauldronHealthMade);
                    }
                    //Love Potion - Lovable Flower
                    if (container[ResourceType.Flower] == ResourceEffect.Lovable)
                    {
                        Debug.Log("Created Love Potion");
                        potioned = ResourceType.LovePotion;
                        TutorialScript.Trigger(TutorialTrigger.CauldronLoveMade);
                    }
                    //Love Potion - Single Lovefruit
                    if (container[ResourceType.Lovefruit] == ResourceEffect.Lovable)
                    {
                        Debug.Log("Created Love Potion");
                        potioned = ResourceType.LovePotion;
                        TutorialScript.Trigger(TutorialTrigger.CauldronLoveMade);
                    }
                    //Poison1 Potion - Corrupted Mushroom
                    else if (container[ResourceType.Mushroom] == ResourceEffect.Crushed)
                    {
                        Debug.Log("Created Poison Potion");
                        potioned = ResourceType.PoisonPotion;
                        TutorialScript.Trigger(TutorialTrigger.CauldronPoisonMade);
                    }
                    else if (container[ResourceType.PwCrystal] == ResourceEffect.Charged)
                    {
                        Debug.Log("Mana Potion");
                        potioned = ResourceType.ManaPotion;
                        TutorialScript.Trigger(TutorialTrigger.CauldronManaMade);
                    }
                    break;

                //Only 2 ingredients
                case 2:

                    if (container[ResourceType.Flower] == ResourceEffect.Purified && container[ResourceType.Lovefruit] == ResourceEffect.Lovable)
                    {
                        Debug.Log("Created Love Potion");
                        potioned = ResourceType.LovePotion;
                    }
                    break;

                //Only 3 ingredients
                case 3:

                    if (container[ResourceType.BookOfBlokes] == ResourceEffect.Purified && container[ResourceType.Beachball] == ResourceEffect.Purified
                        && container[ResourceType.RonThePlushie] == ResourceEffect.Purified)
                    {
                        Debug.Log("Created TOPBLOKERON");
                        GameObject ron = GameObject.FindGameObjectWithTag("Ron");
                        if (ron != null) { ron.GetComponent<RonScript>().ActivateRon(); }
                    }
                    break;
            }

            //Spoil if that potion is not unlocked yet
            if (!potioned.IsAPotion()) { potioned = ResourceType.SpoiltPotion; }

            //If no matching combinations were found
            if (potioned == ResourceType.SpoiltPotion)
            {
                ResetContainerDictionary();
                Contain(ResourceType.SpoiltPotion);
                SetLiquidMesh(ResourceType.SpoiltPotion);

                //Create spoilt particle
                CreateParticle(spoiltParticle, 7f);

                // fail sfx
                FMOD_ControlScript.PlayFailSound(transform.position);
                
                //Clear stored icons
                storedIcons.Clear();

                if (mySmoke == null)
                {
                    mySmoke = Instantiate(spoiltSmokeParticle, transform.position, Quaternion.identity);
                    mySmoke.transform.parent = this.transform;
                }

                foreach (PlayerScript player in LastInteractedPlayers)
                {
                    player.GetComponent<GiveWand>().StirCauldron(false);
                }

                //Trigger Tutorial
                //TutorialScript.Trigger(TutorialTrigger.CauldronFail);
            }

            //If a valid potion was created
            else if (potioned != ResourceType.Empty)
            {
                //Create success particle
                CreateParticle(successParticle, 3f);
                SetLiquidMesh(ResourceType.Empty);

                ResetContainerDictionary();
                PlayerScript lastPlayer = LastInteractedPlayers[0];
                GlassBottleScript bottle = Instantiate(potionBottlePrefab, lastPlayer.transform.position + lastPlayer.transform.forward, Quaternion.identity).GetComponent<GlassBottleScript>();

                bottle.StoredPotion = potioned;
                LastInteractedPlayers[0].GrabItem(bottle);

                foreach (PlayerScript p in tempAssists)
                {
                    bottle.AddPlayerAssist(p);
                }

                foreach (PlayerScript p in LastInteractedPlayers)
                {
                    p.ActionCounter(2);
                    bottle.AddPlayerAssist(p);
                }

                GameControllerScript.UnisonAttempt(); //Attemps to show unison juicy text

                //Clear stored icons
                storedIcons.Clear();

                //Trigger Tutorial
                //TutorialScript.Trigger(TutorialTrigger.CauldronSuccess);

                foreach (PlayerScript player in LastInteractedPlayers)
                {
                    player.GetComponent<GiveWand>().StirCauldron(false);
                }

                // success sfx
                FMOD_ControlScript.PlaySuccessSound(transform.position);
            }
            
        }

        //Else, is draining the spoilt potion
        else if(pref_num == 1)
        {
            container[ResourceType.SpoiltPotion] = ResourceEffect.Null;
            SetLiquidMesh(ResourceType.Empty);
            if (mySmoke != null)
            {
                Destroy(mySmoke);
            }

            // play cleaning cauldron sound
            FMOD_ControlScript.PlaySoundOneShot(cleanEventPath, transform.position);
        }

        tempAssists.Clear();
        UpdateTempCounter();
    }

    //if the quick time event failed
    protected override void Fail(int pref_num)
    {
        Debug.Log("Failed with: " + pref_num);

        if (pref_num == 0)
        {
            ResetContainerDictionary();
            Contain(ResourceType.SpoiltPotion);
            SetLiquidMesh(ResourceType.SpoiltPotion);

            //Create spoilt particle
            CreateParticle(spoiltParticle, 7f);

            //Remove the store icons
            storedIcons.Clear();

            //Create the smoke effect
            if (mySmoke == null)
            {
                mySmoke = Instantiate(spoiltSmokeParticle, transform.position, Quaternion.identity);
                mySmoke.transform.parent = this.transform;
            }

            drainTimer = 15f;
        }

        foreach (PlayerScript player in LastInteractedPlayers)
        {
            player.GetComponent<GiveWand>().StirCauldron(false);
        }

        tempAssists.Clear();
        UpdateTempCounter();
    }

    //Sets which type of storing this is employing (if putting in an item (0), if using a potion bottle (1))
    //This is used by Store() below to determine what to do with the item
    protected override int StoreOption(PlayerScript p, PlayerStats stats)
    {
        //print(p);
        //print(stats.holdingItem);
        if (stats.holdingItem != null)
        {
            ResourceType itemType = stats.holdingItem.resourceCompound.resourceType;

            if (itemType == ResourceType.GlassBottle)
            {
                if (container.GetPotion() != ResourceType.Empty)
                {
                    return 1; //Is bottling a potion in a bottle
                }
            }
            else if (container[itemType] == ResourceEffect.Null && container.CountNonNulls() < storeLimit)
            {
                bool accepted = false; //Will this object be accepted

                //Debug.Log("NonNulls = " + container.CountNonNulls());

                //Go through the list of accepted resources and check if player resource is accepted
                for (int i = 0; i < resourcesAccepting.Length; i++)
                {
                    //Check if Players item is accepted by this object
                    if (resourcesAccepting[i].Equals(itemType))
                    {
                        //The player held object can be accepted
                        accepted = true;
                        break;
                    }
                }

                //Can only store when there is no potion already in the cauldron
                if (accepted && container.GetPotion() == ResourceType.Empty)
                {
                    return 0;
                }
            }
            else if (itemType == ResourceType.BookOfBlokes || itemType == ResourceType.Beachball || itemType == ResourceType.RonThePlushie)
            {
                return 0;
            }
        }

        return -1; //No applicable options
    }

    public bool IsEmpty()
    {
        return container.CountNonNulls() == 0; //If the cauldron doesn't have any finished potions in it
    }

    //Updates the time for automatic draining
    protected override void OverridenUpdate()
    {
        if (drainTimer > 0)
        {
            drainTimer = Mathf.MoveTowards(drainTimer, 0, Time.fixedDeltaTime);

            if (drainTimer == 0)
            {
                container[ResourceType.SpoiltPotion] = ResourceEffect.Null;
                SetLiquidMesh(ResourceType.Empty);
                if (mySmoke != null)
                {
                    Destroy(mySmoke);
                }
                UpdateTempCounter();
            }
        }
    }

    //Store Objects inside this object. Override from InteractorScript
    protected override bool Store(ItemScript itemToStore, PlayerScript player, int storeOption)
    {
        if (storeOption == 0) {

            ResourceCompound _compound = itemToStore.resourceCompound;

            if (_compound.resourceEffect != ResourceEffect.Normal)
            {
                ResourceType typeToStore = itemToStore.resourceCompound.resourceType;

                //Sets the liquid material
                container[typeToStore] = itemToStore.resourceCompound.resourceEffect;
                UpdateTempCounter();
                LiquidMeshRenderer.material = Resources.Load("Art/3D/Materials/CauldronUnMixed", typeof(Material)) as Material;
                Debug.Log(typeToStore.ToString() + " added with an effect of: " + itemToStore.resourceCompound.resourceEffect.ToString());

                //The stored icon position
                float height = (2.5f) + storedIcons.Count;
                StoredUiScript icon = Instantiate(uiStorePrefab, transform.position + Vector3.up * height, Quaternion.identity).GetComponent<StoredUiScript>();
                storedIcons.Add(icon);
                icon.cauldron = this;
                icon.SetSprite(typeToStore);
                icon.transform.SetParent(transform);
                icon.transform.forward = -mainCam.transform.forward;

                // drop item in water script
                FMOD_ControlScript.PlaySoundOneShot(placeInWaterEventPath, transform.position);

                //Start stirring as soon as ingredient is placed in station
                ImmediateInterface(player);
                player.ActionCounter(1);

                foreach (PlayerScript p in itemToStore.PlayerAssists)
                {
                    tempAssists.Add(p);
                }
            }
            else
            {
                LastInteractedPlayers = new PlayerScript[] { player };
                Fail(0);
            }

            return true;
        }
        else if (storeOption == 1)
        {
            player.ActionCounter(2);
            //Store the potion in the glass bottle
            ResourceType potion = container.GetPotion();
            ((GlassBottleScript)itemToStore).StoredPotion = potion;

            GameObject tempPart = Instantiate(successParticle, itemToStore.transform.position, Quaternion.identity); //Visual info to the player
            tempPart.transform.parent = itemToStore.transform;
            Destroy(tempPart, 3f);
            SetLiquidMesh(ResourceType.Empty);
            container[potion] = ResourceEffect.Null; //Remove the potion
        }

        UpdateTempCounter();
        return false; //Wasn't successfully stored
    }

    private void SetLiquidMesh(ResourceType r)
    {
        LiquidMeshRenderer.material = liquidMats[r];
    }

    private void UpdateTempCounter()
    {
        tempCounter = container.CountNonNulls();

        Debug.Log("tempCounter = " + tempCounter);
    }
}
