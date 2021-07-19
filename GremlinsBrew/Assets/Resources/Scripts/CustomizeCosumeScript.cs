using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomizeCosumeScript : MonoBehaviour
{
    [SerializeField]
    SkinnedMeshRenderer body; //The body to apply the base colour to

    [SerializeField]
    SkinnedMeshRenderer[] clothes; //An array of all the costume segments that need to be altered when the colour is changed

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetColour(Color c, Material matToApply)
    {
        body.material.color = c;

        //Applies the coloured material to all costume segments
        for (int i = 0; i < clothes.Length; i++)
        {
            Material[] mats = clothes[i].materials;
            mats[0] = matToApply;
            clothes[i].materials = mats;
        }
    }
}
