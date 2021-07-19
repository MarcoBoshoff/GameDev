using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DUMMYQTE_DummyScript : QTEScript
{
    //##IMPORTANT##
    //IF CREATING A NEW QTE_???Script, remember to also make a new prefab for it in Resources/Prefabs/Quick_Time_Events/
    //Then, create a new type for it below
    //Finally, add that type and prefab to the dictionary in the QTEPrefsScript. Line ~27


    // ## Any variables specific to this QTE go here


    //Override for any values before setup starts
    public override void SetupStarting(){ }

    //Override for any values after setup has finished -> ie, after all the variables have been assigned by the QTEPrefsScript
    public override void SetupFinished() { }

    //## If not needed, you can delete the above 2 methods

    //Receive Movement Input
    public override void MovementInput(PlayerScript player, Vector2 movement)
    {
        QTEPlayer qtePlayer = attachedPlayers[player];

        if (qtePlayer != null)
        {

            if (movement.magnitude > 1)
            {
                movement = movement.normalized;
            }

            //## Can apply that movement to something here. If not needed, delete this method
        }
    }

    //Just the update method
    protected override void QteUpdate(int playerCount)
    {
        //## You want to have it that the End() method is called somehow
        //## Pass in false if it failed, or true if it succeed

        bool QTE_Succeed = true; //## Just an example haha

        if (QTE_Succeed)
        {
            End(true); //## Calls the success method of the interactor (because 'true' was passed)
        }
        else
        {
            //Do something else
            //## Again, just an example. You can redo this whole method, just as long as End() can be called 
        }
        
    }

    //## These are all values you can use to assign to the QTE. They are called by the QTEPrefsScript, depending on the values
    //     that were set in the inspector.
    //## See other QTE scripts for examples.
    public override void SetFloat(float f) { }

    public override void SetBool(bool b) { }

    public override void SetInt(int i) { }

    public override void AddPoint(Vector3 point) { }

    //## If you need specific things to happen each time a new player is added, handle that here
    protected override void PlayerAdded(PlayerScript player) { }

    //## And, as always, if you don't need any of these overrided methods, just delete them!
}
