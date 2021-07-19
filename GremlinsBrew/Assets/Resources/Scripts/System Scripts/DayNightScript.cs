using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DayNightScript : MonoBehaviour
{
    public GameObject lightSource, lightSourceNight;
    private Light lightSrc, nightLightSrc;
    public CustomerControlScript Customers;
    private bool _isNight = false, _isDawn = false;
    public List<ResourceType> dawnFocus = new List<ResourceType>();
   // public ResourceType tutorialFocus;
    public float nightAmount = 0;

    public int currentDay = 0;

    //Day Night Cycle
    //public int currentDay;
    public float timeModSec;      //How fast a second is ingame
    public float dayLength;//, nightLength;   //How long each day & night are
    public float changeSpeed;

    //Clock Variables
    [SerializeField]
    private float _seconds = 0;

    private Quaternion lightPointStart, lightPointEnd;

    //private float totalLength;

    public Color dayCol, nightCol;

    public Color patienceColor0, patienceColor1;
    private float patienceF = 1, patienceFTarget = 1;

    //Lighting
    //private Color _DayTime = new Color(144, 127, 80, 255);
    //private Color _NightTime = new Color(170, 220, 100, 255);
    private float dayInt = 0.2f, nightInt = 0.8f; //Intensity of the light during the day & night respectively

    // Start is called before the first frame update
    void Start()
    {
        _seconds = 0;//dayLength / 2;
        //totalLength = dayLength + nightLength;

        lightSrc = lightSource.GetComponent<Light>();
        nightLightSrc = lightSourceNight.GetComponent<Light>();

        lightPointStart = Quaternion.Euler(140, 40, -25);
        lightPointEnd = Quaternion.Euler(175, -42, -25);

        RenderSettings.ambientSkyColor = dayCol;

        currentDay = 0;
    }

    // Update is called once per frame
    void Update()
    {
        DayCycle();
        PatienceColor();
    }

    /// <summary>
    /// This handles everything with the shadows and time
    /// </summary>
    private void DayCycle()
    {
        if (!GameControllerScript.Paused)
        {
            //Day time
            if (_seconds <= dayLength)
            {
                if (_isDawn == false)
                {
                    _seconds += Time.deltaTime * timeModSec;
                }

                if (nightAmount > 0)
                {
                    nightAmount = Mathf.MoveTowards(nightAmount, 0, Time.deltaTime * changeSpeed);

                    if (nightAmount == 0)
                    {
                        TriggerPotionTutorial();

                        _isNight = false;
                        nightLightSrc.intensity = 0;
                        RenderSettings.ambientIntensity = dayInt;
                        RenderSettings.ambientSkyColor = dayCol;
                        GameControllerScript.DayStarting();
                        currentDay++;

                    }
                    else
                    {
                        RenderSettings.ambientIntensity = Mathf.Lerp(dayInt, nightInt, nightAmount); //Lerp between the day and night colours
                        nightLightSrc.intensity = nightAmount;
                        RenderSettings.ambientSkyColor = Color.Lerp(dayCol, nightCol, nightAmount);
                    }
                }
            }

            //Night time
            else
            {
                if (nightAmount < 1)
                {
                    nightAmount = Mathf.MoveTowards(nightAmount, 1, Time.deltaTime * changeSpeed);

                    if (nightAmount == 1)
                    {
                        _isNight = true;
                        GameControllerScript.DayFinished();
                        lightSrc.intensity = 0;
                        RenderSettings.ambientIntensity = nightInt;
                        RenderSettings.ambientSkyColor = nightCol;

                        Customers.OverstockedPotions = GameControllerScript.OverstockedPotions();
                    }
                    else
                    {

                        RenderSettings.ambientIntensity = Mathf.Lerp(dayInt, nightInt, nightAmount); //Lerp between the day and night colours
                        lightSrc.intensity = (1 - nightAmount)*2;
                        RenderSettings.ambientSkyColor = Color.Lerp(dayCol, nightCol, nightAmount);
                    }
                }
            }
        }

        //Control light direction, regardless of whether its paused
        if (_isNight == false)
        {
            lightSource.transform.rotation = Quaternion.Slerp(lightPointStart, lightPointEnd, _seconds / dayLength);
            //lightSource.transform.forward = Vector3.MoveTowards(lightSource.transform.forward, newPoint - lightSource.transform.position, 0.1f);
        }

        //Control light intensity
        if (_seconds > dayLength)
        {
            if (lightSrc.intensity == 0)
            {
                nightLightSrc.intensity = Mathf.MoveTowards(nightLightSrc.intensity, 1, 0.01f);
            }
        }
        else
        {
            if (nightLightSrc.intensity == 0)
            {
                lightSrc.intensity = Mathf.MoveTowards(lightSrc.intensity, 2, 0.02f);
            }
        }
    }

    //Turns the game red when patience is dropping
    private List<float> tickPercents = new List<float>();
    private void PatienceColor()
    {
        if (patienceF != patienceFTarget)
        {
            patienceF = Mathf.MoveTowards(patienceF, patienceFTarget, 0.05f);

            lightSrc.color = Color.Lerp(patienceColor0, patienceColor1, patienceF);
        }
    }

    public void UpdatePatience(float patience)
    {
        patienceFTarget = patience;
    }

    private void TriggerPotionTutorial()
    {
        if(GameControllerScript.local.currentUnlockedPotion.Contains(ResourceType.PoisonPotion))
        {
            TutorialScript.NewTutorial(TutorialType.PoisonPotionUnlocked);
            Debug.Log("Poison Unlocked");
        }

        if (GameControllerScript.local.currentUnlockedPotion.Contains(ResourceType.LovePotion))
        {
            TutorialScript.NewTutorial(TutorialType.LovePotionUnlocked);
        }

        if (GameControllerScript.local.currentUnlockedPotion.Contains(ResourceType.ManaPotion))
        {
            TutorialScript.NewTutorial(TutorialType.ManaPotionUnlocked);
        }
    }

    public float Seconds
    {
        get
        {
            return _seconds;
        }
    }

    public void ResetToDay()
    {
        lightSource.transform.rotation = lightPointStart; //Move light source back to starting position
        _seconds = 0;
    }

    public bool IsNight
    {
        get { return _isNight;}
    }

    public bool IsDawn
    {
        get => _isDawn;
        set { _isDawn = value; }
    }

    public void SetDawnFocus(ResourceType[] tutorialFocus)
    {
        dawnFocus.Clear();
        foreach(ResourceType r in tutorialFocus)
        {
            dawnFocus.Add(r);
        }
        
    }
}
