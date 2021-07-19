using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HourGlass : MonoBehaviour
{
    // Start is called before the first frame update

    public Material material;
    public float speed = 0.25f;

    private float t = 1;

    // Update is called once per frame
    void Update()
    {
        t -= speed * Time.deltaTime * transform.up.y;
        t = Mathf.Clamp01(t);
        material.SetFloat("_Amount", t);

    }
}
