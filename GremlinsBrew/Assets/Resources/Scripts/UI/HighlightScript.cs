using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HighlightScript : MonoBehaviour
{
    private float glowProg = 0, maxGlow = 2;
    bool glow = false;

    [SerializeField]
    Renderer[] renderers;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (glow && glowProg < maxGlow)
        {
            glowProg = Mathf.MoveTowards(glowProg, maxGlow, Time.fixedDeltaTime* maxGlow);

            foreach (Renderer renderer in renderers)
            {
                renderer.material.SetFloat("_glow_intensity", glowProg);
            }
        }
        else if (!glow && glowProg > 0)
        {
            glowProg = Mathf.MoveTowards(glowProg, 0, Time.fixedDeltaTime * maxGlow);

            foreach (Renderer renderer in renderers)
            {
                renderer.material.SetFloat("_glow_intensity", glowProg);
            }
        }
    }

    public void EnableGlow()
    {
        glow = true;
    }

    public void DisableGlow()
    {
        glow = false;
    }
}
