using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using FMOD.Studio;

public class QTE_CookScript : QTEScript
{
    private float yPos = 0,  mushroomDir = 1;
    //private float direction;

    float cooked = 0;

    private float scale = 1;

    private const float spriteScale = 3;

    private float sweetPoint, _size;

    public Image mushroomSR;
    public RectTransform myRect, stillIcon, movingIcon, redTemp;

    private Text holdText;
    [SerializeField]
    private Image buttonPrompt;

    [FMODUnity.EventRef]
    public string fireEventPath;
    private EventInstance fireAudio;

    public override void SetupStarting()
    {
        scale = Camera.main.pixelHeight / 5;
    }

    private void Awake()
    {
        fireAudio = FMODUnity.RuntimeManager.CreateInstance(fireEventPath);
    }

    //Override for any values after setup has finished -> ie, after all the variables have been assigned by the QTEPrefsScript
    public override void SetupFinished() { TutorialScript.Trigger(TutorialTrigger.CookedQTEStarted); }

    // Update is called once per frame    
    protected override void QteUpdate(int playerCount)
    {
        foreach (KeyValuePair<PlayerScript, QTEPlayer> p in attachedPlayers)
        {
            bool moveUp = upPressed || (p.Value.fireButtonDown[0]);

            yPos = Mathf.MoveTowards(yPos, moveUp ? 1 : 0, 0.009f);
            p.Value.representation.position = myRect.position + ((yPos-0.5f) * Vector3.up * scale);

            if (Mathf.Abs(yPos - sweetPoint) <= _size) {
                cooked = Mathf.MoveTowards(cooked, 1, 0.015f);

                redTemp.localScale = new Vector3(5, cooked*5f, 5);

                Color c = mushroomSR.color;
                c.g = 1-cooked;
                c.b = 1 - cooked;
                mushroomSR.color = c;

                if (!FMOD_ControlScript.PlaybackState(fireAudio))   
                {
                    fireAudio.start();
                }

                if (cooked >= 1)
                {
                    if (FMOD_ControlScript.PlaybackState(fireAudio))
                    {
                        FMOD_ControlScript.StopAudio(fireAudio);
                    }
                    End(true);
                }
            }

            else
            {
                if (FMOD_ControlScript.PlaybackState(fireAudio))
                {
                    FMOD_ControlScript.StopAudio(fireAudio);
                }
            }
        }

        upPressed = false;

        sweetPoint += (mushroomDir * 0.01f);
        sweetPoint = Mathf.Clamp(sweetPoint, 0.3f, 1);
        if (sweetPoint == 0.3f) { mushroomDir = 1f; }
        else if (sweetPoint == 1) { mushroomDir = -1f; }

        Vector3 pos = myRect.position + ((sweetPoint-0.3f) * Vector3.up * scale);
        stillIcon.position = pos;
    }

    private bool upPressed = false;
    public override void MovementInput(PlayerScript player, Vector2 movement)
    {
        if (movement.y > 0) { upPressed = true; }
    }

    public override void StopEarly()
    {
        if (FMOD_ControlScript.PlaybackState(fireAudio))
        {
            FMOD_ControlScript.StopAudio(fireAudio);
        }

        End(false);
    }
    public override void Interrupt(int id, int i)
    {
        if (id == 0)
        {
            if (FMOD_ControlScript.PlaybackState(fireAudio))
            {
                FMOD_ControlScript.StopAudio(fireAudio);
            }
            End(true);
        }
    }

    public override void SetFloat(float size)
    {
        _size = size;

        sweetPoint = Random.Range(0.3f, 1 - _size);

        Vector3 pos = myRect.position + (sweetPoint * Vector3.up * scale) ;
        stillIcon.position = pos;
        stillIcon.forward = transform.forward;

        //sweetspotObj.transform.GetChild(0).GetComponent<SpriteRenderer>().sortingOrder = 1; //Draw on top of other bar
    }

    protected override void PlayerAdded(PlayerScript player)
    {
        buttonPrompt.GetComponent<Image>().sprite = GameControllerScript.BtnPromptSprite(player, BtnPromptEnum.InteractBtn);

        QTEPlayer p = attachedPlayers[player];
        if (p.representation == null)
        {
            //GameObject pointer = Instantiate(playerChargeIndicator, transform.position, Quaternion.identity);
            p.representation = movingIcon.GetComponent<RectTransform>();
            p.representation.transform.parent = this.transform;
            p.representation.transform.forward = transform.forward;

            //mushroomIcon.GetComponent<SpriteRenderer>().sortingOrder = 1;
            //pointer.transform.GetChild(0).GetComponent<Renderer>().material.color = player.myColor;
        }
    }
}
