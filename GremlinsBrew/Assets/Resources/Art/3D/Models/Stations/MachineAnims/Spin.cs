using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spin : MonoBehaviour {

	public float spinx = 0;
	public float spiny = 0;
	public float spinz = 0;

    public bool machineIsOn = false;

	void Update () {
        if (machineIsOn == true) {
            transform.Rotate(spinx, spiny, spinz);
        } 
	}
}
