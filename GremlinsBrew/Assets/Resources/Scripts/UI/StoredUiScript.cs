using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StoredUiScript : MonoBehaviour
{
    public Sprite flower, mushroom, heart, crystal, chili;
    [SerializeField]
    private SpriteRenderer sr;
    public CauldronScript cauldron;

    public void SetSprite(ResourceType t)
    {
        switch (t)
        {
            case ResourceType.Flower:
                sr.sprite = flower;
                break;
            case ResourceType.Mushroom:
                sr.sprite = mushroom;
                break;
            case ResourceType.Lovefruit:
                sr.sprite = heart;
                break;
            case ResourceType.PwCrystal:
                sr.sprite = crystal;
                break;
            default:
                sr.sprite = chili;
                break;
        }
    }

    public Sprite GetSprite() { return sr.sprite; }

    void Update()
    {
       
        if (cauldron.IsEmpty())
        {
            Destroy(this.gameObject);
        }
    }

}
