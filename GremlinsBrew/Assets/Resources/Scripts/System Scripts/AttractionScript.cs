using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttractionScript : MonoBehaviour
{
    public Transform Source;
    public Rigidbody Attract;
    public float distance = 15f;
    public float speed = 3.0f;

    // Update is called once per frame
    void Update()
    {
        float step = speed * Time.deltaTime; // calculate distance to move
        if (Vector3.Distance(Source.position, Attract.position) > distance)
        {
            Attract.AddForce((Source.position - Attract.position).normalized);
        }
    }
}
