using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using InControl;

public class SplashscreenScript : MonoBehaviour
{
    bool loaded = false;

    public GameObject cam, background, foreground;
    public Image bgImg;
    public RectTransform _right, _left, _up;

    void Start()
    {
        Cursor.visible = false;
        FMOD_ControlScript.menuMusic.start();
    }

    // Update is called once per frame
    void Update()
    {
        if (loaded == false)
        {
            if (Input.anyKeyDown)
            {
                loaded = true;
                SceneManager.LoadScene(1, LoadSceneMode.Additive);
                Destroy(foreground, 7f);
                Destroy(background, 8f);
                //Destroy(cam);
            }

            if (InputManager.Devices != null)
            {
                for (int i = 0; i < InputManager.Devices.Count; i++)
                {
                    if (InputManager.Devices[i].AnyButtonIsPressed)
                    {
                        loaded = true;
                        SceneManager.LoadScene(1, LoadSceneMode.Additive);
                        Destroy(foreground, 7f);
                        Destroy(background, 8f);
                        //Destroy(cam);
                        break;
                    }
                }
            }
        }
        else
        {
            if (foreground != null)
            {
                _left.Translate(Vector3.left * 5f, Space.World);
                _right.Translate(Vector3.right * 5f, Space.World);
                _up.Translate(Vector3.up * 7f, Space.World);

                Color c = bgImg.color;
                c.a = Mathf.MoveTowards(c.a, 0, 0.04f);
                bgImg.color = c;

                //background.Translate(Vector3.down * 5f, Space.World);
            }
        }
    }
}
