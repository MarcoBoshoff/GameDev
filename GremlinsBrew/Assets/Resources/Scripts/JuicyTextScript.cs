using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JuicyTextScript : MonoBehaviour
{
    public TextMesh text;
    float life = 0;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        life = Mathf.MoveTowards(life, 5, Time.fixedDeltaTime);

        Color c = text.color;

        if (life < 1)
        {
            c.a = life;
        }
        else if (life < 4)
        {
            c.a = 1;
        }
        else
        {
            c.a = 1 - (life - 4);
        }
        
        text.color = c;

        transform.Translate(Vector3.up * Time.fixedDeltaTime, Space.World);

        if (life == 5) { Destroy(gameObject); }
    }
}
