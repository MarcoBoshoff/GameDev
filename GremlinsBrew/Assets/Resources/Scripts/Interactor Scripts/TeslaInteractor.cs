using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeslaInteractor : InteractorScript
{
    public static bool StoredBolt = false; //Whether a bolt has been received from the generator
    public Animator animator;

    //private ResourceType[] resourcesAccepting = new ResourceType[] { ResourceType.PwCrystal, ResourceType.Lovefruit};

    private int _storeLimit = 1;
    private int tempCounter = 0;

    private float hover = 1, hoverTarget = 0.7f;

    private GameObject modelShowing;

    [FMODUnity.EventRef]
    public string zapEventPath;

    void Start()
    {
        storeOptionNames = new string[] { "Place Crystal", "Place Heart" };

        cooldownAmount = 1f; //You can set a custom number for how long after a QTE it takes before this interactor can be used again

        Init(Interactables.TeslaStation, "Prefabs/GridSystem/SnapTelsaMachine", 2);
        animator.SetBool("SwitchOn", false);
    }

    protected override void OnQTEUpdate(QTE_Info info)
    {
        if (currentQTE != null && TeslaInteractor.StoredBolt)
        {
            currentQTE.Interrupt(0); //Id = 0 (Shoot a lightning bolt)
            TeslaInteractor.StoredBolt = false;

            // shoot lighting sfx
            FMOD_ControlScript.PlaySoundOneShot(zapEventPath, transform.position);
            animator.SetBool("SwitchOn", true);
        }

        //info.GetRectTransform().position = transform.position + Vector3.up * 2.5f;
        //Instantiate(electricalBurstParticle, transform.position, Quaternion.identity);
    }

    //## Set a limit when the first interaction can't be called (set to false to have no limit)
    protected override int Interact1Option(PlayerStats stats)
    {
        if (tempCounter > 0)
        {
            if (container.GetNormal() == ResourceType.PwCrystal && container.GetNotNormal() == ResourceType.Empty)
            {
                //if (GeneratorScript.InProgress)
                //{
                    return 0;
                //}
            }
        }

        return -1;
    }


    //## This method is called when the quick time event is successfully completed
    //If the quick time event was successful
    protected override void Success(int pref_num)
    {
        if (tempCounter > 0)
        {
            ResourceType t = container.GetNormal();

            if (t != ResourceType.Empty)
            {
                print(t);
                //Create item with effect
                ItemScript spawned = Instantiate(GameControllerScript.local.gamePrefabs[t], LastInteractedPlayers[0].transform.position, Quaternion.identity).GetComponent<ItemScript>();
                spawned.resourceCompound.resourceEffect = ResourceEffect.Charged;
                spawned.NewForm(1); //Electrified model
                LastInteractedPlayers[0].GrabItem(spawned);

                //Create essence particle
                if (pref_num == 0)
                {
                    CreateParticle(purifiedParticle, 5f);
                }

                //Empty containers
                container[t] = ResourceEffect.Null;

                int de = container.CountNonNulls();
                //Debug.Log("Non nulls = " + de);
            }

            if (modelShowing != null)
            {
                Destroy(modelShowing);
            }
        }

        GameControllerScript.UnisonAttempt(); //Attemps to show unison juicy text
        FMOD_ControlScript.PlaySuccessSound(transform.position); // zap success sfx

        GeneratorScript.Done = true;
        UpdateTempCounter();
        animator.SetBool("SwitchOn", false);
    }

    //## Again, you can call this if the QTE fails
    protected override void Fail(int pref_num)
    {
        //## Do something... or nothing, up to you :)
    }

    protected override void OverridenUpdate()
    {
        if (modelShowing != null)
        {
            hover = Mathf.MoveTowards(hover, hoverTarget, 0.009f);
            if (hover == hoverTarget) { hoverTarget = (hoverTarget == 1.4f) ? 0.7f : 1.4f; }

            modelShowing.transform.position = (transform.position + Vector3.up * hover);

            Vector3 rot = modelShowing.transform.eulerAngles;
            rot.y += 0.3f;
            modelShowing.transform.eulerAngles = rot;
        }
    }

    //## This is important for storing. If you don't wish to store anything, return -1;
    //## Return a 0 or larger if the player can store there
    //## That number (0+) is used to get the prompt from above (storeOptionNames) in Start()
    //## It is also used in the Store function below. It simply sees which option number is being
    //      returned, and performs the related action
    //Sets which type of storing this is employing (if placing in an item (0), if no room (-1))
    //This is used by Store() below to determine what to do with the item
    protected override int StoreOption(PlayerScript p, PlayerStats stats)
    {
        if (stats.holdingItem != null)
        {
            ItemScript item = stats.holdingItem;

            //Deposit resource value into gameController, check if resource doesn't already exist in storage
            if (item != null && container[item.resourceCompound.resourceType] == ResourceEffect.Null)
            {
                if (container.CountNonNulls() < _storeLimit)
                {
                    if (item.resourceCompound.resourceType == ResourceType.PwCrystal && item.resourceCompound.resourceEffect == ResourceEffect.Normal)
                    {
                        return 0; //Is a valid item to place on the tesla tower
                    }
                    else if (item.resourceCompound.resourceType == ResourceType.Lovefruit && item.resourceCompound.resourceEffect == ResourceEffect.Lovable)
                    {
                        return 1; //Is a valid item to place on the tesla tower
                    }
                }
            }
        }

        return -1;
    }

    protected override bool Store(ItemScript itemToStore, PlayerScript player, int storeOption)
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

            //Tutorial trigger
            TutorialScript.Trigger(TutorialTrigger.CrystalOnTesla);

            ImmediateInterface(player);
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
