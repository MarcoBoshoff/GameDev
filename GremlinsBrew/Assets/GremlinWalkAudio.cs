using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GremlinWalkAudio : MonoBehaviour
{
    public FMOD_Control_Data fmodData;

    void Awake()
    {
        fmodData = GameObject.FindGameObjectWithTag("FMODControl").GetComponent<FMOD_ControlScript>().FMOD_Data;
    }

    public void PlayFootstep()
    {
        FMOD_ControlScript.PlaySoundOneShot(fmodData.footstepEventPath, transform.position);
    }
}
