using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EssenceScript : InteractorScript
{
    //What resources you can put on the essence table
    private ResourceType[] resourcesAccepting = new ResourceType[] { ResourceType.Flower};
    [SerializeField]
    private int _storeLimit = 1;

    private int tempCounter = 0;

    private GameObject modelShowing; //The ingredient being shown

    [FMODUnity.EventRef]
    public string placeEventPath;

    void Start()
    {
        storeOptionNames = new string[] { "Place item" };

        Init(Interactables.EssenceStation, "Prefabs/GridSystem/SnapEssenceStation", 1); //Make sure init is called for interactor setup
    }

    //When the interactions 1 can occur
    protected override int Interact1Option(PlayerStats stats)
    {
        if (currentQTE != null) { return -1; }

        if (tempCounter > 0 && container.GetNotNormal() == ResourceType.Empty)
        {
            if (container.GetNormal() == ResourceType.Flower)
            {
                return 0; //Can begin the draw QTE
            }
        }

        return -1;
    }

    //If the quick time event was successful
    protected override void Success(int pref_num)
    {
        if (tempCounter > 0)
        {
            ResourceType t = container.GetNormal();
            
            if (t != ResourceType.Empty) {

                //Create item with effect
                ItemScript spawned = Instantiate(modelShowing, LastInteractedPlayers[0].transform.position, Quaternion.identity).GetComponent<ItemScript>();
                spawned.gameObject.layer = 11;
                spawned.AddPlayerAssist(LastInteractedPlayers[0]);

                GameControllerScript.UnisonAttempt(); //Attemps to show unison juicy text

                //Create essence particle
                if (pref_num == 0)
                {
                    spawned.resourceCompound.resourceEffect = ResourceEffect.Purified; //Purifies the ingredient
                    spawned.NewForm(1);
                    LastInteractedPlayers[0].GrabItem(spawned); //Grab straight away

                    //Essence Station Particle
                    CreateParticle(purifiedParticle, 5f);
                    //Item particle
                    Instantiate(isPureParticle, spawned.transform.position, Quaternion.identity).transform.SetParent(spawned.transform);

                    
                    //play win sound
                    FMOD_ControlScript.PlaySuccessSound(transform.position);
                }

                else if (pref_num == 1)
                {
                    spawned.resourceCompound.resourceEffect = ResourceEffect.Corrupted;
                    spawned.NewForm(1);
                    LastInteractedPlayers[0].GrabItem(spawned); //Grab straight away

                    //Essence Station Particle
                    CreateParticle(corruptedParticle, 9f);
                    //Item particle
                    Instantiate(isCorruptedParticle, spawned.transform.position, Quaternion.identity).transform.SetParent(spawned.transform);

                    //fail sound
                    FMOD_ControlScript.PlayFailSound(transform.position);
                }

                //Trigger Tutorial
                switch (t)
                {
                    case ResourceType.Flower:
                        TutorialScript.Trigger(TutorialTrigger.EssenceSuccessFlower);
                        break;
                    case ResourceType.Mushroom:
                        TutorialScript.Trigger(TutorialTrigger.EssenceSuccessMushroom);
                        break;
                }

                //Empty containers
                container[t] = ResourceEffect.Null;

                foreach (PlayerScript player in LastInteractedPlayers)
                {
                    player.GetComponent<GiveWand>().CastingSpell(false);
                }
            }

            if (modelShowing != null)
            {
                Destroy(modelShowing);
            }
        }

        UpdateTempCounter();
    }

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
                    if (resourcesAccepting.Contains(item) && item.resourceCompound.resourceEffect == ResourceEffect.Normal)
                    {
                        return 0; //Is a valid item to place on the essence station
                    }
                }
            }
        }

        return -1;
    }

    //Check the resource type and send it to the Game Controller.
    //Store Objects inside this object. Override from InteractorScript
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
            //modelShowing.name = "Resource display model";
            modelShowing.transform.SetParent(this.transform);
            modelShowing.transform.localScale = new Vector3(1, 1, 1);

            Debug.Log(itemToStore.resourceCompound.resourceType.ToString() + " added");

            // place item sfx
            FMOD_ControlScript.PlaySoundOneShot(placeEventPath, transform.position);

            //Stewart/Nathan - if you need, this method triggres the QTE immediately :)
            ImmediateInterface(player); 


            return true; //Successfully placed

            
        }

        return false; //Wasn't successfully placed
    }

    private void UpdateTempCounter()
    {
        tempCounter = container.CountNonNulls();
    }
}


//Jake: "...there must be more (FMOD files) we can ignore for next semester haha"
// FMOD Studio Cache to Jake: "Am I a joke to you?"