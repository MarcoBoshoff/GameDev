using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PromptSpriteScript : MonoBehaviour
{
    public Image renderer;
    private float life = 0, fadeSpeed = 2;

    public void Display(Sprite sprite)
    {
        life = Mathf.MoveTowards(life, 1, (Time.deltaTime * fadeSpeed * 2));
        renderer.sprite = sprite;
    }

    // Update is called once per frame
    void Update()
    {
        life -= Time.deltaTime * fadeSpeed;

        if (life >= 0)
        {
            Color c = renderer.color;
            c.a = life;
            renderer.color = c;
        }

        if (life < -1)
        {
            Destroy(gameObject);
        }
    }
}
