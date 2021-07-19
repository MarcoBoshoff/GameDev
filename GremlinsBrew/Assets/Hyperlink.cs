using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[ExecuteInEditMode]
public class Hyperlink : MonoBehaviour
{
    public Text masterText;
    public Text[] text;

    public void OpenLink()
    {
        Application.OpenURL(EventSystem.current.GetComponent<Text>().text);
    }

    private void Update()
    {
        foreach (Text t in text)
        {
            t.color = masterText.color;
            t.fontSize = masterText.fontSize;
            t.font = masterText.font;
        }
    }
}
