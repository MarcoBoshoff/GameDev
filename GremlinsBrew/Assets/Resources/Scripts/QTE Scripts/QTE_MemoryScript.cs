using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class QTE_MemoryScript : QTEScript
{
    //AxesPoints
    private List<GameObject> axesTargets = new List<GameObject>();
    private bool axesEnabled = false;
    private float nextAxesCount = 0;
    private int currentTarget = 0;
    private Color highlightColor;

    public Sprite[] symbols;

    public GameObject playerPointerPrefab;

    //Override for any values before setup starts
    public override void SetupStarting(){
        highlightColor = new Color(0.7f, 0.7f, 0.7f);
    }

    //Override for any values after setup has finished -> ie, after all the axes targets have been added
    public override void SetupFinished()
    {
        //Axis Points init
        if (axesTargets.Count > 0)
        {
            nextAxesCount = 1.1f / axesTargets.Count; //1.5f = how many seconds it takes to show all qtes
            TIMER = nextAxesCount;
        }
    }

    //Receive Movement Input
    public override void MovementInput(PlayerScript player, Vector2 movement)
    {
        QTEPlayer qtePlayer = attachedPlayers[player];

        if (qtePlayer != null)
        {

            if (movement.magnitude > 1)
            {
                movement = movement.normalized;
            }

            //Move the player's sphere
            if (axesEnabled)
            {
                Vector3 newPos = transform.position + transform.right * movement.x;
                newPos += transform.up * movement.y;
                //sphere1.transform.position = transform.position + (Vector3)storedMovement;
                qtePlayer.representation.transform.position = newPos;
            }
        }
    }

    //Just the update method
    protected override void QteUpdate(int playerCount)
    {
        if (axesTargets.Count == 0)
        {
            //Player has removed all targets
            End(true);
        }
        else
        {
            //Show each target in order
            if (currentTarget <= axesTargets.Count)
            {
                if (TIMER <= 0)
                {
                    if (currentTarget < axesTargets.Count)
                    {
                        axesTargets[currentTarget].transform.GetChild(0).GetComponent<Renderer>().enabled = true;
                    }
                    currentTarget++;

                    if (currentTarget > axesTargets.Count)
                    {
                        axesEnabled = true; //Allow player to move

                        foreach (KeyValuePair<PlayerScript, QTEPlayer> qp in attachedPlayers)
                        {
                            qp.Value.representation.transform.GetChild(0).GetComponent<Renderer>().enabled = true;
                        }
                    }
                    else
                    {
                        TIMER = nextAxesCount;
                    }
                }
                else
                {
                    TIMER -= Time.deltaTime;
                }
            }
            else
            {
                //Handle the actual axes point QTE
                foreach (KeyValuePair<PlayerScript, QTEPlayer> qp in attachedPlayers)
                {
                    Vector3 pos = qp.Value.representation.transform.position;
                    if (Vector3.Distance(pos, axesTargets[0].transform.position) < 0.3f)
                    {
                        //if (axesTargets.Count > 1)
                        //{
                        //    axesTargets[1].GetComponent<Renderer>().material.color = highlightColor;
                        //}

                        GameObject temp = axesTargets[0];
                        axesTargets.Remove(temp);
                        Destroy(temp);
                        break;
                    }

                    for (int i = 1; i < axesTargets.Count; i++)
                    {
                        if (Vector3.Distance(pos, axesTargets[i].transform.position) < 0.3f)
                        {
                            //Hit the wrong sphere

                            foreach (GameObject temp in axesTargets)
                            {
                                Destroy(temp);
                            }

                            End(false);
                            break;
                        }
                    }
                }
            }
        }
    }

    //Add a new target point to hit
    public override void AddPoint(Vector3 point)
    {
        GameObject axesTarget = Instantiate(axesPointTargetPrefab, transform.position, Quaternion.identity);

        //if (axesTargets.Count == 0) { axesTarget.GetComponent<Renderer>().material.color = highlightColor; }

        axesTarget.transform.GetChild(0).GetComponent<SpriteRenderer>().sprite = symbols[Random.Range(0, symbols.Length)];

        axesTargets.Add(axesTarget);
        axesTarget.transform.GetChild(0).GetComponent<Renderer>().enabled = false;

        point = point.normalized;

        Vector3 pos = axesTarget.transform.position;
        pos += transform.right * point.x;
        pos += transform.up * point.y;

        axesTarget.transform.position = pos;
        axesTarget.transform.parent = this.transform;

        axesTarget.transform.forward = mainCam.transform.position - axesTarget.transform.position;
    }

    //New player added, so add a text prompt above them
    protected override void PlayerAdded(PlayerScript player)
    {
        QTEPlayer p = attachedPlayers[player];
        if (p.representation == null)
        {
            GameObject pointer = Instantiate(playerPointerPrefab);

            p.representation = pointer.GetComponent<RectTransform>();
            p.representation.transform.parent = this.transform;

            pointer.transform.GetChild(0).GetComponent<Renderer>().material.color = player.myColor;

            //Invisible at start of axis point QTE
            if (!axesEnabled)
            {
                p.representation.transform.GetChild(0).GetComponent<Renderer>().enabled = false;
            }

            pointer.transform.forward = pointer.transform.position - mainCam.transform.position;
        }
    }
}
