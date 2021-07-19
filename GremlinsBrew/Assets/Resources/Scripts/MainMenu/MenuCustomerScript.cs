using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuCustomerScript : MonoBehaviour
{
    public MenuAiControlScript AiControl;
    public MenuAiPointScript currentPoint;
    public Animator myAnimator;

    public float waiting = 0;
    public List<MenuAiCommand> commands;

    public Quaternion faceTarget;

    // Start is called before the first frame update
    void Start()
    {
        AiControl.SetOccupied(currentPoint, true);
        waiting = Random.Range(3f, 16f);
    }

    // Has a current target and animation
    void FixedUpdate()
    {
        Vector3 currentTarget = currentPoint.transform.position;
        currentTarget.y = transform.position.y;

        if (transform.position != currentTarget)
        {
            faceTarget = Quaternion.FromToRotation(Vector3.left, (currentTarget - transform.position).normalized);

            transform.position = Vector3.MoveTowards(transform.position, currentTarget, 0.2f);
            myAnimator.SetBool("Walk", true);
        }
        else
        {
            myAnimator.SetBool("Walk", false);
            waiting = Mathf.MoveTowards(waiting, 0, Time.fixedDeltaTime);

            if (waiting == 0)
            {
                if (commands == null || commands.Count == 0)
                {
                    myAnimator.SetBool("Order", false);
                    myAnimator.SetBool("Pickup", false);
                    myAnimator.SetBool("Hulk", false);

                    commands = AiControl.RequestNewPoint(currentPoint);

                    if (commands.Count > 0)
                    {
                        currentPoint = commands[0].point;
                    }
                    waiting = 0.5f;
                }
                else
                {
                    MenuAiCommandEnum command = commands[0].command; //Uses current command from the queue

                    switch (command)
                    {
                        case MenuAiCommandEnum.LookAt:
                            Vector3 tempTarget = commands[0].point.transform.position;
                            tempTarget.y = transform.position.y;

                            faceTarget = Quaternion.FromToRotation(Vector3.left, (tempTarget - transform.position).normalized);
                            waiting = commands[0].waitFor;
                            break;
                        case MenuAiCommandEnum.Order:
                            myAnimator.SetBool("Order", true);
                            waiting = commands[0].waitFor;
                            break;
                        case MenuAiCommandEnum.Pickup:
                            myAnimator.SetBool("Pickup", true);
                            waiting = commands[0].waitFor;
                            break;
                        case MenuAiCommandEnum.Smash:
                            myAnimator.SetBool("Hulk", true);
                            waiting = commands[0].waitFor;
                            break;
                        default:
                            waiting = commands[0].waitFor;
                            break;
                    }

                    commands.RemoveAt(0);
                }
            }
        }

        if (transform.rotation != faceTarget)
        {
            transform.rotation = Quaternion.RotateTowards(transform.rotation, faceTarget, 1.6f);
        }
    }
}
