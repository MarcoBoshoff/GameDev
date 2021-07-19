using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TracerControlScript : MonoBehaviour
{
    [SerializeField]
    private TracerScript[] trace;

    public TracerScript GetATracer()
    {
        TracerScript rune = trace[Random.Range(0, trace.Length)];
        rune.gameObject.SetActive(true);
        return rune;
    }
}
