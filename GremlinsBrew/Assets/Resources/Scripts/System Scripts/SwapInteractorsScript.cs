using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwapInteractorsScript : MonoBehaviour
{
    InteractorScript interactor1, interactor2;

    public void Swap(InteractorScript i1, InteractorScript i2)
    {
        interactor1 = i1;
        interactor2 = i2;

        i1.UntagInteractor();
        i2.UntagInteractor();

        Vector3 temp = i1.transform.position;
        i1.transform.position = i2.transform.position;
        i2.transform.position = temp;

        GameControllerScript.GridPoint[] tempPoints = i1.gridPoints;
        i1.gridPoints = i2.gridPoints;
        i2.gridPoints = tempPoints;

        Destroy(this); //Finished swapping
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        //TODO: Add more visual elements to the swapping (green phases swapping, particles, etc)
    }
}
