using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DepositChuteScript : InteractorScript
{
    public CollectionScript myCollector;

    public ParticleSystem myParticles;

    public bool takesClassOfItem = true;

    [FMODUnity.EventRef]
    public string portalDropEventPath;

    //public string classAccepting = "potion";

    void Start()
    {
        if (myParticles != null)
        {
            myParticles.Stop();
        }

        storeOptionNames = new string[]{"Send to Chute"};
        Init(Interactables.Deposit, "Prefabs/GridSystem/SnapCauldron", 0); //Make sure init is called for interactor setup
    }

    private float comboCounter = 0;
    private int combo = 0;
    void FixedUpdate()
    {
        if (comboCounter > 0)
        {
            comboCounter = Mathf.MoveTowards(comboCounter, 0, Time.fixedDeltaTime);

            if (comboCounter == 0) { combo = 0; }
        }
    }

    //Sets which type of storing this is employing (if a potion (0), if not a potion (-1))
    //This is used by Store() below to determine what to do with the item
    protected override int StoreOption(PlayerScript p, PlayerStats stats)
    {
        if (takesClassOfItem)
        {
            if (ResourceCompound.IsPotion(stats.holdingItem.resourceCompound.resourceType))
            {
                return 0; //If a potion, return 0
            }
        }

        return -1; //No storing options
    }        

 

    //Check the resource type and send it to the Game Controller.
    //Store Objects inside this object. Override from InteractorScript
    protected override bool Store(ItemScript itemToChute, PlayerScript Player, int storeOption)
    {
        //Defined storeOption 0 as depositing a potion
        if (storeOption == 0)
        {
            

            CollectItem(itemToChute);
            Player.ActionCounter(2);

            itemToChute.AddPlayerAssist(Player);
        }

        return false; //Wasn't successfully chuted
    }

    //Adds the item to the collector's list
    public void CollectItem(ItemScript item)
    {
        if (myParticles != null && !myParticles.isPlaying)
        {
            myParticles.Play();
        }

        myCollector.itemsToCollect.Add(item);
        item.busy = true;
        //item.GetComponent<Rigidbody>().useGravity = false;
        Destroy(item.GetComponent<Collider>());


        ResourceType potion = item.resourceCompound.resourceType;

        string scoreText = "combo";
        if (GameControllerScript.StockLevel(potion) == 0 && GameControllerScript.customerControl.IsWaitingOnPotion(potion))
        {
            scoreText = "GOOD SAVE!";
        }
        else
        {
            combo++;
            comboCounter += 2f;

            if (combo == 1)
            {
                scoreText = "+1 POTION";
            }
            else
            {
                scoreText = $"+{combo} BREWTASTIC";
            }
        }

        Instantiate(GameControllerScript.juicyTextPrefab, transform.position, Quaternion.identity).GetComponent<TextMesh>().text = scoreText;
        GameControllerScript.UnisonAttempt(); //Attemps to show unison juicy text

        FMOD_ControlScript.PlaySoundOneShot(portalDropEventPath, transform.position); // plays portal drop sfx

        //Trigger Tutorial
        switch (item.resourceCompound.resourceType)
        {
            case ResourceType.HealthPotion:
                TutorialScript.Trigger(TutorialTrigger.HealthPotionSent);
                break;
            case ResourceType.PoisonPotion:
                TutorialScript.Trigger(TutorialTrigger.PoisonPotionSent);
                break;
            case ResourceType.LovePotion:
                TutorialScript.Trigger(TutorialTrigger.LovePotionSent);
                break;
            case ResourceType.ManaPotion:
                TutorialScript.Trigger(TutorialTrigger.ManaPotionSent);
                break;
        }
    }
}
