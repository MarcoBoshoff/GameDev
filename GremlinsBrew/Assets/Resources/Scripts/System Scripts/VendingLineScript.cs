using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VendingLineScript : MonoBehaviour
{
    GameObject attachmentPrefab;
    public float length = 55; //Public variable exposed to inspector
    public float moveSpeed = 0.11f;

    private Vector3 startPoint, endPoint;
    List<ConveyourAttachmentScript> attachments = new List<ConveyourAttachmentScript>();

    private Vector3 myDir = Vector3.right;
    private float spacingAmount = 3.5f;

    // Start is called before the first frame update
    public void Setup()
    {
        attachmentPrefab = Resources.Load("Prefabs/ConveyorAttachment") as GameObject;

        startPoint = transform.position + myDir * (length / 4);
        endPoint = transform.position - myDir * (length / 2);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        for (int i = attachments.Count - 1; i >= 0; i--)
        {
            ConveyourAttachmentScript attachment = attachments[i];
            if (attachment == null) { attachments.Remove(attachment); continue; }

            //attachment.transform.Translate(Vector3.right*moveSpeed);
            attachment.transform.position = Vector3.MoveTowards(attachment.transform.position, endPoint + (myDir * i * spacingAmount), moveSpeed);
        }
    }

    Vector3 offset = Vector3.up * 0.5f;
    public ConveyourAttachmentScript PrespawnItem(ResourceType t, GameObject prefab)
    {
        GameObject obj = Instantiate(attachmentPrefab, endPoint + (myDir * attachments.Count * spacingAmount), Quaternion.identity);
        ConveyourAttachmentScript newAttachment = obj.GetComponent<ConveyourAttachmentScript>();

        FixedJoint j = Instantiate(prefab, newAttachment.transform.position - offset, Quaternion.identity).AddComponent<FixedJoint>();
        newAttachment.attached = j;
        j.connectedBody = newAttachment.GetComponent<Rigidbody>();

        Vector3 tempForce = new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f));
        if (tempForce != Vector3.zero) {
            j.transform.GetChild(1).GetComponent<Rigidbody>().AddForce(tempForce.normalized*4, ForceMode.Impulse);
        }

        attachments.Add(newAttachment);
        return newAttachment;
    }

    public ConveyourAttachmentScript SpawnItem(ResourceType t, GameObject prefab)
    {
        GameObject obj = Instantiate(attachmentPrefab, startPoint, Quaternion.identity);
        ConveyourAttachmentScript newAttachment = obj.GetComponent<ConveyourAttachmentScript>();

        FixedJoint j = Instantiate(prefab, newAttachment.transform.position - offset, Quaternion.identity).AddComponent<FixedJoint>();
        newAttachment.attached = j;
        j.connectedBody = newAttachment.GetComponent<Rigidbody>();

        Vector3 tempForce = new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f));
        if (tempForce != Vector3.zero)
        {
            j.transform.GetChild(1).GetComponent<Rigidbody>().AddForce(tempForce.normalized * 8, ForceMode.Impulse);
        }

        attachments.Add(newAttachment);
        return newAttachment;
    }
}



// Nathan = Definitely not lousy! Just don't tickle his feet
