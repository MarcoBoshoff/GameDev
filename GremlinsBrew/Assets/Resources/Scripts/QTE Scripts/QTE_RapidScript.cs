using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class QTE_RapidScript : QTEScript
{
    //Rapid
    private Text textMesh;
    private bool _usingFireButtons = true;
    private int progressTarget = 0;

    // Start is called before the first frame update

    //Override for any values before setup starts
    public override void SetupStarting(){ }

    //Override for any values after setup has finished
    public override void SetupFinished(){ }

    //Receive Movement Input
    public override void MovementInput(PlayerScript player, Vector2 movement)
    {
        if (movement.magnitude > 0)
        {
            End(false);
        }
    }

    protected override void QteUpdate(int playerCount)
    {
        int totalProgres = 0;

        foreach (KeyValuePair<PlayerScript, QTEPlayer> p in attachedPlayers)
        {
            QTEPlayer qp = p.Value;

            float sign = Mathf.Sign(qp.progress);

            if (qp.fireButtonDown[(qp.progress >= 0) ? 0 : 1])
            {
                float fractionOf = Mathf.Min(1f, ((float)attachedPlayers.Count / (float)PlayersNeeded));

                if (Mathf.Abs(qp.progress) < (progressTarget * fractionOf))
                {
                    qp.progress = (int)(Mathf.Abs(qp.progress) + 1);

                    qp.progress *= (int)(sign * -1);
                }
                else
                {
                    qp.progress *= -1;
                }
            }

            //qp.myText.text = qp.buttonNames[qp.myNum*2 + (int)Mathf.Max(0, -sign)];

            totalProgres += Mathf.Abs(qp.progress);
        }

        if (attachedPlayers.Count < PlayersNeeded)
        {
            foreach (KeyValuePair<PlayerScript, QTEPlayer> p in attachedPlayers)
            {
                QTEPlayer qp = p.Value;
                qp.myText.text = string.Format("Needs {0} players", PlayersNeeded);
            }
        }

        if (Mathf.Abs(totalProgres) / attachedPlayers.Count >= progressTarget)
        {
            End(true);
        }
    }

    //Overrides bool to set whether using fire buttons to stick
    public override void SetBool(bool b)
    {
        _usingFireButtons = b;
    }

    //Overrides int to set the progress target for the rapid movement
    public override void SetInt(int i)
    {
        progressTarget = i;
    }

    protected override void PlayerAdded(PlayerScript p) {
        CreateText("Waiting...", p);
    }

    public override void CreateText(string text, PlayerScript p)
    {
        //Instantiate the text
        GameObject temp = Instantiate(textPrefab, transform.position, Quaternion.identity) as GameObject;
        temp.transform.parent = this.transform;

        //Attach the text object to the player
        QTEPlayer qp = attachedPlayers[p];
        qp.myText = temp.transform.GetChild(0).GetComponent<Text>();

        //Starting value
        //qp.myText.text = qp.buttonNames[qp.myNum * 2];
        qp.myText.color = p.myColor;

        //Position the text above the player
        temp.transform.position = p.transform.position + Vector3.up * 5;

        //Make sure the text faces the camera
        temp.transform.forward = mainCam.transform.forward;
    }
}



