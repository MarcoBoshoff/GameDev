using System.Collections;
using System.Collections.Generic;
using FMOD.Studio;
using UnityEngine;

public class MenuMusicController : MonoBehaviour
{
    [FMODUnity.EventRef]
    public string musicEventPath;

    private EventInstance music;

    // Start is called before the first frame update
    void Start()
    {
        music = FMODUnity.RuntimeManager.CreateInstance(musicEventPath);
        music.start(); // starts music playback
    }

    public void Stop() // call this from another script to stop playback
    {
        music.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
    }

}
