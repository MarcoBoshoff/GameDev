using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuAiPointScript : MonoBehaviour
{
    //Mainly used as a struct
    [SerializeField]
    public List<MenuAiCommand> commands;
    public MenuAiPointScript[] connections;
}
