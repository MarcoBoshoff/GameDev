using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollectionScript : MonoBehaviour
{
    public MeshRenderer glowRenderer;
    public List<ItemScript> itemsToCollect = new List<ItemScript>();

    private Vector3 largeBottleScale;

    // Start is called before the first frame update
    void Start()
    {
        GetComponent<Renderer>().enabled = false;
        largeBottleScale = new Vector3(14, 14, 14);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        //Material portalGlowMat = glowRenderer.material;

        //TODO: This material glow for highlighting portal
        //float glowIntensity = portalGlowMat.GetFloat("_glow_intensity");
        //if (itemsToCollect.Count > 0 && glowIntensity < 1)
        //{
        //    glowIntensity = Mathf.MoveTowards(glowIntensity, 1, 0.1f);
        //}
        //else if (itemsToCollect.Count == 0 && glowIntensity > 0)
        //{
        //    glowIntensity = Mathf.MoveTowards(glowIntensity, 0, 0.1f);
        //}

        //portalGlowMat.SetFloat("_glow_intensity", glowIntensity);

        for (int i = itemsToCollect.Count - 1; i >= 0; i--)
        {
            ItemScript item = itemsToCollect[i];

            if (item == null) { itemsToCollect.RemoveAt(i); break; }
            item.transform.Translate((transform.position - item.transform.position).normalized * Time.deltaTime*55, Space.World);

            Vector3 currentScale = item.transform.localScale;
            item.transform.localScale = Vector3.MoveTowards(currentScale, largeBottleScale, 0.3f);

            if (Vector3.Distance(transform.position, item.transform.position) - item.transform.localScale.x <= 1)
            {
                itemsToCollect.Remove(item);
                Destroy(item.gameObject);
                GameControllerScript.StockUpdate(item.PlayerAssists, item.resourceCompound.resourceType);
            }
        }
    }
}
