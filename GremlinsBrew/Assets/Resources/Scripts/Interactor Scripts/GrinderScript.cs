using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GrinderScript : InteractorScript
{
    private ResourceType[] resourcesAccepting = new ResourceType[] { ResourceType.Mushroom };
    [SerializeField]
    private int storeLimit = 1;

    public bool fireExtinguisher = true;

    private int tempCounter = 0;

    private GameObject modelShowing;

    [FMODUnity.EventRef]
    public string placeEventPath;

    private float cookedCooldown = 0;
    public ParticleSystem fire, smoke;
    public GameObject playerFlameParticle;

    //The stopwatch animation that plays
    public Animator animator;
    public GameObject StopWatch;

    void Start()
    {
        storeOptionNames = new string[] { "Place item" };

        Init(Interactables.Crusher, "Prefabs/GridSystem/SnapEssenceStation", 1); //Make sure init is called for interactor setup

        fire.Stop();
        smoke.Stop();
        animator.SetBool("SwitchOn", false);
    }

    //## Set a limit when the first interaction can't be called (set to false to have no limit)
    protected override int Interact1Option(PlayerStats stats)
    {
        if (tempCounter > 0)
        {
            if (container[ResourceType.Mushroom] == ResourceEffect.Normal)
            {
                return 0;
            }
            if (container[ResourceType.Mushroom] == ResourceEffect.Corrupted && cookedCooldown <= 0)
            {
                return 1;
            }
        }

        return -1;
    }

    protected override void OverridenUpdate()
    {
        if (cookedCooldown > 0)
        {
            cookedCooldown -= Time.fixedDeltaTime * 0.5f;
            StopWatch.GetComponentInChildren<Text>().text = Mathf.RoundToInt(cookedCooldown).ToString();
            if (cookedCooldown <= 0)
            {
                smoke.Stop();
                StopWatch.SetActive(false);
                animator.SetBool("SwitchOn", true);
            }
        }
    }

    protected override void OnQTEUpdate(QTE_Info info)
    {
        if (info.PrefNum == 0 && fire.isStopped)
        {
            fire.Play();
            animator.SetBool("SwitchOn", false);
        }
    }

    //## This method is called when the quick time event is successfully completed
    //If the quick time event was successful
    protected override void Success(int pref_num)
    {
        if (tempCounter > 0)
        {
            ResourceType t = container.GetAll();
            if (t != ResourceType.Empty)
            {
                Debug.Log("Crushed Mushroom " + pref_num);
                
                if (pref_num == 0)
                {
                    fire.Stop();
                    smoke.Play();
                    animator.SetBool("SwitchOn", false);
                    cookedCooldown = 5f;
                    StopWatch.SetActive(true);

                    PlayerScript player = LastInteractedPlayers[0];

                    if (fireExtinguisher == false)
                    {
                        GameObject playerFlame = Instantiate(playerFlameParticle, player.transform.position, Quaternion.identity);
                        playerFlame.transform.parent = player.transform;
                        playerFlame.transform.eulerAngles = new Vector3(270, 0, 0);
                        Destroy(playerFlame, 10f);
                    }

                    //Essence Station Particle
                    //CreateParticle(corruptedParticle, 9f);
                    TutorialScript.Trigger(TutorialTrigger.CookSuccess);

                    //fail sound
                    FMOD_ControlScript.PlaySuccessSound(transform.position);

                    container[t] = ResourceEffect.Corrupted;

                    modelShowing.GetComponent<ItemScript>().NewForm(1);

                }
                else if (pref_num == 1)
                {
                    ItemScript spawned = Instantiate(GameControllerScript.local.gamePrefabs[t], LastInteractedPlayers[0].transform.position, Quaternion.identity).GetComponent<ItemScript>();
                    spawned.resourceCompound.resourceEffect = ResourceEffect.Crushed;
                    spawned.NewForm(2); // 1 = corrupt, 2 = crushed

                    LastInteractedPlayers[0].GrabItem(spawned); //Grab straight away

                    //Essence Station Particle
                    CreateParticle(corruptedParticle, 9f);
                    //Item particle
                    Instantiate(isCorruptedParticle, spawned.transform.position, Quaternion.identity).transform.SetParent(spawned.transform);
                    animator.SetBool("SwitchOn", false);

                    //Empty containers
                    container[t] = ResourceEffect.Null;

                    if (modelShowing != null)
                    {
                        Destroy(modelShowing);
                    }

                    TutorialScript.Trigger(TutorialTrigger.CrushSuccess);

                    GameControllerScript.UnisonAttempt(); //Attemps to show unison juicy text
                    FMOD_ControlScript.PlaySuccessSound(transform.position);//fail sound

                }

                //Trigger Tutorial
                switch (t)
                {
                    case ResourceType.Mushroom:
                        TutorialScript.Trigger(TutorialTrigger.EssenceSuccessMushroom);
                        break;
                }

            }

        }

        UpdateTempCounter();
    }

    protected override int StoreOption(PlayerScript p, PlayerStats stats)
    {
        if (stats.holdingItem != null)
        {
            ItemScript item = stats.holdingItem;

            //Deposit resource value into gameController, check if resource doesn't already exist in storage
            if (item != null && (container[item.resourceCompound.resourceType] == ResourceEffect.Null))
            {

                if (container.CountNonNulls() < storeLimit)
                {

                    if (resourcesAccepting.Contains(item) && item.resourceCompound.resourceEffect == ResourceEffect.Normal || resourcesAccepting.Contains(item) && item.resourceCompound.resourceEffect == ResourceEffect.Corrupted)
                    {
                        return 0; //Is a valid item to place on the grinder station
                    }
                }
            }
        }
        return -1;
    }


    //## Store items inside this object. The 'int storeOption' is calculated above. Don't calculate any options here!
    //## Just check which storeOption is being passed in as a parameter
    protected override bool Store(ItemScript itemToStore, PlayerScript Player, int storeOption)
    {
        //Store option (defined above)
        if (storeOption == 0)
        {
            ResourceType resourceToStore = itemToStore.resourceCompound.resourceType;

            container[resourceToStore] = itemToStore.resourceCompound.resourceEffect;
            UpdateTempCounter();

            //Display the resource model
            modelShowing = Instantiate(itemToStore.gameObject, transform.position + Vector3.up, Quaternion.identity);
            modelShowing.layer = 0;
            modelShowing.name = "Resource display model";
            modelShowing.transform.SetParent(this.transform);
            modelShowing.transform.localScale = new Vector3(1, 1, 1);

            Debug.Log(itemToStore.resourceCompound.resourceType.ToString() + " added");

            // place item sfx
            FMOD_ControlScript.PlaySoundOneShot(placeEventPath, transform.position);
            animator.SetBool("SwitchOn", false);
            // Grind sfx here?

            //Stewart/Nathan - if you need, this method triggres the QTE immediately :)
            ImmediateInterface(Player);

            return true; //Successfully placed
        }
        return false; //Wasn't successfully placed
    }

    private void UpdateTempCounter()
    {
        tempCounter = container.CountNonNulls();
    }
    // ## And of course, you can have any method heres that are specific to your interactor. Probably best to make them private
    //     unless other classes need to call them.
}
