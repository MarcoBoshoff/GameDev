using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class FMOD_ControlScript : MonoBehaviour
{
    //*************************************************************
    //********  ************  ********  ******
    //**        **   **   **  **    **  **    **
    //*******   **   **   **  **    **  **    **
    //**        **   **   **  **    **  **    **
    //**        **        **  **    **  **    **
    //**        **        **  ********  ******  
    //*************************************************************

    public static string respawnSoundPath;

    public static string successEventPath, failEventPath;

    public static string coinCollisionEventPath;

    public static string changeSelectionEventPath, confirmSelectionEventPath;

    public static string cancelEventPath;

    public static string damageEventPath, repairEventPath;

    public static string coinSpawnEventPath, coinImpactEventPath;

    public static string throwSoundEventPath;

    public static string interactorLandEventPath;

    private string bgMusicEventPath, menuMusicEventPath;

    public FMOD_Control_Data FMOD_Data;
    public static FMOD_Control_Data fmodData;

    public static FMOD_ControlScript local;

    //Parameters Inside FMOD
    public static FMOD.Studio.ParameterInstance perDangerousStock, player1Melody, player2Melody;

    public static FMOD.Studio.ParameterInstance pause;
    private float pauseValue;

    //FMOD Audio Events
    public static FMOD.Studio.EventInstance FEbgMusic, menuMusic, nightAmbience;
    public static float musicReleaseLength;

    //Simple variables (are passed into FMOD parameters)
    [Range(0, 5)]
    public int StockWarningLevel = 2, StockNumFullIntensity = 5;

    //Static variables
    public static float actionsp1 = 0, actionsp2 = 0;

    public bool muteMusic = false; // stops music from playing at start of game

    public Dictionary<string, string> eventPaths = new Dictionary<string, string>();

    private static float maxPanWidth;

    public static bool created = false; // checks if this object exists already

    //*************************************************************

    public static AudioSource patienceAudio;

    //*************************************************************

    private void Awake()
    { 
        // Check if this object already exists (eg. from menu scene)
        if (created)
        {
            Destroy(this);
        }

        else
        {
            created = true;
            DontDestroyOnLoad(this); // keeps object persistent between scenes
            FMODSetup();
        } 
    }

    // Start is called before the first frame update
    public void Init()
    {
        FMOD_ControlScript.local = this;
    }

    private void FMODSetup()
    {
        fmodData = FMOD_Data;

        // sets up globally accessible sound events
        respawnSoundPath = FMOD_Data.respawnSoundPath;
        successEventPath = FMOD_Data.successEventPath;
        failEventPath = FMOD_Data.failEventPath;
        coinCollisionEventPath = FMOD_Data.coinCollsionEventPath;

        // UI Sounds
        changeSelectionEventPath = FMOD_Data.changeSelectionEventPath;
        cancelEventPath = FMOD_Data.cancelEventPath;
        confirmSelectionEventPath = FMOD_Data.confirmSelectionEventpath;

        damageEventPath = FMOD_Data.damageEventPath;
        repairEventPath = FMOD_Data.repairEventPath;
        coinSpawnEventPath = FMOD_Data.coinSpawnEventPath;
        coinImpactEventPath = FMOD_Data.coinImpactEventPath;
        bgMusicEventPath = FMOD_Data.bgMusicEventPath;
        menuMusicEventPath = FMOD_Data.menuMusicEventPath;
        throwSoundEventPath = FMOD_Data.throwSoundEventPath;
        musicReleaseLength = FMOD_Data.musicReleaseLength;

        // sets min and max pan widtht
        maxPanWidth = FMOD_Data.maxPanWidth;

        FMOD_ControlScript.FEbgMusic = FMODUnity.RuntimeManager.CreateInstance(bgMusicEventPath);
        FMOD_ControlScript.menuMusic = FMODUnity.RuntimeManager.CreateInstance(menuMusicEventPath);

        //FMOD linkings
        FMOD_ControlScript.FEbgMusic.getParameter("Intensity", out perDangerousStock);
        FMOD_ControlScript.FEbgMusic.getParameter("Player 1", out player1Melody);
        FMOD_ControlScript.FEbgMusic.getParameter("Player 2", out player2Melody);
        FMOD_ControlScript.FEbgMusic.getParameter("Pause", out pause);

        patienceAudio = GetComponent<AudioSource>(); // Assign 'Patience_v2' audioclip to this AudioSource, and disable 'Play on Awake'
    }

    private void Update()
    {
        CheckMuteState(); // checks if music should be muted
    }

    public static void PlayPatienceAudio(float length)
    {
        if (patienceAudio != null && patienceAudio.clip != null)
        {
            patienceAudio.Play();
            patienceAudio.time = patienceAudio.clip.length - length; // starts playback based on length of patience amount
        }
    }

    // mutes music if muteMusic is true
    private void CheckMuteState()
    {
        if (muteMusic)
        {
            if (PlaybackState(FEbgMusic))
            {
                StopAudio(FEbgMusic);

            }

            else if (PlaybackState(menuMusic))
            {
                StopAudio(menuMusic);
            }
        }
    }

    // stops audio for one instance
    public static void StopAudio(FMOD.Studio.EventInstance instance)
    {
        if (PlaybackState(instance))
        {
            instance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        }
    }

    // Plays a one shot of the given sound at the given position
    public static void PlaySoundOneShot(string eventPath, Vector3 position)
    {
        float posX = position.x;

        if (posX  < -maxPanWidth)
        {
            posX = -maxPanWidth;
        }

        else if (posX > maxPanWidth)
        {
            posX = maxPanWidth;
        }

        FMODUnity.RuntimeManager.PlayOneShot(eventPath, new Vector3(posX, 0, 0));
    }

    public static void PlaySoundOneShot(string eventPath)
    {
        FMODUnity.RuntimeManager.PlayOneShot(eventPath, new Vector3(0, 0, 0));
    }

    public static void PlaySuccessSound(Vector3 position)
    {
        PlaySoundOneShot(successEventPath, position);
    }

    public static void PlayFailSound(Vector3 position)
    {
        PlaySoundOneShot(failEventPath, position);
    }

    public static void PlaySuccessSound()
    {
        PlaySoundOneShot(successEventPath);
    }

    public static void PlayFailSound()
    {
        PlaySoundOneShot(failEventPath);
    }

    public static bool PlaybackState(FMOD.Studio.EventInstance instance)
    {
        FMOD.Studio.PLAYBACK_STATE playbackState;
        instance.getPlaybackState(out playbackState);
        bool isPlaying = playbackState != FMOD.Studio.PLAYBACK_STATE.STOPPED;

        if (isPlaying)
        {
            return true;
        }

        else return false;
    }

    public static Transform SetDefaultTransform(Transform trans)
    {
        trans.position = new Vector3(trans.position.x, 0, 0);

        return trans;
    }   

    public static void ChangeSelection()
    {
        PlaySoundOneShot(changeSelectionEventPath);
    }

    public static void ConfirmSelection()
    {
        PlaySoundOneShot(confirmSelectionEventPath);
    }

    public static void CancelSelection()
    {
        PlaySoundOneShot(cancelEventPath);
    }

    public static void InvalidSelection()
    {
        CancelSelection(); // **** FIX THIS IF THERE'S TIME
    }

    public static void SetPauseParam(int pauseValue)
    {
        pause.setValue(pauseValue);
    }

    public static void SetMasterVolume(float value)
    {
        string masterBusString = "Bus:/";
        FMOD.Studio.Bus masterBus;

        masterBus = FMODUnity.RuntimeManager.GetBus(masterBusString);
        masterBus.setVolume(value / 100); // value comes in at value between 0-100, must ensure it's only a value between 0-1
    }

    public static float GetMasterVolume()
    {
        float vol, finalVol;
        string masterBusString = "Bus:/";
        FMOD.Studio.Bus masterBus;

        masterBus = FMODUnity.RuntimeManager.GetBus(masterBusString);
        masterBus.getVolume(out vol, out finalVol);

        return finalVol;
    }
}
