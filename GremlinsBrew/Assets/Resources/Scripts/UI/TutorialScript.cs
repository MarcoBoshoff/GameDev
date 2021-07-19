using RotaryHeart.Lib.SerializableDictionary;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TutorialScript : MonoBehaviour
{
    public static TutorialScript localTute; //local reference
    public static bool expire = false;
    public GameObject tutorialWindow; //pop up of the tutorial
    //public Image myImage;
    public GameObject textDisplay;
    public Image spriteDisplay;
    //public Sprite defaultSprite;

    private DayNightScript dayNightScript;

    public float TutorialHeight = 0, BDelay = 0;
    
    public bool show = true;
    public TutorialTrigger currentTrigger = TutorialTrigger.PressAnyKey;
    public TutorialEvent OnExit = TutorialEvent.None;

    private static GameObject spawnSmoke;

    private static Interactables interactorHighlight = Interactables.None;
    private static ResourceType resourceHighlight = ResourceType.Empty;

    private static GameObject skipSign;

    public RectTransform rt;

    //Stores all text to feed into the tutorial prompt
    public TutorialDictionary TUTORIAL = new TutorialDictionary();
    public Dictionary<TutorialType, bool> triggerDict = new Dictionary<TutorialType, bool>();

    public List<TutorialText> tutorialStack = new List<TutorialText>(); //Stack of all loaded tutorial prompts that are active

    public ResourceType highlightingResource = ResourceType.Empty;

    // Start is called before the first frame update
    void Start()
    {
        TutorialScript.localTute = this;
        TutorialScript.localTute.tutorialWindow = this.gameObject;
        rt = this.GetComponent<RectTransform>();

        Vector3 pos = rt.transform.position;
        pos.y = -400;
        rt.position = pos;

        TutorialType[] typesArray = (TutorialType[])Enum.GetValues(typeof(TutorialType));
        foreach (TutorialType t in typesArray)
        {
            triggerDict[t] = false; //Sets all tutorials triggers to false
        }

        spawnSmoke = Resources.Load("Prefabs/Particles/SmokeKabooshParticle") as GameObject;
    }

    // Update is called once per frame
    readonly float scrollSpeed = 14f;
    void FixedUpdate()
    {
        if (expire) { Expire(); }

        Vector3 pos = rt.position;

        if (show && pos.y < TutorialHeight)
        {
            pos.y = Mathf.MoveTowards(pos.y, TutorialHeight, scrollSpeed);
            rt.position = pos;
        }
        else if (!show && pos.y > -400)
        {
            pos.y = Mathf.MoveTowards(pos.y, -400, scrollSpeed);
            rt.position = pos;
        }

        if (BDelay > 0)
        {
            BDelay -= Time.deltaTime;
        }
    }

    public static void NewTutorial(TutorialType t)
    {
        if (!localTute.triggerDict[t])
        {
            bool emptyStack = (localTute.tutorialStack.Count == 0);

            if (localTute.TUTORIAL.ContainsKey(t) == false) { Debug.Log($"Key '{t.ToString()}' not in tutorial dictionary"); return; }
            TutorialText[] texts = localTute.TUTORIAL[t].TutorialElements;

            foreach (TutorialText text in texts)
            {
                localTute.tutorialStack.Add(text);
            }

            localTute.triggerDict[t] = true;

            localTute.show = true;

            if (emptyStack)
            {
                PopTutorial();
            }
        }

        if (localTute.textDisplay != null)
        {
            Destroy(localTute.textDisplay);
        }
    }

    public static void Trigger(TutorialTrigger trigger)
    {
        //Debug.Log($"Triggered: {trigger.ToString()}");
        if (localTute.show && localTute.currentTrigger == trigger)
        {
            if (localTute.tutorialStack.Count > 0)
            {
                PopTutorial();

            }
            else
            {
                //Any exit events
                if (localTute.OnExit != TutorialEvent.None)
                {
                    CallEvent(localTute.OnExit);
                }

                //Unhighlight any objects
                Highlight(false);

                //Close tutorial view
                localTute.show = false;
            }
        }
    }

    public static void PressedAnyKey()
    {
        if (localTute.show && localTute.currentTrigger == TutorialTrigger.PressAnyKey && localTute.BDelay <= 0)
        {
            if (localTute.tutorialStack.Count > 0)
            {
                PopTutorial();
                localTute.BDelay = 0.8f;
            }
            else
            {
                //Any exit events
                if (localTute.OnExit != TutorialEvent.None)
                {
                    CallEvent(localTute.OnExit);
                }
                
                //Unhighlight any objects
                Highlight(false);
                
                //Close tutorial view
                localTute.show = false;
            }
        }
    }

    private static void PopTutorial()
    {
        //Any exit events
        if (localTute.OnExit != TutorialEvent.None)
        {
            CallEvent(localTute.OnExit);
        }

        //Unhighlight any objects
        Highlight(false);

        TutorialText newText = localTute.tutorialStack[0];
        localTute.tutorialStack.RemoveAt(0);

        //Set the trigger
        localTute.currentTrigger = newText.trigger;

        localTute.spriteDisplay.sprite = newText.sprite;

        //Set exit event
        localTute.OnExit = newText.eventCalls.OnExit;

        interactorHighlight = newText.highlights.interactorToHighlight;
        resourceHighlight = newText.highlights.typeToHighlight;

        //Highlight any objects
        Highlight(true);

        TutorialEvent OnStart = newText.eventCalls.OnStart;
        if (OnStart != TutorialEvent.None)
        {
            CallEvent(OnStart);
        }
    }

    private static void Highlight(bool b)
    {
        //TODO: Glow/highlight
        //property name is: _glow_intensity

        //Interactors
        if (interactorHighlight != Interactables.None)
        {
            //Debug.Log("Highlighting something");

            GameObject[] objs = GameObject.FindGameObjectsWithTag("Interactable");

            foreach (GameObject obj in objs)
            {
                HighlightScript highlightComponent = obj.GetComponent<HighlightScript>();
                if (highlightComponent != null)
                {
                    if (obj.GetComponent<InteractorScript>().MYTYPE == interactorHighlight && b)
                    {
                        //Debug.Log(obj.name + " highlighted!");
                        highlightComponent.EnableGlow();
                    }
                    else if (highlightComponent != null)
                    {
                        highlightComponent.DisableGlow();
                    }
                }
            }
        }

        //Items
        localTute.highlightingResource = resourceHighlight;        
        if (resourceHighlight != ResourceType.Empty)
        {
            GameObject[] objs = GameObject.FindGameObjectsWithTag("Pickup");

            foreach (GameObject obj in objs)
            {
                HighlightScript highlightComponent = obj.GetComponent<HighlightScript>();
                if (obj.GetComponent<ItemScript>().resourceCompound.resourceType == resourceHighlight && b)
                {
                    highlightComponent.EnableGlow();
                }
                else if (highlightComponent != null)
                {
                    highlightComponent.DisableGlow();
                }
            }
        }
    }

    private static void CallEvent(TutorialEvent e)
    {
        switch (e)
        {
            case TutorialEvent.DamageMachine:
                GameControllerScript.Damage();
                break;
            case TutorialEvent.DayStop:
                GameControllerScript.TimeMod(0);
                break;
            case TutorialEvent.DayResume:
                GameControllerScript.TimeMod(1);
                break;
            case TutorialEvent.EnabledBed:
                SetBedActive(true);
                GameControllerScript.local.CanRepair = true;
                break;
            case TutorialEvent.DisableBed:
                SetBedActive(false);
                GameControllerScript.local.CanRepair = false;
                break;
            case TutorialEvent.EnableRepairing:
                GameControllerScript.local.CanRepair = true;
                break;
            case TutorialEvent.ResetHP:
                GameControllerScript.local.LIVES += 1;
                break;
            case TutorialEvent.DawnStartPoison:
                GameControllerScript.DayNight.IsDawn = true;
                GameControllerScript.DayNight.SetDawnFocus(new ResourceType[]{ ResourceType.PoisonPotion, ResourceType.Mushroom });
                break;
            case TutorialEvent.DawnStartLove:
                GameControllerScript.DayNight.IsDawn = true;
                GameControllerScript.DayNight.SetDawnFocus(new ResourceType[] { ResourceType.LovePotion, ResourceType.Lovefruit });
                break;
            case TutorialEvent.DawnStartMana:
                GameControllerScript.DayNight.IsDawn = true;
                GameControllerScript.DayNight.SetDawnFocus(new ResourceType[] { ResourceType.ManaPotion, ResourceType.PwCrystal });
                break;
            case TutorialEvent.DawnEnd:
                GameControllerScript.DayNight.IsDawn = false;
                break;
        }
    }

    private void Expire()
    {
        Debug.Log("Expiring tutorial");
        TutorialType[] typesArray = (TutorialType[])Enum.GetValues(typeof(TutorialType));
        foreach (TutorialType t in typesArray)
        {
            if (triggerDict.ContainsKey(t))
            {
                triggerDict[t] = true; //Sets all tutorials triggers to false
            }
        }

        show = false;
        expire = false;
    }

    public static void SetBedActive(bool b)
    {
        GameControllerScript.local.bedScript1.enabled = b;
        GameControllerScript.local.bedScript2.enabled = b;
    }
}

[System.Serializable]
public enum TutorialType
{
    Day, Night, PoisonPotionUnlocked, LovePotionUnlocked, ManaPotionUnlocked, DamageTaken, NeedsRepair
}

[System.Serializable]
public enum TutorialTrigger
{
    PressAnyKey,
    HealthPotionSent, PoisonPotionSent, LovePotionSent, ManaPotionSent,
    FlowerPickup, MushroomPickup, LovePickup, CrystalPickup,
    EssenceSuccessFlower, EssenceSuccessMushroom,
    CauldronHealthMade, CauldronPoisonMade, CauldronLoveMade, CauldronManaMade,
    CauldronMoved,
    Repairing, WentToBed,
    EssenseQTEStarted, CauldronQTEStarted, HugQTEStarted, CatcherQTEStarted,
    HugSuccess, TelsaSuccess,
    CrystalOnTesla,
    OpenBuyMenu,
    CookedQTEStarted,
    CookSuccess, CrushSuccess

    //TODO: Reorder these one day when you feel like redoing the tutorial triggers in the inspector... kinda done I guess?
}

[System.Serializable]
public enum TutorialEvent
{
    None, 
    DayStop, DayResume, DawnEnd,
    ResetHP, EnableRepairing, DamageMachine,
    EnabledBed, DisableBed,
    DawnStartPoison, DawnStartLove, DawnStartMana
}


[System.Serializable]
public class TutorialText
{
    public string debugTitle;
    public Sprite sprite;

    //public Sprite spriteToShow;
    public TutorialTrigger trigger = TutorialTrigger.PressAnyKey;
    public TutorialHighlight highlights;
    public TutorialEventcall eventCalls;
}

[System.Serializable]
public class TutorialHighlight
{
    public ResourceType typeToHighlight = ResourceType.Empty;
    public Interactables interactorToHighlight = Interactables.None;
}

[System.Serializable]
public class TutorialEventcall
{
    public TutorialEvent OnStart = TutorialEvent.None;
    public TutorialEvent OnExit = TutorialEvent.None;
}

[System.Serializable]
public class TTContainer
{
    public TutorialText[] TutorialElements;
}

[System.Serializable]
public class TutorialDictionary : SerializableDictionaryBase<TutorialType, TTContainer> { }


// Sarah = Just your average creative lead, lead artist, lead designer, and world's leading milk connoisseur