using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RonScript : MonoBehaviour
{
    public AudioSource voice;
    [SerializeField]
    private GameObject Ron;

    private float timer = 0;
    
    public void ActivateRon()
    {
        if (Ron != null)
        {
            timer = 11f;
            Ron.SetActive(true);
            voice.Play();
        }
    }

    public void FixedUpdate()
    {
        if (timer > 0)
        {
            timer = Mathf.MoveTowards(timer, 0, Time.fixedDeltaTime);
            if (timer == 0) { Destroy(Ron); }

            if (timer <= 1)
            {
                Ron.transform.localScale = new Vector3(timer*timer, timer*timer, 1);
            }
        }
    }
}
