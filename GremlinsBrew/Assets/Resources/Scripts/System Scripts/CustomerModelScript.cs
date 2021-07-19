using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomerModelScript : MonoBehaviour
{
    public Animator myAnimator;
    public GameObject customerHealth, customerPoison, customerMana, customerLove;
    public float pause = 0;

    private float progress = 0;
    private Quaternion targetRot;
    
    public void TargetRot(Quaternion tr)
    {
        targetRot = tr;
        progress = 0;
    }

    void FixedUpdate()
    {
        if (progress < 1) { progress = Mathf.MoveTowards(progress, 1, Time.fixedDeltaTime*4f); }
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, progress);

        pause = Mathf.MoveTowards(pause, 0, Time.fixedDeltaTime);
    }
}

//"Paint me like one of those french lizards" - Lizard customers to Lizzy