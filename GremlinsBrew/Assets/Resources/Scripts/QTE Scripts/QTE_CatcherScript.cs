using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class QTE_CatcherScript : QTEScript
{
    //How many lightning bolts have been collected
    private int COLLECTED = 0;
    private float _lightningSpeed = 14;

    private float SPACING = 1.5f, scale = 1;

    private List<RectTransform> bolts = new List<RectTransform>();

    public RectTransform myRect;
    public GameObject chargedText, boltPrefab, playerCatcherPrefab;
    public GameObject[] Crystals = new GameObject[3];
    [SerializeField]
    private Image buttonPrompt;
    [SerializeField]
    private Sprite[] PromtpSprite = new Sprite[2];

    [FMODUnity.EventRef]
    public string zapCatchEventPath;

    public override void SetupStarting()
    {
        TutorialScript.Trigger(TutorialTrigger.CatcherQTEStarted);
        scale = Camera.main.pixelWidth / 50;
    }

    // Update is called once per frame    
    protected override void QteUpdate(int playerCount)
    {
        foreach(KeyValuePair < PlayerScript, QTEPlayer > attached in attachedPlayers) {
            QTEPlayer qp = attached.Value;

            if (COLLECTED >= 3)
            {
                chargedText.SetActive(true);                
            }
        }

        for (int i = bolts.Count - 1; i >= 0; i--)
        {
            RectTransform bolt = bolts[i];
            if (bolt == null || bolt.gameObject == null)
            {
                bolts.RemoveAt(i);
                continue;
            }


            bolt.Translate(-bolt.transform.up * _lightningSpeed * (Screen.height/100), Space.World);

            foreach (KeyValuePair<PlayerScript, QTEPlayer> attached in attachedPlayers)
            {
                Vector3 pos = attached.Value.representation.position, pos2 = bolt.position;

                //Debug.Log("pos = " + pos + ", pos2 = " + pos2);

                if (Vector3.Distance(pos, pos2) < 4f * (Screen.height / 100))
                {
                    Destroy(bolt.gameObject);
                    Crystals[COLLECTED].SetActive(true);
                    COLLECTED++;

                    FMOD_ControlScript.PlaySoundOneShot(zapCatchEventPath, transform.position); // zap catch sfx
                }
            }
        }
        if (COLLECTED >= 3)
        {
            End(true);
            TutorialScript.Trigger(TutorialTrigger.TelsaSuccess);
        }
    }

    public override void MovementInput(PlayerScript player, Vector2 movement)
    {
        if (COLLECTED < 3)
        {
            QTEPlayer qtePlayer = attachedPlayers[player];

            if (movement != Vector2.zero && qtePlayer != null)
            {
                movement *= transform.right;

                Vector3 pos = qtePlayer.representation.position, myPos = myRect.position;
                pos.x += movement.x * 4;
                pos.x = Mathf.Clamp(pos.x, myPos.x-(SPACING*scale), myPos.x+(SPACING*scale));
                qtePlayer.representation.position = pos;
            }
        }
    }

    public override void Interrupt(int id, int i)
    {
        if (id == 0)
        {
            GameObject bolt = Instantiate(boltPrefab, transform.position + transform.up*2 + (transform.right * Random.Range(-SPACING, SPACING)), Quaternion.identity);
            Vector3 target = transform.position - transform.up + (transform.right * Random.Range(-SPACING*0.8f, SPACING*0.8f));
            bolt.transform.up = (bolt.transform.position - target);

            bolt.transform.parent = this.transform;
            bolt.transform.localScale = new Vector3(1, 1, 1);

            bolts.Add(bolt.GetComponent<RectTransform>());
            Destroy(bolt, 4f);
        }
    }

    //Puts the float value in the timer
    public override void SetFloat(float t)
    {
        _lightningSpeed = t;
    }

    protected override void PlayerAdded(PlayerScript player)
    {
        if (player.gamePad == true)
        {
            buttonPrompt.GetComponent<Image>().sprite = PromtpSprite[0];
        }
        else
        {
            buttonPrompt.GetComponent<Image>().sprite = (player.PLAYERNUM == 1) ? (PromtpSprite[1]) : (PromtpSprite[2]);
        }



        QTEPlayer p = attachedPlayers[player];
        if (p.representation == null)
        {
            GameObject pointer = Instantiate(playerCatcherPrefab, myRect.position + (Vector3.down * scale*3), Quaternion.identity);
            p.representation = pointer.GetComponent<RectTransform>();
            p.representation.transform.parent = this.transform;

            p.representation.localScale = new Vector3(1, 1, 1);
            //pointer.transform.GetChild(0).GetComponent<Renderer>().material.color = player.myColor;
        }
    }
}
