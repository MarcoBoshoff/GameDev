using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DetectionSphereScript : MonoBehaviour
{
    [SerializeField]
    public List<InteractorScript> myInteractors = new List<InteractorScript>();
    public List<ItemScript> nearbyItems = new List<ItemScript>();

    //List of all nearby interactors
    void OnTriggerEnter(Collider other)
    {
        if (other.tag.Equals("Interactable"))
        {
            InteractorScript interactor = other.GetComponent<InteractorScript>();
            if (!myInteractors.Contains(interactor))
            {
                myInteractors.Add(interactor);
            }

        }
        else if (other.tag.Equals("Pickup") || other.tag.Equals("PotionBottle") 
                    || other.tag.Equals("Heavy") || other.tag.Equals("Coin"))
        {
            ItemScript item = other.GetComponent<ItemScript>();
            if (!nearbyItems.Contains(item))
            {
                nearbyItems.Add(item);
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.tag.Equals("Interactable"))
        {
            InteractorScript interactor = other.GetComponent<InteractorScript>();
            if (myInteractors.Contains(interactor))
            {
                myInteractors.Remove(interactor);
            }
        }
        else if (other.tag.Equals("Pickup") || other.tag.Equals("PotionBottle")
                    || other.tag.Equals("Heavy") || other.tag.Equals("Coin"))
        {
            ItemScript item = other.GetComponent<ItemScript>();
            if (nearbyItems.Contains(item))
            {
                nearbyItems.Remove(item);
            }
        }
    }
}



//* *     * * * * 
//*   * *  Mat W  *
//*   * *         *
//* *     * * * *
//Le Chocolate Salmon 