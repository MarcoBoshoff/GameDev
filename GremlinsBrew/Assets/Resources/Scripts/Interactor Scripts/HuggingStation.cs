using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HuggingStation : InteractorScript
{
    public ResourceCompound[] resourcesAccepting;
    public GameObject loveFruitPrefab;

    private PlayerScript huggingStarter = null; //Which player started the hug

    [FMODUnity.EventRef]
    public string hugSoundEventPath = null;

    void Start()
    {
        storeOptionNames = new string[] { "Imbue the flower" };
        cooldownAmount = 2f;
        Init(Interactables.HugStation, "Prefabs/GridSystem/SnapHuggingStation", 2); //Make sure init is called for interactor setup
    }

    protected override int Interact1Option(PlayerStats stats)
    {
        if(GameControllerScript.DayNight.IsDawn && GameControllerScript.DayNight.dawnFocus.Contains(ResourceType.LovePotion) == false)
        {
            return - 1;
        }

        ItemScript item = stats.holdingItem;

        return 0;
    }

    //If the quick time event was successful
    protected override void Success(int pref_num)
    {
        // Plays hugging sound/sting
        FMOD_ControlScript.PlaySoundOneShot(hugSoundEventPath, transform.position);

        if (pref_num == 1)
        {
            CreateParticle(loveheartParticle, 12f);
            Transform h = huggingStarter.Holding;

            if (h != null)
            {
                h.GetComponent<ItemScript>().resourceCompound.resourceEffect = ResourceEffect.Lovable;
                GameObject particle = Instantiate(isLovelyParticle, h.transform.position, Quaternion.identity);
                particle.transform.parent = h.transform;
            }

            huggingStarter = null;
        }
        else if (pref_num == 0)
        {
            CreateParticle(loveheartParticle, 12f);
            GameObject leFruit = Instantiate(loveFruitPrefab, LastInteractedPlayers[0].transform.position
                                                + LastInteractedPlayers[0].transform.forward, Quaternion.identity);

            LastInteractedPlayers[0].GrabItem(leFruit.GetComponent<ItemScript>());

            Instantiate(GameControllerScript.juicyTextPrefab, transform.position, Quaternion.identity).GetComponent<TextMesh>().text = "Lovely";
        }
        

        //Tutorial Trigger
        TutorialScript.Trigger(TutorialTrigger.HugSuccess);
    }

    //If the quick time event was successful
    protected override void Fail(int pref_num)
    {
        huggingStarter = null;
    }

    //Can begin the hug if the player is holding the purified flower (0)
    //This is used by Store() below to determine what to do with the item
    protected override int StoreOption(PlayerScript p, PlayerStats stats)
    {
        if (huggingStarter == null && resourcesAccepting.Contains(stats.holdingItem))
        {
            return 0;
        }

        return -1;
    }

    //Check the resource type and send it to the Game Controller.
    //Store Objects inside this object. Override from InteractorScript
    protected override bool Store(ItemScript itemToStore, PlayerScript player, int storeOption)
    {
        if (storeOption == 0)
        {
            player.SetMyQTE(InterfaceInteractor(true, player));
            huggingStarter = player;
        }

        return false; //Wasn't successfully stored
    }
}



// Stewart = the only agent capable of a performing a royal guard with one arm, and eating a resistance burger with the other