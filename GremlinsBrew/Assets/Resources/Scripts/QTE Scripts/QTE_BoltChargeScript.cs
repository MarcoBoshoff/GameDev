using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class QTE_BoltChargeScript : QTEScript
{
    private float start = 0.5f, colorTimer = 0f;

    private int _SUCCESSES = 0;
    private float CHARGE = 0;
    private float direction;

    private bool slowing = true;
    private float charging_speed = 0.05f, charging_target = 0.02f;

    private float scale = 3;

    private const float spriteScale = 3;

    private float sweetSpotMin, sweetSpotMax, sweetSpotSize;

    [SerializeField]
    private RectTransform myRect, sweetSpotRect;

    [SerializeField]
    private Image sweetSpot, ImgsweetSpot;

    public GameObject playerChargeIndicatorPrefab;

    public override void SetupStarting()
    {
        scale = Camera.main.pixelHeight / 5;
    }

    public override void SetupFinished()
    {
        RecalculateValues();

        my_info._setup(GetComponent<RectTransform>(), 0, attachedPlayers.Count, pref_num); //Update the class that'll be passed to the interactor
    }

    private void RecalculateValues()
    {
        CHARGE = Random.Range(0, 1);

        direction = Random.Range(0, 1) * 2 - 1;
        slowing = true;
        //charging_speed = 0.05f;
        charging_speed = Random.Range(0.025f, 0.035f);
        charging_target = 0.02f;        
    }

    // Update is called once per frame    
    protected override void QteUpdate(int playerCount)
    {
        //Change charge
        CHARGE += charging_speed * direction;
        if (CHARGE <= 0) { direction = 1; }
        if (CHARGE >= 1) { direction = -1; }
        CHARGE = Mathf.Clamp01(CHARGE);

        //Change charging speed
        charging_speed = Mathf.MoveTowards(charging_speed, charging_target, 0.0002f);
        if (charging_speed == charging_target)
        {
            slowing = !slowing;
            charging_target = (slowing)?0.02f:0.04f;
        }

        foreach (KeyValuePair<PlayerScript, QTEPlayer> p in attachedPlayers)
        {
            QTEPlayer qp = p.Value;

            if (qp.representation != null)
            {
                Vector3 indicatorPos = myRect.position;
                indicatorPos += CHARGE * Vector3.up * scale;

                qp.representation.transform.position = indicatorPos;
            }
        }

        Vector3 pos = myRect.position;
        Vector3 target = pos + ((sweetSpotMin+sweetSpotMax)/2 + 0.1f) * Vector3.up * scale;

        pos = Vector3.MoveTowards(sweetSpotRect.position, target, 2.8f);
        sweetSpotRect.position = pos;

        start = Mathf.MoveTowards(start, 0, Time.deltaTime);

        if (colorTimer > 0)
        {
            colorTimer = Mathf.MoveTowards(colorTimer, 0, Time.fixedDeltaTime);
            if (colorTimer == 0) { ImgsweetSpot.color = Color.white; }
        }
    }

    public override void Interrupt(int id, int i)
    {
        if (id == 0)
        {
            End(true);
        }
    }

    public override QTE_Info INFO()
    {
        float goal = _SUCCESSES;
        _SUCCESSES = 0;

        my_info._update(goal, attachedPlayers.Count); //Update the class that'll be passed to the interactor
        
        return my_info; //Return that class
    }

    public override void ButtonPressed(PlayerScript player, GamepadEnum btn)
    {
        if (start == 0)
        {
            if (btn == GamepadEnum.A)
            {
                Debug.Log("hit at: " + CHARGE);
                Debug.Log("needed between " + sweetSpotMin + " & " + sweetSpotMax);

                if (CHARGE >= sweetSpotMin && CHARGE <= sweetSpotMax)
                {
                    colorTimer = 0.8f;
                    ImgsweetSpot.color = Color.yellow;
                    _SUCCESSES += 1;
                }
                else
                {
                    colorTimer = 0.6f;
                    ImgsweetSpot.color = Color.red;
                }

                sweetSpotMin = Random.Range(0f, 1f - sweetSpotSize);
                sweetSpotMax = sweetSpotMin + sweetSpotSize;


                RecalculateValues();
            }
        }
    }

    public override void SetFloat(float size)
    {
        sweetSpotSize = size;

        sweetSpotMin = Random.Range(0f, 1f - sweetSpotSize);
        sweetSpotMax = sweetSpotMin + sweetSpotSize;

        sweetSpotRect.position = myRect.position + ((sweetSpotMin + sweetSpotMax) / 2) * Vector3.up * scale;

        ImgsweetSpot.color = Color.white;
    }

    protected override void PlayerAdded(PlayerScript player)
    {
        QTEPlayer p = attachedPlayers[player];
        if (p.representation == null)
        {
            GameObject pointer = Instantiate(playerChargeIndicatorPrefab);
            p.representation = pointer.GetComponent<RectTransform>();
            p.representation.transform.parent = this.transform;
            p.representation.transform.forward = transform.forward;
            pointer.GetComponent<Image>().color = player.myColor;
        }
    }
 }
