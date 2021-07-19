using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIScript : MonoBehaviour
{
    private int storedHealth = 3, ingredientsToDisplay = 2;
    private float dayProgress;

    //Day/Night cycle
    public RectTransform clock;

    //stock tracking bar
    public GameObject UIBACKGROUND, ingredientUIPrefab, promptPlusPrefab;

    private Vector3 ingredientScaling = new Vector3(0.1f, 0.1f, 0.1f);

    private Dictionary<ResourceType, IngredientsUIScript> allIngredients = new Dictionary<ResourceType, IngredientsUIScript>();
    private List<IngredientsUIScript> currentIngredients = new List<IngredientsUIScript>();

    public DayNightScript dayNight;
    public CustomerControlScript customerControl;
	
    //Text variables for UICanvas
    public Text coins, timeLeft;
    public GameObject shopIcon;

    public Image[] hearts;
    public Sprite blackHeart, redHeart;
    //public Text customerPurchase;

    // Update is called once per frame
    private bool initialized = false;
    void FixedUpdate()
    {
        if (!initialized)
        {
            if (GameControllerScript.local != null)
            {
                foreach (ResourceType potion in GameControllerScript.local.currentUnlockedPotion)
                {
                    IngredientsUIScript newIngredients = Instantiate(ingredientUIPrefab).GetComponent<IngredientsUIScript>();
                    allIngredients[potion] = newIngredients;
                    newIngredients.PotionType = potion;

                    newIngredients.transform.parent = UIBACKGROUND.transform;
                    newIngredients.name = potion.ToString() + "_IngredientsUI";

                    newIngredients.rect.anchoredPosition = new Vector3(0, 8, 0);
                    newIngredients.rect.localScale = ingredientScaling;

                }

                foreach (ResourceType potion in GameControllerScript.local.PotionsCatalog)
                {
                    IngredientsUIScript newIngredients = Instantiate(ingredientUIPrefab).GetComponent<IngredientsUIScript>();
                    allIngredients[potion] = newIngredients;
                    newIngredients.PotionType = potion;

                    newIngredients.transform.parent = UIBACKGROUND.transform;
                    newIngredients.name = potion.ToString() + "_IngredientsUI";

                    newIngredients.rect.anchoredPosition = new Vector3(0, 8, 0);
                    newIngredients.rect.localScale = ingredientScaling;
                }

                initialized = true;

                UpdateCurrentPotionsUI(true);
                CalculateSpacing();
            }
        }
        else
        {
            //lives counter
            int health = GameControllerScript.local.LIVES;
            dayProgress = GameControllerScript.NightProgress;

            if (health != storedHealth)
            {
                for (int i = 0; i < hearts.Length; i++)
                {
                    hearts[i].sprite = (i < health) ? redHeart : blackHeart;
                }
                //healthDisplay.text = health.ToString();

                storedHealth = health;
            }

            //Clock 
            Vector3 rot = clock.transform.eulerAngles;
            rot.z = 360 - Mathf.Lerp(90, 270, (dayNight.IsNight) ? (dayNight.nightAmount) : (dayNight.Seconds / dayNight.dayLength));
            clock.transform.eulerAngles = rot;

            //Time left
            string t_string = "00:00";
            if (dayNight.IsNight)
            {
                t_string = "Night";
            }
            else
            {
                float _m = Mathf.Lerp(dayNight.dayLength/60, 0, dayNight.Seconds / dayNight.dayLength);
                int _s = (int)((_m - Mathf.Floor(_m)) * 60);

                string hs = (_m < 10) ? ("0" + (int)_m) : (((int)_m).ToString());
                string ss = (_s < 10) ? ("0" + _s) : (_s.ToString());
                t_string = hs + ":" + ss;
            }
            timeLeft.text = t_string;

            //Shop Icon
            //shopIcon.SetActive(dayNight.nightAmount == 1 && !GameControllerScript.local.BUYMENU.gameObject.activeSelf);
            
            //TODO: Can we optimize this?
            UIBACKGROUND.SetActive(!GameControllerScript.local.DayEndMenu.activeSelf);


            //Update the displaying ingredients list
            bool recalculateSpacing = UpdateCurrentPotionsUI(dayNight.nightAmount >= 0);           
            if (recalculateSpacing) { CalculateSpacing(); }

            //Stock level & position
            //float maxStock = GameControllerScript.local.maxPotions;
            foreach (IngredientsUIScript ingredientUI in currentIngredients)
            {
                ingredientUI.UpdateStockLevel();
            }
            foreach (KeyValuePair<ResourceType, IngredientsUIScript> kp_ingredientsUI in allIngredients)
            {
                kp_ingredientsUI.Value.MoveToTarget();
            }

            ////Money counter
            int money = GameControllerScript.local.Coins.Count;
            string zeros = "";
            if (money < 100)
            {
                zeros += "0";
                if (money < 10)
                {
                    zeros += "0";
                }
            }
            coins.text = zeros + money;


            ////Day tracking
            //day.text = "Day: " + GameControllerScript.currentDay;

            ////customer purchase
            //customerPurchase.color = (Mathf.Floor(GameControllerScript.customerPatience / 2) == (GameControllerScript.customerPatience / 2))?Color.white:Color.red;
            //customerPurchase.text = "Next Purchase: " + GameControllerScript.nextCustomerPurchase;
        }
    }

    public void PromptStock(ResourceType potion, bool sold)
    {
        if (allIngredients.ContainsKey(potion))
        {
            Transform ppT = Instantiate(promptPlusPrefab, allIngredients[potion].transform.position + Vector3.right*Screen.height/12, Quaternion.identity).transform;

            ppT.parent = UIBACKGROUND.transform;
            ppT.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);

            Text promptPlus = ppT.GetChild(0).GetComponent<Text>();
            if (sold)
            {
                promptPlus.text = "SOLD";
            }
            else
            {
                promptPlus.text = "+1";
            }
        }
    }

    private bool UpdateCurrentPotionsUI(bool canCheck)
    {
        bool recalculateSpacing = false;

        int gcsCurrentUnlockedPotions = GameControllerScript.local.currentUnlockedPotion.Count;
        if (canCheck)
        {
            //Removes any potions that were unbought
            for (int i = currentIngredients.Count - 1; i >= 0; i--)
            {
                if (GameControllerScript.local.currentUnlockedPotion.Contains(currentIngredients[i].PotionType) == false)
                {
                    currentIngredients.RemoveAt(i);
                    recalculateSpacing = true;
                }
            }

            if (currentIngredients.Count < gcsCurrentUnlockedPotions)
            {
                Debug.Log("Looking for the potion ingredients ui to add");

                //Adds any potion that were bought
                for (int i = gcsCurrentUnlockedPotions - 1; i >= 0; i--)
                {
                    ResourceType potionToCheck = GameControllerScript.local.currentUnlockedPotion[i];

                    bool found = false;
                    foreach (IngredientsUIScript ingredients in currentIngredients)
                    {
                        found = (ingredients.PotionType == potionToCheck);
                        if (found) { break; }
                    }

                    if (!found)
                    {
                        IngredientsUIScript ingredientToAdd = allIngredients[potionToCheck];
                        if (currentIngredients.Contains(ingredientToAdd))
                        {
                            Debug.LogError(string.Format("Ingrident of type {0} is being added again", potionToCheck));
                        }
                        else
                        {
                            //This calculates the middile of the list
                            float middle = currentIngredients.Count / 2;
                            float rounded = Mathf.Floor(middle);
                            if (middle != rounded)
                            {
                                rounded += 1;
                            }

                            //Add the new ingredient to the middle of the list
                            currentIngredients.Insert((int)rounded, ingredientToAdd);
                            recalculateSpacing = true;
                        }
                    }
                }
            }
        }

        return recalculateSpacing;
    }

    float spacing = 15;
    private void CalculateSpacing()
    {
        //Calculates the positions based on the count and fix spacing size from above
        float count = currentIngredients.Count, totalSpacing = (count-1)*spacing;
        float startPoint = totalSpacing * -0.5f;

        //Applies the positions
        int i = 0;
        foreach (KeyValuePair<ResourceType, IngredientsUIScript> kp_ing in allIngredients)
        {
            IngredientsUIScript ingredientsUI = kp_ing.Value;
            if (currentIngredients.Contains(ingredientsUI))
            {
                ingredientsUI.targetPos.x = (count==1)?0:((startPoint + (spacing * i)));
                ingredientsUI.targetPos.y = -6;
                i++;
            }
            else
            {
                ingredientsUI.targetPos.x = 0;
                ingredientsUI.targetPos.y = 8;
            }
            
        }
        
    }
}

// Yong = Lit
