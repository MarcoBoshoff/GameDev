using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomerIndicatorScript : MonoBehaviour
{
    public GameObject indicationPrefab;
    public UIScript UI;

    private float LeftX = -640, RightX = 655;
    private List<RectTransform> indications = new List<RectTransform>();
    float dayLength;

    // Start is called before the first frame update
    void Start()
    {
        GameObject indic = Instantiate(indicationPrefab, transform.position, Quaternion.identity);
        indic.transform.SetParent(this.transform);
        indications.Add(indic.GetComponent<RectTransform>());

        UI = GameControllerScript.local.UI;
        dayLength = UI.dayNight.dayLength;
    }

    // Update is called once per frame
    void Update()
    {
        int remaining = UI.customerControl.CustomersRemaining;
        if (indications.Count < remaining)
        {
            int loop = remaining - indications.Count;
            for (int i = 0; i < loop; i++)
            {
                GameObject indic = Instantiate(indicationPrefab, transform.position, Quaternion.identity);
                indic.transform.SetParent(this.transform);
                indications.Add(indic.GetComponent<RectTransform>());
            }
        }
        else if (indications.Count > remaining)
        {
            int loop = indications.Count - remaining;
            for (int i = 0; i < loop; i++)
            {
                Destroy(indications[0].gameObject);
                indications.RemoveAt(0);
            }
        }

        float currentTime = UI.dayNight.Seconds;
        for (int i = 0; i < indications.Count; i++)
        {
            float t = UI.customerControl.CustomerTimeFrom(i);

            RectTransform rt = indications[i];
            Vector3 pos = rt.localPosition;
            pos.x = Mathf.Lerp(LeftX, RightX, (t - currentTime) / dayLength);
            rt.localPosition = pos;
        }
    }
}
