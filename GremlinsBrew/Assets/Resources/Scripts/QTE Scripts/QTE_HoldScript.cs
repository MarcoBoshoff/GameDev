using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class QTE_HoldScript : QTEScript
{
    //Hold specific variables
    private float startTimer = 0;
    private int targettedFireButton = 0;
    private bool overhold = false;

    public RectTransform holdIcon;
    
    // Update is called once per frame    
    protected override void QteUpdate(int playerCount)
    {
        //Specific stuff for hold

        float holdSpeed = (0.5f * (playerCount - 1)) + 1;
        TIMER -= Time.deltaTime * holdSpeed;

        bool holdingFireButton = true;

        foreach (KeyValuePair<PlayerScript, QTEPlayer> p in attachedPlayers)
        {
            holdingFireButton = (p.Value.fireButtonDown[targettedFireButton]);

            if (!holdingFireButton) { break; }
        }
        if (TIMER < (startTimer - 0.3f) && holdingFireButton == false)
        {
            Debug.Log("Oof. They let go");
            End(false);
        }
        else if (TIMER <= 0)
        {
            End(true);
        }

        float scale = TIMER / startTimer;
        holdIcon.localScale = new Vector3(scale, scale, 1);
    }
    
    //Puts the float value in the timer
    public override void SetFloat(float t)
    {
        startTimer = t;
        TIMER = t;
    }
}
