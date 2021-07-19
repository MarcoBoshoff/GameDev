using System.Collections;
using RotaryHeart.Lib.SerializableDictionary;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BuySelection : MonoBehaviour
{
    //[HideInInspector]
    public GameObject RecipeButtonPrefab, PlayerSelectionCursor;
    public Text TotalCoins;

    private GameObject[] Cursors = new GameObject[2];
    private Vector3 CursorOGScale, CursorLargeScale;

    public float CursorSpeed = 1f;
    public int ButtonCollumns = 2;
    public int[] buttonSelected = new int[2] { 0, 0 };
    public Sprite PoisonSprite, LoveSprite, ManaSprite, HealthSprite;

    public PotionSpriteDict ButtonBackgrond = new PotionSpriteDict();

    private ButtonBehaviour Button;

    //[HideInInspector]
    public List<Transform> CatalogButtons = new List<Transform>();
    private List<ResourceType> PotionsList = new List<ResourceType>();
    private Dictionary<Interactables, int> StationDependencies = new Dictionary<Interactables, int>(); //Are machines needed? 

    private List<ResourceType> AllPotions;

    private bool _haveSelectedMachine = false;  //Both Players have agreed on a recipe
    [SerializeField]
    private bool[] PlayerSelect = new bool[2], CursorMove = new bool[2] { true, true };

    public Image btnToClose;
    public Sprite closeBtnB, closeBtnO, closeBtnQ;

    //Called in the GameController Script at Start
    public void Init()
    {
        PlayerSelect[0] = false;
        PlayerSelect[1] = false;

        AllPotions = new List<ResourceType>() { ResourceType.HealthPotion, ResourceType.LovePotion, ResourceType.PoisonPotion, ResourceType.ManaPotion };
        int length = Mathf.Min(AllPotions.Count, 4);
        for (int i = 0; i < length; i++)
        {
            //Add all potions to a single list for easy reference
            PotionsList.Add(AllPotions[i]);

            CatalogButtons.Add(Instantiate(RecipeButtonPrefab).transform);
            CatalogButtons[i].transform.SetParent(this.transform, false);
            CatalogButtons[i].transform.localScale = new Vector3(4.026308f, 4.026308f, 4.026308f);
            Button = CatalogButtons[i].GetComponentInChildren<ButtonBehaviour>();

            Button.PotionRecipe = PotionsList[i]; //Random list...how

            switch (Button.PotionRecipe)
            {
                case ResourceType.HealthPotion:
                    {
                        CatalogButtons[i].GetComponentInChildren<ButtonBehaviour>().potionIcon.GetComponent<Image>().sprite = HealthSprite;
                        Button.Highlight.GetComponent<Image>().sprite = ButtonBackgrond[ResourceType.HealthPotion].Enabled;
                        Button.Disabled.GetComponent<Image>().sprite = ButtonBackgrond[ResourceType.HealthPotion].Disabled;
                        Button.ButtonText.GetComponent<Text>().text = "Refund";
                        GameControllerScript.local.PotionPrice[ResourceType.HealthPotion] /= 2;
                        Button.Unlocked = true;
                        Button.Highlight.SetActive(true);
                        Button.Disabled.SetActive(true);
                        break;
                    }
                case ResourceType.PoisonPotion:
                    {
                        CatalogButtons[i].GetComponentInChildren<ButtonBehaviour>().potionIcon.GetComponent<Image>().sprite = PoisonSprite;
                        Button.Highlight.GetComponent<Image>().sprite = ButtonBackgrond[ResourceType.PoisonPotion].Enabled;
                        Button.Disabled.GetComponent<Image>().sprite = ButtonBackgrond[ResourceType.PoisonPotion].Disabled;
                        Button.Disabled.SetActive(false);
                        break;
                    }
                case ResourceType.LovePotion:
                    {
                        CatalogButtons[i].GetComponentInChildren<ButtonBehaviour>().potionIcon.GetComponent<Image>().sprite = LoveSprite;
                        Button.Highlight.GetComponent<Image>().sprite = ButtonBackgrond[ResourceType.LovePotion].Enabled;
                        Button.Disabled.GetComponent<Image>().sprite = ButtonBackgrond[ResourceType.LovePotion].Disabled;
                        Button.Disabled.SetActive(false);
                        break;
                    }
                case ResourceType.ManaPotion:
                    {
                        CatalogButtons[i].GetComponentInChildren<ButtonBehaviour>().potionIcon.GetComponent<Image>().sprite = ManaSprite;
                        Button.Highlight.GetComponent<Image>().sprite = ButtonBackgrond[ResourceType.ManaPotion].Enabled;
                        Button.Disabled.GetComponent<Image>().sprite = ButtonBackgrond[ResourceType.ManaPotion].Disabled;
                        Button.Disabled.SetActive(false);
                        break;
                    }
                default:
                    {
                        CatalogButtons[i].GetComponentInChildren<ButtonBehaviour>().potionIcon.GetComponent<Image>().sprite = HealthSprite;
                        break;
                    }
            }

            float Price = GameControllerScript.local.PotionPrice[CatalogButtons[i].GetComponent<ButtonBehaviour>().PotionRecipe];
            CatalogButtons[i].GetComponentInChildren<Text>().text = Price.ToString();
            CatalogButtons[0].GetComponentInChildren<Text>().text = "+" + Mathf.RoundToInt(2).ToString();
            CatalogButtons[i].name = "Button" + i;
            /* PoisonSprite; LoveSprite; ManaSprite; FourthPotion;*/
        }
        CatalogButtons[0].transform.localPosition = new Vector3(-676, 153, 0);
        CatalogButtons[1].transform.localPosition = new Vector3(-230, 87, 0);
        CatalogButtons[2].transform.localPosition = new Vector3(212, 124, 0);
        CatalogButtons[3].transform.localPosition = new Vector3(655, 95, 0);



        //Cursor Creation
        Cursors[0] = Instantiate(PlayerSelectionCursor);
        Cursors[1] = Instantiate(PlayerSelectionCursor);

        Cursors[0].transform.SetParent(this.transform, false);
        Cursors[1].transform.SetParent(this.transform, false);

        CursorOGScale = Cursors[0].transform.localScale;
        CursorLargeScale = new Vector3(1.5f, 1.5f, 1.5f);

        Cursors[0].transform.GetChild(0).GetComponent<Image>().color = (GameControllerScript.PlayerColors != null)?GameControllerScript.PlayerColors[0]:Color.red;
        Cursors[1].transform.GetChild(0).GetComponent<Image>().color = (GameControllerScript.PlayerColors != null)?GameControllerScript.PlayerColors[1]:Color.cyan;

        Cursors[0].transform.position = CatalogButtons[0].GetComponent<ButtonBehaviour>().IconLocation[0].transform.position;
        Cursors[1].transform.position = CatalogButtons[0].GetComponent<ButtonBehaviour>().IconLocation[1].transform.position;

        Cursors[0].transform.rotation = CatalogButtons[0].GetComponent<ButtonBehaviour>().IconLocation[0].transform.rotation;
        Cursors[1].transform.rotation = CatalogButtons[0].GetComponent<ButtonBehaviour>().IconLocation[1].transform.rotation;

        //Setting dependencies
        StationDependencies[Interactables.EssenceStation] = 1;
        StationDependencies[Interactables.HugStation] = 0;
        StationDependencies[Interactables.Generator] = 0;
        StationDependencies[Interactables.TeslaStation] = 0;
        StationDependencies[Interactables.Crusher] = 0;
    }

    public void Update()
    {
        float step = CursorSpeed * Time.deltaTime; // calculate distance to move
        for (int p = 0; p < 2; p++)
        {
            if (CursorMove[p] == true)
            {
                Cursors[p].transform.position = Vector3.MoveTowards(Cursors[p].transform.position, CatalogButtons[buttonSelected[p]].GetComponent<ButtonBehaviour>().IconLocation[p].transform.position, step);
                if (Cursors[p].transform.position == CatalogButtons[buttonSelected[p]].GetComponent<ButtonBehaviour>().IconLocation[p].transform.position)
                {
                    CursorMove[p] = false;
                }
            }
        }

        TotalCoins.text = GameControllerScript.local.Coins.Count.ToString();

        if (GameControllerScript.local.Player1.gamePad)
        {
            switch (GameControllerScript.PSController[0])
            {
                case 0:
                    btnToClose.sprite = closeBtnB;
                    break;
                case 1:
                    btnToClose.sprite = closeBtnO;
                    break;
            }
        }
        else
        {
            btnToClose.sprite = closeBtnQ;
        }
    }

    public void ResetPrices()
    {
        foreach (ResourceType potion in AllPotions)
        {
            GameControllerScript.local.PotionPrice[potion] = Mathf.Abs(GameControllerScript.local.PotionPrice[potion]);
        }
    }

    public void AddDependency(Interactables station)
    {
        if (StationDependencies[station] == 0)
        {
            int gridCount = (station == Interactables.HugStation || station == Interactables.TeslaStation) ? 2 : 1;
            NewMachineOnGrid(station, gridCount);
        }

        StationDependencies[station]++;

        foreach (GameObject interactor in GameControllerScript.local.existingInteractors)
        {
            FarmInteractorScript farm = interactor.GetComponent<FarmInteractorScript>();

            if (farm != null) { farm.UpdateCurrentlyActive(); }
        }
    }

    public void RemoveDependency(Interactables station)
    {
        if (StationDependencies[station] > 0)
        {
            StationDependencies[station]--;
            if (StationDependencies[station] == 0)
            {
                List<GameObject> existingStations = GameControllerScript.local.existingInteractors;
                for (int i = existingStations.Count - 1; i >= 0; i--)
                {
                    InteractorScript interactorScript = existingStations[i].GetComponent<InteractorScript>();
                    Debug.Log("Name = " + interactorScript.name);
                    Debug.Log("interactorScript.MYTYPE = " + interactorScript.MYTYPE);
                    Debug.Log("station = " + station);
                    if (interactorScript.MYTYPE == station)
                    {
                        interactorScript.DestroyMe();
                        GameControllerScript.local.existingInteractors.RemoveAt(i);
                        break;
                    }
                    Debug.Log("...");
                }
            }
        }

        foreach (GameObject interactor in GameControllerScript.local.existingInteractors)
        {
            FarmInteractorScript farm = interactor.GetComponent<FarmInteractorScript>();

            if (farm != null) { farm.UpdateCurrentlyActive(); }
        }
    }

    public void NewMachineOnGrid(Interactables machineType, int numOfPoints)
    {
        GameControllerScript.GridPoint[] gridPoints = GameControllerScript.FindMeSpecificGridPoints(numOfPoints, machineType);

        Vector3 avgPos = Vector3.zero;

        for (int i = 0; i < numOfPoints; i++)
        {
            avgPos += gridPoints[i].worldPos;
            gridPoints[i].Occupied = true;
        }

        if (numOfPoints > 1) { avgPos /= numOfPoints; }

        GameControllerScript.SpawnInteractorDrop(machineType, avgPos, gridPoints);
    }
    
    /// <summary>
    /// If A button is pressed, new interactor is true, spawn new interactor object. False will delete the object
    /// </summary>
    /// <param name="NewInteractor">True to spawn a new interactor</param>
    public void MenuSelect(Transform SelectedButton, byte PlayNum)
    {
        int index = PlayNum - 1;
        float price = GameControllerScript.local.PotionPrice[PotionsList[buttonSelected[index]]];
        int roundedPrice = Mathf.RoundToInt(price);
        Button = SelectedButton.GetComponentInChildren<ButtonBehaviour>();
        //Get Potion Recipe from Button from selection.

        //If there are enough coins trigger whether to 
        if (price <= GameControllerScript.local.Coins.Count || Button.Unlocked == true)
        {
            PlayerSelect[index] = !(PlayerSelect[index]);
            FMOD_ControlScript.ChangeSelection(); // player select SFX
        }
        else
        {
            PlayerSelect[index] = false;
            FMOD_ControlScript.InvalidSelection(); // not enough cash sfx
        }

        Cursors[index].transform.localScale = (PlayerSelect[index]) ? CursorLargeScale : CursorOGScale;

        if ((PlayerSelect[0] == true && PlayerSelect[1] == true) && (buttonSelected[0] == buttonSelected[1]))
        {
            if (Button.Unlocked == false)
            {
                Debug.Log("price = " + price);
                
                if (GameControllerScript.MakePurchase(roundedPrice))
                {
                    ResourceType potionToAdd = PotionsList[buttonSelected[index]];
                    GameControllerScript.local.PotionPrice[potionToAdd] /= 2;
                    GameControllerScript.local.currentUnlockedPotion.Add(potionToAdd);
                    GameControllerScript.local.PotionsCatalog.Remove(potionToAdd);

                    Button.Unlocked = true;
                    Debug.Log(PotionsList[buttonSelected[index]] + " Added");
                    CatalogButtons[buttonSelected[index]].GetComponentInChildren<Text>().text = "+" + Mathf.RoundToInt(roundedPrice/2).ToString();
                    Button.Disabled.SetActive(true);
                    Button.Highlight.SetActive(true);
                    Button.ButtonText.GetComponent<Text>().text = "Refund";

                    switch (potionToAdd)
                    {
                        case ResourceType.HealthPotion:
                            {
                                AddDependency(Interactables.EssenceStation);
                                break;
                            }
                        case ResourceType.LovePotion:
                            {
                                AddDependency(Interactables.HugStation);
                                break;
                            }
                        case ResourceType.ManaPotion:
                            {
                                AddDependency(Interactables.TeslaStation);
                                AddDependency(Interactables.Generator);
                                break;
                            }
                        case ResourceType.PoisonPotion:
                            {
                                AddDependency(Interactables.Crusher);
                                break;
                            }
                    }

                    PlayerSelect[0] = false;
                    PlayerSelect[1] = false;

                    Cursors[0].transform.localScale = CursorOGScale;
                    Cursors[1].transform.localScale = CursorOGScale;

                    FMOD_ControlScript.PlaySoundOneShot(FMOD_ControlScript.coinSpawnEventPath); // buy sfx
                }
                else
                {
                    Debug.Log("Broke like a Uni Student. Soz not Soz"); //Lolz Stewart
                    FMOD_ControlScript.InvalidSelection(); // invalid choice sfx
                    PlayerSelect[0] = false;
                    PlayerSelect[1] = false;
                }
            }
            else
            {
                if (GameControllerScript.local.currentUnlockedPotion.Count > 1)
                {
                    Debug.Log("price = " + price);
                    if (GameControllerScript.MakePurchase(-roundedPrice))
                    {
                        ResourceType potionToAdd = PotionsList[buttonSelected[index]];
                        GameControllerScript.local.PotionPrice[potionToAdd] *= 2;
                        GameControllerScript.local.PotionsCatalog.Add(potionToAdd);
                        GameControllerScript.local.currentUnlockedPotion.Remove(potionToAdd);
                        CatalogButtons[buttonSelected[index]].GetComponentInChildren<Text>().text = Mathf.RoundToInt(roundedPrice*2).ToString();
                        Button.Disabled.SetActive(false);
                        Button.Highlight.SetActive(false);
                        Button.ButtonText.GetComponent<Text>().text = "Buy";

                        Button.Unlocked = false;

                        switch (potionToAdd)
                        {
                            case ResourceType.HealthPotion:
                                {
                                    RemoveDependency(Interactables.EssenceStation);
                                    break;
                                }
                            case ResourceType.LovePotion:
                                {
                                    RemoveDependency(Interactables.HugStation);
                                    break;
                                }
                            case ResourceType.ManaPotion:
                                {
                                    RemoveDependency(Interactables.TeslaStation);
                                    RemoveDependency(Interactables.Generator);
                                    break;
                                }
                            case ResourceType.PoisonPotion:
                                {
                                    RemoveDependency(Interactables.Crusher);
                                    break;
                                }
                        }
                    }
                }
                else
                {
                    PlayerSelect[0] = false;
                    PlayerSelect[1] = false;
                    Cursors[index].transform.localScale = CursorOGScale;

                }
            }
        }

    }

    //Menu Movement
    public void MenuMove(bool right, byte PlayNum)
    {
        int index = PlayNum - 1;
        PlayerSelect[index] = false;
        Cursors[index].transform.localScale = CursorOGScale;
        CursorMove[PlayNum - 1] = true;
        if (_haveSelectedMachine == false)
        {
            //0 = up, 1 = right, 2 = down, 3 = left
            if (right)
            {
                buttonSelected[index] = (buttonSelected[index] < CatalogButtons.Count - 1) ? (buttonSelected[index] + 1) : 0;
            }
            else
            {
                buttonSelected[index] = (buttonSelected[index] > 0) ? (buttonSelected[index] - 1) : (CatalogButtons.Count - 1);
            }

            FMOD_ControlScript.ChangeSelection();
        }
    }
    [System.Serializable]
    public class PotionStruct
    {
        public Sprite Enabled, Disabled;
    }
    [System.Serializable]
    public class PotionSpriteDict : SerializableDictionaryBase<ResourceType, PotionStruct> { }
}
