using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuAiControlScript : MonoBehaviour
{
    private Dictionary<MenuAiPointScript, bool> occupiedStatus;

    private List<MenuAiPointScript> unoccupiedPoints = new List<MenuAiPointScript>();
    private List<MenuAiCommand> emptyCommandsList = new List<MenuAiCommand>();

    // Start is called before the first frame update
    void Awake()
    {
        occupiedStatus = new Dictionary<MenuAiPointScript, bool>();
        int childCount = transform.childCount;

        for (int i = 0; i < childCount; i++)
        {
            occupiedStatus[transform.GetChild(i).GetComponent<MenuAiPointScript>()] = false;
        }
    }

    public void SetOccupied(MenuAiPointScript point, bool status)
    {
        occupiedStatus[point] = status;
    }

    public List<MenuAiCommand> RequestNewPoint(MenuAiPointScript oldPoint)
    {
        unoccupiedPoints.Clear();

        //Find unoccupied points that are connections of the old point
        foreach (MenuAiPointScript temp in oldPoint.connections)
        {
            if (occupiedStatus[temp] == false)
            {
                unoccupiedPoints.Add(temp);
            }
        }

        //If there are unoccupied points, move to one
        if (unoccupiedPoints.Count > 0)
        {
            MenuAiPointScript newPoint = unoccupiedPoints[Random.Range(0, unoccupiedPoints.Count)];

            occupiedStatus[oldPoint] = false;
            occupiedStatus[newPoint] = true;

            return newPoint.commands.SimpleClone();
        }

        return emptyCommandsList;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

[System.Serializable]
public struct MenuAiCommand
{
    [SerializeField]
    public MenuAiPointScript point;
    [SerializeField]
    public float waitFor;
    [SerializeField]
    public MenuAiCommandEnum command;

    MenuAiCommand(MenuAiPointScript p, float w, MenuAiCommandEnum a)
    {
        point = p;
        waitFor = w;
        command = a;
    }
}

[System.Serializable]
public enum MenuAiCommandEnum { Idle, Order, Pickup, Smash, LookAt};