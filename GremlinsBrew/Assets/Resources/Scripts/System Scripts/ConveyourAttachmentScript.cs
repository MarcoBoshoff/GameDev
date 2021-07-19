using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConveyourAttachmentScript : MonoBehaviour
{
    public FixedJoint attached;

    public void Detach()
    {
        if (attached != null)
        {
            Destroy(attached.GetComponent<FixedJoint>());
            Destroy(this.gameObject);
        }
    }

    public void OnTriggerEnter(Collider other)
    {
        //Debug.Log(other.tag);
        if (other.tag.Equals("ConveyourDetach"))
        {
            Detach();
        }
    }
}
