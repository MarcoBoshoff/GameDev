using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UiPromptTextScript : MonoBehaviour
{
    public Text myText;

    private float life = 3;

    void FixedUpdate()
    {
        transform.Translate(Vector3.down * Screen.height * Time.fixedDeltaTime * 0.015f);
        life -= Time.fixedDeltaTime;

        Color c = myText.color;
        if (life > 2)
        {
            c.a = Mathf.Abs(life - 3);
        }
        else if (life > 1)
        {
            c.a = 1;
        }
        else
        {
            c.a = life;
        }

        myText.color = c;

        if (life <= 0) { Destroy(this.gameObject); }
    }
}
