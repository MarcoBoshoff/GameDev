using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class QTE_RotateScript : QTEScript
{
    //Check how many rotations are required.
    private int Target = 1;
    private float progress = 0, rotate_speed = 0;
    private float previous_rotation = -1;
    private Vector2 prevs;

    private float rot_leeway = 40, prevValue = 0;
    private float dir = -1;

    public RectTransform showRotation;
    public Image rotationImage, LS, ingredientImg;
    public GameObject WASD, ArKeys;
    private bool fadeLS = true;

    //Override for any values before setup starts
    public override void SetupStarting(){ my_info._update(ingredientImg); doBubble = false; }

    //Override for any values after setup has finished -> ie, after all the variables have been assigned by the QTEPrefsScript
    public override void SetupFinished() { TutorialScript.Trigger(TutorialTrigger.CauldronQTEStarted); }

    //## If not needed, you can delete the above 2 methods

    //Receive Movement Input
    public override void MovementInput(PlayerScript player, Vector2 movement)
    {
        //Get position of 0 - Thumbstick direction, stretch Arrow Sprite?

        QTEPlayer qtePlayer = attachedPlayers[player];

        if (qtePlayer != null)
        {
            if (movement == Vector2.zero)
            {
                if (previous_rotation == -1)
                {
                    movement = Vector2.right;
                }
                else
                {
                    movement = prevs;
                }
            }
            movement = movement.normalized;

            //Move representation to that point
            Vector3 newPos = (transform.position + transform.right * movement.x);
            newPos += transform.up * movement.y;
            //newPos = Vector2.MoveTowards(qtePlayer.representation.transform.position, newPos, 0.2f);

            Vector3 rot = qtePlayer.representation.transform.eulerAngles;

            //Calculate new rotation amount
            float new_rotation = Vector2.Angle(Vector2.right, movement);
            if (movement.y < 0) { new_rotation = 360 - new_rotation; }

            //Previous rotation to work out turning position
            float newValue = 0;
            if (previous_rotation >= 0)
            {
                bool in_range = true;
                if (Mathf.Abs(previous_rotation - new_rotation) > rot_leeway)
                {
                    if (Mathf.Abs(previous_rotation - (new_rotation + 360)) > rot_leeway)
                    {
                        if (Mathf.Abs(previous_rotation - (new_rotation - 360)) > rot_leeway)
                        {
                            in_range = false;
                        }
                    }
                }

                if (in_range && new_rotation * dir > previous_rotation * dir)
                {
                    //newValue normalized between 0 & 1
                    newValue = Mathf.Min(0.6f, Mathf.Abs(previous_rotation - new_rotation) / rot_leeway);
                    //progress += 1;
                    //Debug.Log(progress);
                }
            }

            prevValue = (newValue + prevValue) / 2;
            progress += (newValue==0)?(prevValue*3) : (newValue * 3);
            newValue = prevValue;

            if (newValue > 0)
            {
                rotate_speed = Mathf.MoveTowards(rotate_speed, newValue*70, 0.15f);
            }
            //rotate_speed = Mathf.MoveTowards(rotate_speed, 0, 0.01f);
           
            rot.z -= rotate_speed; //rotate the indicator
            qtePlayer.representation.transform.eulerAngles = rot;

            Color c = LS.color;
            if (fadeLS)
            {
                c.a = (Target - progress) / ((float)Target);
            }
            else
            {
                c.a = 0;
            }
            LS.color = c;

            previous_rotation = new_rotation;
            prevs = movement;
        }

        // prevValue contains speed of rotation, for controlling volume of stiring
    }

    public override void StopEarly()
    {
        PlayerScript[] players = new PlayerScript[attachedPlayers.Count];
        int i = 0;
        foreach (KeyValuePair<PlayerScript, QTEPlayer> pq in attachedPlayers)
        {
            players[i] = pq.Key;
            i++;

            GiveWand wandControl = pq.Key.GetComponent<GiveWand>();
            if (wandControl != null)
            {
                wandControl.StirCauldron(false);
            }
        }
        myInteractor.StopRequest(pref_num, players);
        Destroy(gameObject);
    }

    public override void SetFloat(float f)
    {
        dir = f;
    }

    public override void SetInt(int i)
    {
        Target = i;
    }

    //Just the update method
    protected override void QteUpdate(int playerCount)
    {
        if (progress > Target)
        {
            End(true); //## Calls the success method of the interactor (because 'true' was passed)
        }        
    }

    //New player added, so add a text prompt above them
    protected override void PlayerAdded(PlayerScript player)
    {
        QTEPlayer p = attachedPlayers[player];
        if (p.representation == null)
        {
            p.representation = showRotation;
            //p.representation.transform.parent = this.transform;

            rotationImage.color = player.myColor;

            if (!player.gamePad)
            {
                fadeLS = false;
                if (player.PLAYERNUM == 1) { WASD.SetActive(true); }
                else if (player.PLAYERNUM == 2) { ArKeys.SetActive(true); }
            }
        }

        GiveWand wandControl = player.GetComponent<GiveWand>();
        Debug.Log("wandControl = " + wandControl);
        if (wandControl != null)
        {
            wandControl.StirCauldron(true);
        }
    }

}
