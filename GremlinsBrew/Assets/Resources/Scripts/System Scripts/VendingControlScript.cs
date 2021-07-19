using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VendingControlScript : MonoBehaviour
{
    public VendingLineScript Line1;
    private Dictionary<ResourceType, GameObject> bottles = new Dictionary<ResourceType, GameObject>(); //Links potion types to potion bottles
    private Dictionary<ResourceType, List<ConveyourAttachmentScript>> StockList = new Dictionary<ResourceType, List<ConveyourAttachmentScript>>();

    // Start is called before the first frame update
    public void Setup()
    {
        Line1.Setup();

        bottles[ResourceType.HealthPotion] = Resources.Load("Prefabs/VendingLines/GiantHealthBottle") as GameObject;
        bottles[ResourceType.PoisonPotion] = Resources.Load("Prefabs/VendingLines/GiantPoisonBottle") as GameObject;
        bottles[ResourceType.LovePotion] = Resources.Load("Prefabs/VendingLines/GiantLoveBottle") as GameObject;
        bottles[ResourceType.ManaPotion] = Resources.Load("Prefabs/VendingLines/GiantManaBottle") as GameObject;

        //The list that contains each giant potion
        StockList[ResourceType.HealthPotion] = new List<ConveyourAttachmentScript>();
        StockList[ResourceType.PoisonPotion] = new List<ConveyourAttachmentScript>();
        StockList[ResourceType.LovePotion] = new List<ConveyourAttachmentScript>();
        StockList[ResourceType.ManaPotion] = new List<ConveyourAttachmentScript>();
    }

    public bool StockContainPotionCheck(ResourceType t)
    {
        return StockList.ContainsKey(t);
    }

    public int StockCount(ResourceType t)
    {
        if (StockList.ContainsKey(t))
        {
            return StockList[t].Count; 
        }
        else
        {
            return 0;
        }
    }

    public void PreSpawn(ResourceType t)
    {
        StockList[t].Add(Line1.PrespawnItem(t, bottles[t]));
    }


    public void DropItem(ResourceType t, int amount)
    {
        if (StockList[t].Count >= amount)
        {
            for (int i = 0; i < amount; i++)
            {
                
                ConveyourAttachmentScript attachment = StockList[t][0];

                Destroy(attachment.attached.gameObject, 4f); //Destroy the potion bottle when it's offscreen
                Destroy(attachment.attached.GetComponent<FixedJoint>());

                StockList[t].RemoveAt(0);
                Destroy(attachment.gameObject);
            }
        }
    }

    public void AddToVendingLine(ResourceType t)
    {
        if (StockList[t].Count < GameControllerScript.local.maxPotions)
        {
            StockList[t].Add(Line1.SpawnItem(t, bottles[t]));
        }
        else
        {
            FMOD_ControlScript.PlayFailSound(Vector3.zero);
        }
    }


    public class VendingBottle
    {
        public ResourceType type;
        public ConveyourAttachmentScript attachment;

        public VendingBottle(ResourceType t, ConveyourAttachmentScript a)
        {
            type = t;
            attachment = a;
        }
    }
}



// Kris = Too lou..uh...busy? You might have to ask Ari. 
