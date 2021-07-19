using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoinCountedScript : MonoBehaviour
{
    // This script is to play the coin impact sound when a coin falls onto the coin stack

    private bool impacted = false; //So only the first impact plays a sound

    void Start()
    {
        Vector3 euler = transform.eulerAngles;
        euler.y = Random.Range(0f, 360f);
        transform.eulerAngles = euler;
        GetComponent<Renderer>().material.color = Random.ColorHSV(0.09f, 0.15f, 0.69f, 0.90f, 0.69f, 0.88f);
    }
    // ColorHSV ---> (hueMin, hueMax, saturationMin, saturationMax, valueMin, valueMax). I got Yellow somewhere between 0.1 and 0.19 by randomly guessing lol

    private void OnCollisionEnter(Collision collision)
    {
        // checks if it is impacting another coin, for the first time
        if (collision.gameObject.tag == "Heavy" && !impacted)
        {
            impacted = true;
            FMOD_ControlScript.PlaySoundOneShot(FMOD_ControlScript.coinCollisionEventPath);
        }
    }
}
