using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using InControl;

public class QTE_Thumbstick : QTEScript
{
    public bool SndJoyStick = true;
    //AxesPoints
    private List<GameObject> axesTargets = new List<GameObject>();
    private bool axesEnabled1 = false, axesEnabled2 = false;
    private int currentTarget = 0, offset = 0;
    private float nextAxesCount = 0, x2 = 0, y2 = 0, VibrateValR = 0, VibrateValL = 0, ChargeTimer = 0, Chargespeed = 0, maxvalue = 1;
    private bool Stick1_Found, Stick2_Found;

    private float vibrate_start = 0.3f; //Distance 

    [SerializeField]
    private GameObject User; //Player1

    public GameObject playerPointerPrefab;

    //Override for any values before setup starts
    public override void SetupStarting(){ }

    //Override for any values after setup has finished -> ie, after all the axes targets have been added
    public override void SetupFinished()
    {
        //Axis Points init
        if (axesTargets.Count > 0)
        {
            nextAxesCount = 1.2f / axesTargets.Count; //1.2f = how many seconds it takes to show all qtes
            TIMER = nextAxesCount;
        }

        my_info._setup(GetComponent<RectTransform>(), 0, attachedPlayers.Count, pref_num); //Update the class that'll be passed to the interactor
    }

    //Receive Movement Input
    public override void MovementInput(PlayerScript player, Vector2 movement)
    {
        if (player.gamePad == true)
        {         //axis
            x2 = player.myInputDevice.RightStickX;
            y2 = player.myInputDevice.RightStickY;
        }
        else
        {
            x2 = (player.PLAYERNUM == 1) ? Input.GetAxis("Horizontal") : Input.GetAxis("Horizontal2");
            y2 = (player.PLAYERNUM == 1) ? Input.GetAxis("Vertical") : Input.GetAxis("Vertical2");
        }

        //Get position of 0 - Thumbstick direction, stretch Arrow Sprite?

        QTEPlayer qtePlayer = attachedPlayers[player];

        if (qtePlayer != null)
        {

            if (movement.magnitude > 1)
            {
                movement = movement.normalized;
            }

            //Move the player's sphere
            if (axesEnabled1)
            {
                Vector3 newPos = ((transform.position - new Vector3(offset, 0, 0)) + transform.right * movement.x);
                newPos += transform.up * movement.y;

                qtePlayer.representation.transform.position = newPos;
            }
            //Move the player's sphere
            if (axesEnabled2 && SndJoyStick == true)
            {
                Vector3 newPosR = ((transform.position + new Vector3(offset, 0, 0)) + transform.right * x2);
                newPosR += transform.up * y2;

                qtePlayer.representation2.transform.position = newPosR;
            }
        }
    }

    //Just the update method
    protected override void QteUpdate(int playerCount)
    {
        axesEnabled1 = true;
        axesEnabled2 = true;
        //Show each target in order
        if (currentTarget <= axesTargets.Count)
        {
            if (currentTarget < axesTargets.Count)
            {
                axesTargets[currentTarget].GetComponent<Renderer>().enabled = false;
            }
            currentTarget++;

            if (currentTarget > axesTargets.Count)
            {
                foreach (KeyValuePair<PlayerScript, QTEPlayer> qp in attachedPlayers)
                {
                    qp.Value.representation.GetComponent<Renderer>().enabled = true;
                    if (SndJoyStick == true)
                    {
                        qp.Value.representation2.GetComponent<Renderer>().enabled = true;
                    }

                    print("Vibrate controller accordingly");
                }
            }
        }
        else
        {
            //Handle the actual axes point QTE
            foreach (KeyValuePair<PlayerScript, QTEPlayer> qp in attachedPlayers)
            {
                Vector3 pos = qp.Value.representation.transform.position;
                Vector3 posR = qp.Value.representation2.transform.position;

                float dist = Mathf.Max(0, -Vector3.Distance(pos, axesTargets[0].transform.position) + vibrate_start);
                VibrateValL = dist / vibrate_start;

                Stick1_Found = (Vector3.Distance(pos, axesTargets[0].transform.position) < 0.3f) ? (Stick1_Found = true) : (Stick1_Found = false);

                //If second thumbstick is being used
                if (SndJoyStick == true)
                {
                    dist = Mathf.Max(0, -Vector3.Distance(posR, axesTargets[1].transform.position));
                    VibrateValR = dist / vibrate_start;

                    Stick2_Found = (Vector3.Distance(posR, axesTargets[1].transform.position) < 0.3f) ? (Stick2_Found = true) : (Stick2_Found = false);
                }
                else
                {
                    VibrateValR = 0;
                }

                User.GetComponent<PlayerScript>().myInputDevice.Vibrate(VibrateValL, VibrateValR);
            }
        }

        //timer to charge crystal
        if (ChargeTimer < maxvalue)
        {
            if (true)
            {
                //Timer
                if (SndJoyStick == false)
                {
                    if (Stick1_Found == true)
                    {
                        ChargeTimer += Chargespeed * Time.deltaTime;
                    }
                }
                else
                {
                    if (Stick1_Found == true && Stick2_Found == true)
                    {
                        ChargeTimer += Chargespeed * Time.deltaTime;
                    }
                }
            }
            else
            {
                User.GetComponent<PlayerScript>().myInputDevice.Vibrate(0, 0);
                print("No Power");
            }
        }
        else
        {
            ChargeTimer = 0;
            User.GetComponent<PlayerScript>().myInputDevice.Vibrate(0, 0);
            End(true);
        }
    }


    public override QTE_Info INFO()
    {
        float goal = 0; //Has it reached its goal?

        //goal =


        foreach (KeyValuePair<PlayerScript, QTEPlayer> qp in attachedPlayers)
        {
            Vector3 pos = qp.Value.representation.transform.position;
            Vector3 posR = qp.Value.representation2.transform.position;

            if (SndJoyStick == false)
            {
                goal += (1 - Vector3.Distance(pos, axesTargets[0].transform.position) / 0.3f) + 0.2f;
            }

            else if (SndJoyStick == true && Vector3.Distance(pos, axesTargets[0].transform.position) < 0.3f && Vector3.Distance(posR, axesTargets[1].transform.position) < 0.3f)
            {
                goal += (1 - Vector3.Distance(pos, axesTargets[0].transform.position) / 0.3f)/2 + 0.1f;
                goal += (1 - Vector3.Distance(posR, axesTargets[1].transform.position) / 0.3f)/2 + 0.1f;

            }
        }

        my_info._update(goal, attachedPlayers.Count); //Update the class that'll be passed to the interactor

        return my_info; //Return that class
    }

    //Add a new target point to hit
    public override void AddPointDual(Vector3 pointL,bool SecondStick, Vector3 pointR, Vector2 Values)
    {
        Chargespeed = Values.x;
        maxvalue = Values.y;

        SndJoyStick = SecondStick;
        if (SecondStick == true)
        {
            offset = 2;
        }
        GameObject axesTarget = Instantiate(axesPointTargetPrefab, transform.position - new Vector3(offset, 0, 0), Quaternion.identity);
        axesTarget.GetComponent<Renderer>().enabled = false;
        axesTargets.Add(axesTarget);

        pointL = pointL.normalized;

        Vector3 pos = axesTarget.transform.position;
        pos += transform.right * pointL.x;
        pos += transform.up * pointL.y;

        axesTarget.transform.position = pos;
        axesTarget.transform.parent = this.transform;

        if (SecondStick == true)
        {
            //Right Thumbstick Target
            GameObject axesTargetR = Instantiate(axesPointTargetPrefab, transform.position + new Vector3(offset, 0, 0), Quaternion.identity);
            axesTargetR.GetComponent<Renderer>().enabled = false;

            axesTargets.Add(axesTargetR);
            pointR = pointR.normalized;

            Vector3 posR = axesTargetR.transform.position;
            posR += transform.right * pointR.x;
            posR += transform.up * pointR.y;
            axesTargetR.transform.position = posR;
            axesTargetR.transform.parent = this.transform;
        }
    }

    //New player added, so add a text prompt above them
    protected override void PlayerAdded(PlayerScript player)
    {
        User = player.gameObject;
        GameObject sphere = Instantiate(playerPointerPrefab, transform.position, Quaternion.identity);
        GameObject sphere2 = Instantiate(playerPointerPrefab, transform.position, Quaternion.identity);

        QTEPlayer p = attachedPlayers[player];
        if (p.representation == null)
        {
            p.representation = sphere.GetComponent<RectTransform>();
            p.representation.transform.parent = this.transform;

            //Invisible at start of axis point QTE
            if (!axesEnabled1)
            {
                p.representation.GetComponent<Renderer>().enabled = false;
            }

        }
        if (p.representation2 == null)
        {
            p.representation2 = sphere2.GetComponent<RectTransform>();
            p.representation2.transform.parent = this.transform;

            if (!axesEnabled2)
            {
                p.representation2.GetComponent<Renderer>().enabled = false;
            }
        }
    }
}
