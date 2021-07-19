using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This script returns items to the position they started in. Used for books and bricks currently
/// </summary>
public class ObjReturnScript : MonoBehaviour
{
    public Rigidbody rb;
    public Collider col;
    public float returning = 0;

    Vector3 startPos, returnPos;
    Quaternion startRot, returnRot;

    private float mySpeed = 0.2f;

    // Start is called before the first frame update
    void Awake()
    {
        startPos = transform.position;

        Vector3 e = transform.eulerAngles;
        startRot = Quaternion.Euler(e.x, e.y, e.z);

        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();

        mySpeed = Random.Range(0.01f, 0.04f);
    }

    public void ReturnToStart()
    {
        returning = 1;
        returnPos = transform.position;

        Vector3 e = transform.eulerAngles;
        returnRot = Quaternion.Euler(e.x, e.y, e.z);

        rb.isKinematic = true;
        col.isTrigger = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (returning > 0)
        {
            returning = Mathf.MoveTowards(returning, 0, mySpeed);

            transform.position = Vector3.Lerp(startPos, returnPos, returning);
            transform.rotation = Quaternion.Slerp(startRot, returnRot, returning);

            if (returning == 0)
            {
                //rb.isKinematic = false;
                col.isTrigger = false;
            }
        }
    }
}
