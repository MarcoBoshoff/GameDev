using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonBehaviour : MonoBehaviour
{
    public GameObject DebugText, Highlight, Disabled, potionIcon, ButtonText;
    public GameObject[] IconLocation = new GameObject[2];
    public ResourceType PotionRecipe;
    public bool Unlocked = false;
}
