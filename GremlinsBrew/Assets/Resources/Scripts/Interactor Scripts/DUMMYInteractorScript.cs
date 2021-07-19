using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DummyInteractorScript : InteractorScript
{
    //## (Optional) Place variables here that arespecific to your interactor.

    void Start()
    {
        //## This is important if you want to be able to store items in this interactor
        storeOptionNames = new string[] { "Store item", "Collect potion" }; //Different store option names
        //## The ordering will relate directly to the store options method below


        cooldownAmount = 1f; //You can set a custom number for how long after a QTE it takes before this interactor can be used again


        //## Again, you can place anything specific to this interactor here


        Init(Interactables.None, "Prefabs/GridSystem/SnapCauldron", 1); //## Make sure init is called in the start method (used for interactor setup)
    }

    //## Need to have an update method?
    protected override void OverridenUpdate()
    {

    }

    //## Return a number for which interact the player is currently able to use (return -1 if no currently avaialbe interaction)
    protected override int Interact1Option(PlayerStats stats)
    {
        return (container.GetPotion() != ResourceType.Empty) ? 0 : -1; //## This is just an example, limits the first interaction if it contains a potion
    }

    //## Set a limit when the second interaction can't be called (set to false to have no limit)
    //## Defaults to disabled, so delete the below if you want that to be the case
    protected override int Interact2Option(PlayerStats stats)
    {
        return Interact1Option(stats); //## Again, to the left is just an example
    }


    //## This method is called when the quick time event is successfully completed
    protected override void Success(int pref_num)
    {
        //##You can use the bool passed into this method to decide which quick time event was completed
        if (pref_num == 0)
        {
            //##This method is called when the first interact's QTE is completed, etc
        }
    }

    //## Again, you can call this if the QTE fails
    protected override void Fail(int pref_num)
    {
        //## Do something... or nothing, up to you :)
    }



    //## This is important for storing. If you don't wish to store anything, return -1;
    //## Return a 0 or larger if the player can store there
    //## That number (0+) is used to get the prompt from above (storeOptionNames) in Start()
    //## It is also used in the Store function below. It simply sees which option number is being
    //      returned, and performs the related action
    protected override int StoreOption(PlayerScript p, PlayerStats stats)
    {
        //## This is just an example below
        if (stats.holdingItem != null)
        {
            return 0; //## Example: if the player is holding something, return 0
        }

        return -1; //## Return -1 if the player can't store anything at the moment
    }


    //## Store items inside this object. The 'int storeOption' is calculated above. Don't calculate any options here!
    //## Just check which storeOption is being passed in as a parameter
    protected override bool Store(ItemScript itemToStore, PlayerScript Player, int storeOption)
    {
        //## Example: If the first store option (which we defined above as the player simply holding an item...
        //...which will be 100% of the time, as store option is only called when the player is aready holding an item
        if (storeOption == 0) {

            //## You can use this to place the item in the container
            container[itemToStore.resourceCompound.resourceType] = itemToStore.resourceCompound.resourceEffect;

            return true; //## Make sure you return true if you want the player to delete the item they're holding
                         // If you return false, the player will continue holding their item, even if you stored
                         // its type into this interactor's container
        }
        else if (storeOption == 1)
        {
            //## Example. This will never be called at the moment because we never return 1 in the storeOption() method above
        }

        return false; //## Retun false if the item Wasn't successfully stored, or if you simply don't want the player to delete the
                      // item they're holding (i.e. you're basically making a copy of that item)
    }


    // ## And of course, you can have any method heres that are specific to your interactor. Probably best to make them private
    //     unless other classes need to call them.
}
