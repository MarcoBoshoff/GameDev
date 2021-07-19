using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/FMOD_Control_Data", order = 1)]
public class FMOD_Control_Data : ScriptableObject
{
    [FMODUnity.EventRef]
    public string respawnSoundPath;

    [FMODUnity.EventRef]
    public string successEventPath, failEventPath;

    [FMODUnity.EventRef]
    public string menuMusicEventPath;

    [FMODUnity.EventRef]
    public string bgMusicEventPath;

    [FMODUnity.EventRef]
    public string coinCollsionEventPath;

    [FMODUnity.EventRef]
    public string changeSelectionEventPath, confirmSelectionEventpath;

    [FMODUnity.EventRef]
    public string cancelEventPath;

    [FMODUnity.EventRef]
    public string damageEventPath, repairEventPath;

    [FMODUnity.EventRef]
    public string coinSpawnEventPath, coinImpactEventPath;

    [FMODUnity.EventRef]
    public string footstepEventPath;

    [FMODUnity.EventRef]
    public string throwSoundEventPath;

    [FMODUnity.EventRef]
    public string interactorLandEventPath;

    public float musicReleaseLength = 0.5f;

    [Range(0, 12)]
    public float maxPanWidth;
}