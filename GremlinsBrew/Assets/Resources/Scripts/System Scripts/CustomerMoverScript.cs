using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomerMoverScript : MonoBehaviour
{
    public GameObject customerPrefab;
    public DayNightScript dayNightScript;
    public CustomerControlScript customerControl;
    private List<Vector3> endPositionList = new List<Vector3>();

    public Quaternion faceRight, faceForward;

    //reference for customer animator
    public CustomerModelScript customerAnim;

    public List<GameObject> customerArrive = new List<GameObject>();
    private List<CustomerModelScript> customerLeave = new List<CustomerModelScript>();
    public List<GameObject> customerWait = new List<GameObject>();
    float dayLength;

    void Start()
    {
        dayLength = dayNightScript.dayLength;

        faceRight = Quaternion.Euler(0, -90, 0);
        faceForward = Quaternion.Euler(0, 0, 0);

        ////endPosition = transform.position;
        ////endPosition.x = -0.6f;
    }

    //these variables adjust the time that the customer shows up before the potion is bought
    float startWalk = 5, stopTime = 3;
    //this function is called in the CustomerControlScript - moves the customer characters to the vending machine window
    //then moves them away once a potion is purchased
    public void MoveCustomer(float timeFrom)
    {
        if (customerArrive.Count > 0)
        {
            float currentTime = dayNightScript.Seconds;
            float t = timeFrom - currentTime;

            if (t < startWalk + stopTime)
            {
                //TODO: Set walking bool to true, etc
                customerAnim = customerArrive[0].GetComponent<CustomerModelScript>();

                customerAnim.TargetRot(faceRight);

                customerAnim.myAnimator.SetBool("Walk", true);
                //Debug.Log("I'm walkin here!");

                float percentage = Mathf.Max(t - stopTime, 0) / startWalk;
                percentage = 1 - percentage;
                customerArrive[0].transform.position = Vector3.Lerp(transform.position, endPositionList[0], percentage);

                if(t < stopTime)
                {
                    customerAnim.myAnimator.SetBool("Walk", false);    
                    customerAnim.myAnimator.SetBool("Order", true);
                    //customerAnim.myAnimator.SetBool("Pickup", true);

                    customerAnim.TargetRot(faceForward);
                }
            }
        }

        if (customerLeave.Count > 0)
        {
            foreach(CustomerModelScript customer in customerLeave)
            {
                if (customer.pause == 0)
                {
                    customer.transform.Translate(Vector3.right * Time.fixedDeltaTime * 10, Space.World);
                    customer.TargetRot(faceRight);
                    customer.myAnimator.SetBool("Walk", true);
                }
            }
        }


    }

    //adds customers to the waiting list
    public void StartWaiting()
    {
        if (customerArrive.Count > 0)
        {
            GameObject owen = customerArrive[0];
            customerAnim = owen.GetComponent<CustomerModelScript>();
            //Debug.Log("Even bigger wow");
            customerArrive.Remove(owen);
            endPositionList.RemoveAt(0);
            customerWait.Add(owen);
        }
    }

    //this function spawns the customer characters at the start of each day and deletes the ones from the previous
    public void CustomerSetup(int customerAmount)
    {
        foreach (GameObject customer in customerArrive)
        {
            Destroy(customer);

        }
        customerArrive.Clear();

        foreach (GameObject customer in customerWait)
        {
            Destroy(customer);

        }
        customerWait.Clear();

        foreach (CustomerModelScript customer in customerLeave)
        {
            Destroy(customer.gameObject);

        }
        customerLeave.Clear();

        endPositionList.Clear();

        int j = 0;
        for (int i = 0; i < customerAmount; i++)
        {
            endPositionList.Add(new Vector3(-7.6f + (i * 5), -6f, 35.1f));
            while (endPositionList[i].x >= 7.6f)
            {
                endPositionList[i] = new Vector3(-7.6f + (j * 5), -6f, 35.1f);
                j += 1;

                if(j >= i)
                {
                    j = 0;
                }
            }
            
            GameObject customer = Instantiate(customerPrefab, transform.position, Quaternion.identity);
            customerArrive.Add(customer);

            CustomerModelScript customerModel = customer.GetComponent<CustomerModelScript>();

            switch (customerControl.customerLineup.GetIndividualCustomerRequest(i))
            {
                case ResourceType.HealthPotion:
                    {
                        customerModel.customerHealth.SetActive(true);
                        //Debug.Log("Health customer spawned");
                        break;
                    }
                case ResourceType.PoisonPotion:
                    {
                        customerModel.customerPoison.SetActive(true);
                        //Debug.Log("Poison customer spawned");
                        break;
                    }
                case ResourceType.LovePotion:
                    {
                        customerModel.customerLove.SetActive(true);
                        //Debug.Log("Love customer spawned");
                        break;
                    }
                case ResourceType.ManaPotion:
                    {
                        customerModel.customerMana.SetActive(true);
                        //Debug.Log("Mana customer spawned");
                        break;
                    }
                default:
                    {
                        customerModel.customerHealth.SetActive(true);
                        //Debug.Log("Default");
                        break;
                    }
            }
            
        }
    }

    //this makes the customer leave
    public void GetOuttaHere()
    {
        if (customerWait.Count > 0)
        {
            GameObject owen = customerWait[0];
            //Debug.Log("Sad wow");
            customerWait.Remove(owen);
            customerLeave.Add(owen.GetComponent<CustomerModelScript>());
            owen.GetComponent<CustomerModelScript>().pause = 1;
        }
    }

}

//Our customers like to move it! Move it!