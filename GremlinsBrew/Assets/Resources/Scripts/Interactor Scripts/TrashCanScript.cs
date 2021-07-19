using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrashCanScript : InteractorScript
{ 
    void Start()
    {
        //## These are important (if you want to be able to store items in this interactor)
        storeOptionNames = new string[] { "Trash It" }; //Different store option names
        canBeMoved = false;

        Init(Interactables.None, "Prefabs/GridSystem/SnapCauldron", 0); //## Make sure init is called in the start method (used for interactor setup)
    }

    //Sets which type of storing this is employing (if putting in an item (0), if using a potion bottle (1))
    //This is used by Store() below to determine what to do with the item
    protected override int StoreOption(PlayerScript p, PlayerStats stats)
    {
        if (stats.holdingItem != null)
        {
            return 0;
        }

        return -1; //No applicable options
    }

    //Store Objects inside this object. Override from InteractorScript
    protected override bool Store(ItemScript itemToStore, PlayerScript Player, int storeOption)
    {
        if (storeOption == 0)
        {
            return true;
        }

        return false; //Wasn't successfully stored
    }
}
