using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QTEPrefsScript : MonoBehaviour
{
    private GameObject axesPointSpherePrefab, axesPointTargetPrefab;
    private Dictionary<QTEType, GameObject> qtePrefabs = new Dictionary<QTEType, GameObject>();

    private Dictionary<int, QTEScript> existingQTEs = new Dictionary<int, QTEScript>();

    public List<PrefValues> pref_values = new List<PrefValues>();

    void Start()
    {
        qtePrefabs[QTEType.Memory] = Resources.Load("Prefabs/Quick_Time_Events/QTEObj_Memory") as GameObject;
        qtePrefabs[QTEType.Cook] = Resources.Load("Prefabs/Quick_Time_Events/QTEObj_Cook") as GameObject;
        qtePrefabs[QTEType.Hold] = Resources.Load("Prefabs/Quick_Time_Events/QTEObj_Hold") as GameObject;
        qtePrefabs[QTEType.Rotate] = Resources.Load("Prefabs/Quick_Time_Events/QTEObj_Rotate") as GameObject;
        qtePrefabs[QTEType.RotateCrush] = Resources.Load("Prefabs/Quick_Time_Events/QTEObj_RotateCrush") as GameObject;
        qtePrefabs[QTEType.InstantSuccess] = Resources.Load("Prefabs/Quick_Time_Events/QTEObj_Instant") as GameObject;
        qtePrefabs[QTEType.Rapid] = Resources.Load("Prefabs/Quick_Time_Events/QTEObj_Rapid") as GameObject;
        qtePrefabs[QTEType.Thumbstick] = Resources.Load("Prefabs/Quick_Time_Events/QTEObj_Thumbstick") as GameObject;
        qtePrefabs[QTEType.Draw] = Resources.Load("Prefabs/Quick_Time_Events/QTEObj_Draw") as GameObject;
        qtePrefabs[QTEType.BoltCharge] = Resources.Load("Prefabs/Quick_Time_Events/QTEObj_BoltCharge") as GameObject;
        qtePrefabs[QTEType.Catcher] = Resources.Load("Prefabs/Quick_Time_Events/QTEObj_Catcher") as GameObject;
        qtePrefabs[QTEType.Hug] = Resources.Load("Prefabs/Quick_Time_Events/QTEObj_Hug") as GameObject;
    }

    public static Vector2 QTEUiSpace(Vector3 playerPos)
    {
        Vector2 vp = Camera.main.WorldToScreenPoint(playerPos);

        return vp;
    }

    public QTEScript CreateQTE(Camera mainCam, int pref_num, Vector3 pos, InteractorScript creator, PlayerScript p)
    {
        if (pref_num >= pref_values.Count) { return null; }

        PrefValues PREF = pref_values[pref_num];

        QTEType qteType = PREF.MY_TYPE;
        if (!qtePrefabs.ContainsKey(qteType)) { return null; }

        bool wasNew = false;

        //Attaches to existing QTE if it already exists
        QTEScript newQTE = (existingQTEs.ContainsKey(pref_num)) ? existingQTEs[pref_num] : null;

        //*******************************************
        if (newQTE == null)
        {
            newQTE = Instantiate(qtePrefabs[qteType]).GetComponent<QTEScript>();

            //Core vars
            newQTE.mainCam = mainCam;
            newQTE.pref_num = pref_num;
            newQTE.transform.parent = GameControllerScript.local.QTEUIParent;

            //Scaling
            RectTransform rect = newQTE.GetComponent<RectTransform>();
            newQTE.rect = rect;
            rect.position = QTEUiSpace(pos);
            rect.localScale = new Vector3(2, 2, 2);

            //Setup start
            newQTE.Init(); 

            existingQTEs[pref_num] = newQTE;

            newQTE.myInteractor = creator;
            newQTE.PlayersNeeded = PREF.NUM_PLAYERS;

            wasNew = true;
        }
        else if (newQTE.PlayersRemaining <= 0)
        {
            return null;
        }
        //*******************************************

        newQTE.AttachPlayer(p); //Attach player
        //Things specific to attaching players is handled by the relevant qte script

        //Variables if the QTE has just been created
        if (wasNew)
        {
            switch (qteType)
            {
                case QTEType.Memory:

                    Vector2[] points = PREF.vector_array;

                    //Target
                    foreach (Vector2 point in points)
                    {
                        newQTE.AddPoint(point.normalized);
                    }
                    break;

                case QTEType.Hold:
                    //newQTE.CreateText("Hold...", p);
                    newQTE.SetFloat(PREF.float_value); //Set the timer (uses float)
                    break;

                case QTEType.InstantSuccess:
                    break; //No variables needed


                case QTEType.Rapid:
                    newQTE.SetInt(PREF.int_value); //Set the progress target (int)
                    break;

                case QTEType.Rotate:
                    newQTE.SetFloat(PREF.bool_value ? -1 : 1);
                    newQTE.SetInt(PREF.int_value); //Set the number of laps
                    break;

                case QTEType.RotateCrush:
                    newQTE.SetFloat(PREF.bool_value ? -1 : 1);
                    newQTE.SetInt(PREF.int_value); //Set the number of laps
                    break;

                case QTEType.Thumbstick:

                    Vector2 Pos = PREF.vector_valueL;
                    Vector2 PosR = PREF.vector_valueR;
                    bool SecondStick = PREF.bool_value;
                    Vector2 Thumbstick_values = PREF.vector2_value;

                    //Target
                    newQTE.GetComponent<QTE_Thumbstick>().SndJoyStick = true;
                    newQTE.AddPointDual(Pos.normalized, SecondStick,PosR.normalized, Thumbstick_values);
                    break;

                case QTEType.Draw:
                    newQTE.SetInt(PREF.int_value);
                    break;

                case QTEType.Hug:
                    newQTE.PlayersNeeded = 2;
                    //newQTE.SetFloat(PREF.float_value); //Set lightning speed
                    break;

                case QTEType.Cook:
                    newQTE.SetFloat(0.3f);
                    break;

                case QTEType.BoltCharge:
                    newQTE.SetFloat(PREF.float_value);
                    break;

                case QTEType.Catcher:
                    newQTE.SetFloat(PREF.float_value); //Set lightning speed
                    break;

                //If it doesn't have a proper type
                default:
                    Destroy(newQTE.gameObject);
                    return null;
            }

            //**************************************
            newQTE.SetupFinished(); //Setup finished
            //**************************************
        }

        return newQTE;
    }

    public string String_Of(int i)
    {
        if (i < pref_values.Count && i >= 0)
        {
            return pref_values[i].PROMPT_TEXT;
        }

        return "";
    }

    public BtnPromptEnum BtnPromptOf(int i)
    {
        if (i < pref_values.Count && i >= 0)
        {
            return pref_values[i].PROMPT_BTN;
        }

        return BtnPromptEnum.None;
    }

    public QTEType QTE_TypeOf(int i)
    {
        if (i < pref_values.Count && i >= 0)
        {
            return pref_values[i].MY_TYPE;
        }

        return QTEType.None;
    }
}

[System.Serializable]
public class PrefValues
{
    public string PROMPT_TEXT, text_value;
    public BtnPromptEnum PROMPT_BTN;
    public float float_value;
    public int int_value;
    public bool bool_value;
    public Vector2[] vector_array;
    public Vector2 vector_valueL, vector_valueR, vector2_value;
    public Vector3 vector3_value;
    


    public int NUM_PLAYERS;
    public QTEType MY_TYPE;

}
