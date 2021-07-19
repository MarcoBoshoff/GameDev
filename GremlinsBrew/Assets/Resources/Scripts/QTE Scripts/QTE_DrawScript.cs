using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class QTE_DrawScript : QTEScript
{
    private TracerScript tracer;
    [SerializeField]
    private TracerControlScript tracerControl;

    //public RectTransform rect;
    public GameObject DrawPlayerPrefab;

    public override void SetInt(int i)
    {
        tracer = tracerControl.GetATracer();
    }

    public override void SetupFinished()
    {
        offsetAddition = Vector3.down*60;
        tracer.SetCurrent();
        TutorialScript.Trigger(TutorialTrigger.EssenseQTEStarted);
    }

    public override void StopEarly()
    {
        foreach (KeyValuePair<PlayerScript, QTEPlayer> pq in attachedPlayers)
        {
            GiveWand wandControl = pq.Key.GetComponent<GiveWand>();
            if (wandControl != null)
            {
                wandControl.CastingSpell(false);
            }
        }

        End(false);
    }

    public override void MovementInput(PlayerScript player, Vector2 movement)
    {
        QTEPlayer qtePlayer = attachedPlayers[player];

        if (movement != Vector2.zero && qtePlayer != null)
        {
            tracer.AddMovement(movement.normalized);
        }
    }

    protected override void QteUpdate(int playerCount)
    {
        foreach (KeyValuePair<PlayerScript, QTEPlayer> qp in attachedPlayers)
        {
            qp.Value.representation.transform.position = tracer.PlayerPos();
        }

        if (tracer.Finished()) { End(true); }
    }

    protected override void PlayerAdded(PlayerScript player)
    {
        QTEPlayer p = attachedPlayers[player];
        if (p.representation == null)
        {
            GameObject pCursor = Instantiate(DrawPlayerPrefab);

            p.representation = pCursor.GetComponent<RectTransform>();
            p.representation.transform.parent = this.transform;
            p.representation.transform.forward = transform.forward;

            p.representation.transform.localScale = new Vector3(1f, 1f, 1f);

            Color c = player.myColor;
            c.a = 0.69f; // Nice - Marc 2019
            pCursor.transform.GetChild(0).GetComponent<Image>().color = c; 
        }

        GiveWand wandControl = player.GetComponent<GiveWand>();
        Debug.Log("wandControl = " + wandControl);
        if (wandControl != null)
        {
            wandControl.CastingSpell(true);
            //player.myAnimator.SetBool("IsStirring", true);
        }
    }
}
