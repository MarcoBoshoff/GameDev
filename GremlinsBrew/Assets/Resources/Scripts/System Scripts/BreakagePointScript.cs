using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BreakagePointScript : MonoBehaviour
{
    public GameObject RepairSpotPrefab, brickPatch, woodStack;
    private RepairSpotScript repairSpot;

    // Start is called before the first frame update
    void Start()
    {
        Vector3 pos = transform.position;
        pos.y = 0;

        repairSpot = Instantiate(RepairSpotPrefab, pos, Quaternion.identity).GetComponent<RepairSpotScript>();
        repairSpot.connectedBreakpoint = this;


        Vector3 plankPos = new Vector3(0, 20, 0);
        int childCount = woodStack.transform.childCount;
        for (int i = 0; i < childCount; i++)
        {
            Transform wood = woodStack.transform.GetChild(i);
            wood.position = plankPos;
            wood.gameObject.SetActive(false);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public bool Repaired()
    {
        Debug.Log("Repair called");
        if (repairSpot.active)
        {
            int childCount = brickPatch.transform.childCount;
            for (int i = 0; i < childCount; i++)
            {
                ObjReturnScript brickReturn = brickPatch.transform.GetChild(i).GetComponent<ObjReturnScript>();
                if (brickReturn != null)
                {
                    brickReturn.ReturnToStart();
                }
            }

            childCount = woodStack.transform.childCount;
            for (int i = 0; i < childCount; i++)
            {
                ObjReturnScript woodReturn = woodStack.transform.GetChild(i).GetComponent<ObjReturnScript>();
                if (woodReturn != null)
                {
                    //Enables the wood stack for the first repair
                    if (!woodReturn.gameObject.activeSelf) { woodReturn.gameObject.SetActive(true); }

                    //Returns woodstack to positions
                    woodReturn.ReturnToStart();
                }
            }

            repairSpot.Activate(false);
            GameControllerScript.RepairMachine();
            Debug.Log("Returned TRUE");
            return true;
        }

        return false;
    }

    public bool Damaged(int repairCost)
    {
        if (!repairSpot.active)
        {
            int childCount = brickPatch.transform.childCount;
            for (int i = 0; i < childCount; i++)
            {
                Rigidbody brick = brickPatch.transform.GetChild(i).GetComponent<Rigidbody>();
                if (brick != null) { brick.isKinematic = false; }
            }

            if (woodStack.transform.GetChild(1).gameObject.activeSelf)
            {
                childCount = woodStack.transform.childCount;
                for (int i = 0; i < childCount; i++)
                {
                    Rigidbody wood = woodStack.transform.GetChild(i).GetComponent<Rigidbody>();
                    if (wood != null) { wood.isKinematic = false; }
                }
            }

            repairSpot.Activate(true);
            repairSpot.RepairCost = repairCost;
            return true;
        }

        return false;
    }
}
