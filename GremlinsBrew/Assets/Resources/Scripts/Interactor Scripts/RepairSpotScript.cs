using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RepairSpotScript : InteractorScript
{
    public Transform mySpriteTransform;
    public SpriteRenderer mySprite;
    public GameObject coinIcon;
    public TextMesh coinDisplay;
    public BreakagePointScript connectedBreakpoint;

    public bool active = false;
    private int repairCost = 1; //How many coins needed to repair

    void Start()
    {
        //storeOptionNames = new string[] { "Repair with material" };

        cooldownAmount = 0.01f; //You can set a custom number for how long after a QTE it takes before this interactor can be used again

        Init(Interactables.RepairSpot, "Prefabs/GridSystem/SnapCauldron", 0); //## Make sure init is called in the start method (used for interactor setup)
    }

    public int RepairCost
    {
        set
        {
            repairCost = value;
            coinDisplay.text = " " + repairCost;
        }
    }

    protected override void OverridenUpdate()
    {
        float nightAmount = GameControllerScript.DayNight.nightAmount;

        //Rotate the repair indication sprite
        Vector3 rot = mySpriteTransform.eulerAngles;
        rot.y += 0.7f; //Rotate the repair spot
        mySpriteTransform.eulerAngles = rot;

        if (active)
        {
            if (mySprite.enabled == false && GameControllerScript.local.DayEndMenu.activeSelf == false && GameControllerScript.local.BUYMENU.gameObject.activeSelf == false)
            {
                mySprite.enabled = true;
                coinIcon.SetActive(true);
                coinDisplay.gameObject.SetActive(true);
                TutorialScript.NewTutorial(TutorialType.NeedsRepair);
            }
        }
    }
    
    //Can repair
    protected override int Interact1Option(PlayerStats stats)
    {
        if (GameControllerScript.local.CanRepair == false) { return -1; }
        if (GameControllerScript.local.Coins.Count < repairCost) { return -1; }

        return (active && this.currentQTE == null ) ? 0 : -1; //Only one person can repair at a time??
    }

    //Repair successful
    protected override void Success(int pref_num)
    {
        if (pref_num == 0)
        {
            Debug.Log("Repair QTE was successful");
            if (GameControllerScript.MakePurchase(repairCost))
            {
                connectedBreakpoint.Repaired();
                TutorialScript.Trigger(TutorialTrigger.Repairing);
            }
        }
    }

    protected override int StoreOption(PlayerScript p, PlayerStats stats)
    {
        return -1; //Player can't store anything here
    }

    protected override bool Store(ItemScript itemToStore, PlayerScript Player, int storeOption)
    {
        if (storeOption == 0) {
            container[itemToStore.resourceCompound.resourceType] = itemToStore.resourceCompound.resourceEffect;
            return true;
        }

        return false;
    }

    public void Activate(bool b)
    {
        active = b;
        mySprite.gameObject.SetActive(b);
        coinIcon.SetActive(b);
    }
}
