using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomerControlScript : MonoBehaviour
{
    private GameControllerScript localGameControl;

    [SerializeField]
    private CustomerMoverScript customerMove;
    private DayNightScript dayNightScript;

    [SerializeField]
    public List<CustomerPatience> activeCustomers = new List<CustomerPatience>();

    public CustomerPreset customerLineup;

    public float numberOfCustomers;
    private int overstockedPotions;

    [SerializeField]
    float _custNumInc = 0.1f, _custNumMax = 12; //Percentage increase each day, and the maximum

    //public bool canDamageMachine = true;

    float previousNightProgress = 1;
    private float patience;
    public float patienceStart, patienceEnd, patienceChange;

    // Start is called before the first frame update
    void Start()
    {
        localGameControl = GetComponent<GameControllerScript>();
        customerLineup = new CustomerPreset();

        _custNumInc += 1;

        patience = patienceStart;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (dayNightScript == null) { dayNightScript = GameControllerScript.DayNight; return; }

        float nightProgress = GameControllerScript.NightProgress;
        //int currentDay = dayNightScript.currentDay;
        float seconds = dayNightScript.Seconds;

        if (!GameControllerScript.Paused)
        {
            if (nightProgress == 0)
            {
                if (previousNightProgress != 0)
                {
                    int num = CalculateNumberOfCustomers(); //Number of custoemrs for this day

                    if (dayNightScript.currentDay != 0) { dayNightScript.dayLength = 90; }
                    customerLineup.Setup(dayNightScript.dayLength, num);
                    customerMove.CustomerSetup(num);

                    RampDifficulty(); //Increases the difficulty for tomorrow
                }

                if (customerLineup.GetCustomer(seconds))
                {
                    activeCustomers.Add(new CustomerPatience(patience, customerLineup.currentType));
                    customerMove.StartWaiting();
                    Debug.Log("Wow");
                }

                foreach (CustomerPatience customer in activeCustomers)
                {
                    _requesting = customer.requesting;

                    //Debug.Log("I am requesting : " + customer.requesting.ToString());
                    if (localGameControl.CustomerBuy(customer.requesting, 1))
                    {
                        //the unending war with the pickup animation - will be here just walking animation for now
                        if (customerMove.customerWait.Count > 0)
                        {
                            customerMove.customerWait[0].GetComponent<CustomerModelScript>().myAnimator.SetBool("Walk", true);
                        }
                        else if (customerMove.customerArrive.Count > 0)
                        {
                            customerMove.customerArrive[0].GetComponent<CustomerModelScript>().myAnimator.SetBool("Walk", true);
                        }

                        activeCustomers.Remove(customer);
                        customerMove.GetOuttaHere();
                        dayNightScript.UpdatePatience(1);
                        break;
                    }
                    else
                    {
                        //// Patience audio plays if it's not already playing
                        //if (FMOD_ControlScript.patienceAudio != null && !FMOD_ControlScript.patienceAudio.isPlaying)
                        //{
                        //    FMOD_ControlScript.PlayPatienceAudio(customer.patience);
                        //}


                        customer.patience -= Time.deltaTime; // moving faster than actual time?
                        if (customer.patience <= 0)
                        {
                            customerMove.customerWait[0].GetComponent<CustomerModelScript>().myAnimator.SetBool("Hulk", true);
                            GameControllerScript.Damage(); //Damage machine
                            activeCustomers.Remove(customer);
                            customerMove.GetOuttaHere();
                            dayNightScript.UpdatePatience(1);
                            break;
                        }
                        else
                        {
                            dayNightScript.UpdatePatience(customer.patience / patience);
                        }
                    }

                }

                customerMove.MoveCustomer(CustomerTimeFrom(0));
               
            }
            else if (nightProgress == 1)
            {
                if (previousNightProgress != 1)
                {
                    if(activeCustomers.Count > 0)
                    {
                        foreach(CustomerPatience c in activeCustomers)
                        {
                            customerMove.GetOuttaHere(); //Move the customer away
                        }

                        activeCustomers.Clear();
                        
                    }
                }
            }
            previousNightProgress = nightProgress;
        }
    }

    public int CustomersRemaining
    {
        get
        {
            if (dayNightScript == null) { return 0; }
            //int currentDay = dayNightScript.currentDay;
            //if (currentDay == 0) { return 0; }

            return customerLineup.Remaining;
        }
    }

    private int CalculateNumberOfCustomers()
    {
        float overstockAdditions = overstockedPotions * (numberOfCustomers / _custNumMax);
        return (int)(numberOfCustomers + overstockAdditions);
    }

    public float MaxCustomers
    {
        get
        {
            return _custNumMax;
        }
    }

    private ResourceType _requesting = ResourceType.Empty;
    public string Requesting
    {
        get
        {
            return _requesting.ToString();
        }
    }

    public float CustomerTimeFrom(int next)
    {
        return customerLineup.CustomerTimeFrom(next);
    }

    //If a customer is waiting
    public bool IsWaitingOnPotion(ResourceType checkType)
    {

        foreach (CustomerPatience customer in activeCustomers)
        {
            if (customer.requesting == checkType) { return true; }
        }

        return false;
    }

    //Called each day, increases the difficulty
    private void RampDifficulty()
    {
        if (numberOfCustomers < _custNumMax)
        {
            numberOfCustomers = Mathf.Min(numberOfCustomers * _custNumInc, _custNumMax);
        }

        patience = Mathf.MoveTowards(patience, patienceEnd, patienceChange);

        //dayNightScript.dayLength += 1f;
        //_custNumMax += 0.5f;
    }

    public int OverstockedPotions { set { overstockedPotions = value; } }

    public float CurrentPatience => patience;
}

//The container for customers and what they want
[System.Serializable]
public class CustomerPatience
{
    public float patience;
    public ResourceType requesting;

    public CustomerPatience(float p, ResourceType r)
    {
        patience = p;
        requesting = r;
    }
}

//The collection of customers for the day
[System.Serializable]
public class CustomerPreset
{
    public int howMany;

    private TimeAndType[] customers;
    public ResourceType currentType;
    public int i = 0;

    /// <summary>
    /// How many customers are left
    /// </summary>
    public int Remaining
    {
        get
        {
            if (customers == null) { return 0; }
            return customers.Length - i;
        }
    }

    public float CustomerTimeFrom(int next)
    {
        int a = i + next;
        if (i < customers.Length)
        {
            return customers[a].when;
        }
        else
        {
            return 0;
        }
    }

    public ResourceType GetIndividualCustomerRequest(int index)
    {
        if (index < customers.Length)
        {
            return customers[index].what;
        }
        else
        {
            Debug.LogWarning($"Index '{index}' is larger than customers.Length!");
            return ResourceType.Empty;
        }

        
    }

    /// <summary>
    /// Returns whether the next customer has arrived
    /// </summary>
    /// <param name="t"></param>
    /// <returns></returns>
    public bool GetCustomer(float t)
    {
        if (i < customers.Length)
        {
            TimeAndType nextCustomer = customers[i];

            if (t >= nextCustomer.when)
            {
                currentType = nextCustomer.what;
                i++;
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// This sets up the customer's positions for the day. If the day jumps forward suddenly, it will mess this up
    /// </summary>
    public void Setup(float dayLength, int howMany)
    {
        i = 0; //Reset index

        this.howMany = howMany;
        customers = new TimeAndType[howMany];

        dayLength -= 5;
        float spacing = dayLength / howMany; //Average spacing between each customer

        for (int i = 0; i < howMany; i++)
        {
            float start = spacing / 2 + (spacing * i) + 5;
            float spacingFraction = (spacing / 3);
            float when = Random.Range(start - spacingFraction, start + spacingFraction);

            //customers[i] = new TimeAndType(PickPotionHealthBias(), when);
            customers[i] = new TimeAndType(PickPotionWeighted(), when);

            if (GameControllerScript.local.currentUnlockedPotion.Contains(ResourceType.HealthPotion) && Random.Range(0,3) == 2)
            {
                customers[i] = new TimeAndType(ResourceType.HealthPotion, when);
            }
            else
            {
                int r = Random.Range(0, GameControllerScript.local.currentUnlockedPotion.Count);
                customers[i] = new TimeAndType(GameControllerScript.local.currentUnlockedPotion[r], when);
            }
        }

    }

    private ResourceType PickPotionWeighted()
    {
        int hp = GameControllerScript.StockLevel(ResourceType.HealthPotion), pp = GameControllerScript.StockLevel(ResourceType.PoisonPotion),
            lp = GameControllerScript.StockLevel(ResourceType.LovePotion), mp = GameControllerScript.StockLevel(ResourceType.ManaPotion);

        int choiceTotal = ((hp != -1) ? (hp+1) : 0) + ((pp != -1)?(pp+1):0) + ((lp != -1) ? (lp+1) : 0) + ((mp != -1) ? (mp+1) : 0);

        int r = Random.Range(0, choiceTotal);
        if (hp > -1)
        {
            if (r <= hp) { return ResourceType.HealthPotion; }
            r -= hp;
        }
        if (pp > -1)
        {
            if (r <= pp) { return ResourceType.PoisonPotion; }
            r -= pp;
        }
        if (lp > -1)
        {
            if (r <= lp) { return ResourceType.LovePotion; }
            r -= lp;
        }
        if (mp > -1)
        {
            if (r <= mp) { return ResourceType.ManaPotion; }
            r -= mp;
        }

        return ResourceType.HealthPotion;
    }

    private ResourceType PickPotionHealthBias()
    {
        if (GameControllerScript.local.currentUnlockedPotion.Contains(ResourceType.HealthPotion) && Random.Range(0, 3) == 2)
        {
            return ResourceType.HealthPotion;
        }
        else
        {
            int r = Random.Range(0, GameControllerScript.local.currentUnlockedPotion.Count);
            return GameControllerScript.local.currentUnlockedPotion[r];
        }
    }

    private class TimeAndType
    {
        public ResourceType what;
        public float when;

        public TimeAndType(ResourceType what, float when)
        {
            this.what = what;
            this.when = when;
        }
    }
}
