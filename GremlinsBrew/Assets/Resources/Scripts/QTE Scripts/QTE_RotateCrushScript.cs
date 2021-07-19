using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using FMOD.Studio;

public class QTE_RotateCrushScript : QTEScript
{
    //Check how many rotations are required.
    private int Target = 1;
    private float progress = 0, rotate_speed = 0;
    [SerializeField]
    private Image buttonPrompt;
    private float previous_rotation = -1;
    private Vector2 prevs;

    private Vector3 finalMushroomPos;
    private float scale = 1;

    private float rot_leeway = 40, prevValue = 0;
    private float dir = -1;

    private float mushroomHeight = 1.5f;
    public RectTransform myRect, mushroomIcon;

    //public Image pressPrompt;

    public RectTransform[] Gears = new RectTransform[2];

    [FMODUnity.EventRef]
    public string grindingEventPath;
    private EventInstance grindingAudio;

    //Override for any values before setup starts

    public override void SetupStarting()
    {
        offsetMultiplier = 140;
        scale = Camera.main.pixelHeight / 5;
        grindingAudio = FMODUnity.RuntimeManager.CreateInstance(grindingEventPath);
    }


    ////Override for any values after setup has finished -> ie, after all the variables have been assigned by the QTEPrefsScript
    public override void SetupFinished() {
        
    }

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
                    newValue = Mathf.Min(0.6f, Mathf.Abs(previous_rotation - new_rotation) / rot_leeway);
                }
            }

            prevValue = (newValue + prevValue) / 2;

            if (mushroomHeight == 0)
            {
                progress += (newValue == 0) ? (prevValue * 3) : (newValue * 3);
            }
            //progress += (newValue == 0) ? (prevValue * 3) : (newValue * 3);
            newValue = prevValue;

            if (newValue > 0)
            {
                rotate_speed = Mathf.MoveTowards(rotate_speed, newValue * 70, 0.15f);

                
            }

            if (rotate_speed > 0.2f)
            {
                // Only triggers audio loop once
                if (!FMOD_ControlScript.PlaybackState(grindingAudio))
                {
                    grindingAudio.start();
                }
            }

            else
            {
                if (FMOD_ControlScript.PlaybackState(grindingAudio))
                {
                    FMOD_ControlScript.StopAudio(grindingAudio);
                }
            }

            Gears[0].eulerAngles += new Vector3(0,0,rotate_speed);
            Gears[1].eulerAngles -= new Vector3(0, 0, rotate_speed);

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
        }
        myInteractor.StopRequest(pref_num, players);
        FMOD_ControlScript.StopAudio(grindingAudio);

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
        finalMushroomPos = myRect.position + (Vector3.up * scale);

        foreach (KeyValuePair<PlayerScript, QTEPlayer> p in attachedPlayers)
        {
            //mushroomHeight = Mathf.MoveTowards(mushroomHeight, (p.Value.fireButtonDown[0]) ? 0 : 1, 0.15f);
            mushroomHeight = 0; //For now, mushroom just moves down
            mushroomIcon.transform.position = Vector3.MoveTowards(finalMushroomPos - Vector3.up*(progress/Target)*scale/3, myRect.position + (Vector3.up * 5f * scale), mushroomHeight);

            //Color c = xPrompt.color;
            //c.a = 1 - progress/Target;
            //xPrompt.color = c;            
        }

        if (progress > Target)
        {
            End(true); //## Calls the success method of the interactor (because 'true' was passed)

            if (FMOD_ControlScript.PlaybackState(grindingAudio))
            {
                FMOD_ControlScript.StopAudio(grindingAudio);
            }
        }        
    }

    //New player added, so add a text prompt above them
    protected override void PlayerAdded(PlayerScript player)
    {
        buttonPrompt.GetComponent<Image>().sprite = GameControllerScript.BtnPromptSprite(player, BtnPromptEnum.InteractBtn);
        QTEPlayer p = attachedPlayers[player];
    }

}
