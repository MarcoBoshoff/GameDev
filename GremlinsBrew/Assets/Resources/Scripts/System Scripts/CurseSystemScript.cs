using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class CurseSystemScript : MonoBehaviour
{
    GameControllerScript control;
    Debuff[] curseArray;

    public Debuff currentDebuff = Debuff.none;

    public GameObject cursePopup;
    public Text curseText;
    public float popupTimer = 0;

    private float debuffTimer = 0;
    private int counter = 0;

    private int thisDebuffIndex = -1, previousDebuffIndex = -1;

    // Start is called before the first frame update
    public void Init(GameControllerScript c)
    {
        control = c;
        curseArray = (Debuff[])Enum.GetValues(typeof(Debuff));
    }

    public void NewDebuff()
    {
        while (thisDebuffIndex == previousDebuffIndex)
        {
            thisDebuffIndex = Random.Range(0, curseArray.Length);
        }

        currentDebuff = curseArray[thisDebuffIndex];

        //currentDebuff = Debuff.restricted_resources; //DEBUG
        switch (currentDebuff)
        {
            case Debuff.reversed_controls:
                control.Player1.speedModifier *= -1;
                control.Player2.speedModifier *= -1;
                break;

            case Debuff.restricted_resources:
                int r = Random.Range(0, 2);

                ResourceType t = (r == 0 ? ResourceType.Flower : ResourceType.Mushroom);
                control.Player1.restrictedResources.Add(t);
                control.Player1.SetName("No " + t.ToString());

                t = (r == 1 ? ResourceType.Flower : ResourceType.Mushroom);
                control.Player2.restrictedResources.Add(t);
                control.Player2.SetName("No " + t.ToString());
                break;

        }

        curseText.text = currentDebuff.ToString();
        cursePopup.SetActive(true);
        popupTimer = 4f;

        previousDebuffIndex = thisDebuffIndex;
    }

    public void Reset()
    {
        switch (currentDebuff)
        {
            case Debuff.reversed_controls:
                control.Player1.speedModifier = Mathf.Abs(control.Player1.speedModifier);
                control.Player2.speedModifier = Mathf.Abs(control.Player2.speedModifier);
                break;

            case Debuff.restricted_resources:
                control.Player1.restrictedResources.Clear();
                control.Player1.SetColourAndCostume(control, 0);
                control.Player2.restrictedResources.Clear();
                control.Player2.SetColourAndCostume(control, 1);
                break;

        }

        counter = 0;
        debuffTimer = 0;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (popupTimer > 0)
        {
            popupTimer = Mathf.MoveTowards(popupTimer, 0, Time.deltaTime);
            if (popupTimer == 0) { cursePopup.SetActive(false); }
        }

        if (GameControllerScript.NightProgress == 0)
        {
            Vector3 pos1 = control.Player1.transform.position, pos2 = control.Player2.transform.position;
            switch (currentDebuff)
            {
                case Debuff.magnetic:
                    

                    float dist = Vector3.Distance(pos1, pos2);
                    if (dist > 2) {

                        float strength = Mathf.Min(0.1f, (dist-2) * Time.fixedDeltaTime); //Further apart they are, the stronger the pull

                        control.Player1.transform.position = Vector3.MoveTowards(pos1, pos2, strength);
                        control.Player2.transform.position = Vector3.MoveTowards(pos2, pos1, strength);
                    }
                    break;

                case Debuff.swapping_bodies:
                    debuffTimer -= Time.fixedDeltaTime;
                    if (debuffTimer <= 0)
                    {
                        control.Player1.transform.position = pos2;
                        control.Player2.transform.position = pos1;

                        if (counter == 0)
                        {
                            control.Player1.SetColourAndCostume(control, 1);
                            control.Player2.SetColourAndCostume(control, 0);
                            counter = 1;
                        }
                        else
                        {
                            control.Player1.SetColourAndCostume(control, 0);
                            control.Player2.SetColourAndCostume(control, 1);
                            counter = 0;
                        }

                        debuffTimer = 20;
                    }

                    control.Player1.SetName(((int)debuffTimer).ToString());
                    control.Player2.SetName(((int)debuffTimer).ToString());
                    break;
            }
        }
    }
}

public enum Debuff
{
    none,
    reversed_controls,
    swapping_bodies,
    //still_and_dumb,
    magnetic,
    //seven_seconds_explode,
    //teleporting_cauldron,
    restricted_resources
}
