using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ExtensionMethods
{
    /// <summary>
    /// Calculates a offset position, given a base position and rotation
    /// </summary>
    /// <param name="v"></param>
    /// <param name="offset"></param>
    /// <param name="rot"></param>
    public static Vector3 CalcualteOffsetPos(this Vector3 v, Vector3 offset, float rot)
    {
        float rotation = (rot) * (Mathf.Deg2Rad);

        double newx = (offset.x) * Mathf.Cos(rotation) - (offset.z) * Mathf.Sin(rotation);
        double newz = (offset.x) * Mathf.Sin(rotation) + (offset.z) * Mathf.Cos(rotation);

        return new Vector3((float)(v.x + newx), v.y + offset.y, (float)(v.z + newz));
    }

    /// <summary>
    /// Extension method which countains how many of the resources aren't null (a.k.a are stored)
    /// </summary>
    /// <param name="dict"></param>
    /// <returns></returns>
    public static int CountNonNulls(this Dictionary<ResourceType, ResourceEffect> dict)
    {
        int count = 0;
        foreach (KeyValuePair<ResourceType, ResourceEffect> pair in dict)
        {
            if (pair.Value != ResourceEffect.Null)
            {
                //Debug.Log("There is a: " + pair.Key);
                count++;
            }
        }

        return count;
    }

    /// <summary>
    /// Extension method which looks through items until it finds one that is a potion
    /// </summary>
    /// <param name="dict"></param>
    /// <returns></returns>
    public static ResourceType GetPotion(this Dictionary<ResourceType, ResourceEffect> dict)
    {
        foreach (KeyValuePair<ResourceType, ResourceEffect> pair in dict)
        {
            if (pair.Value != ResourceEffect.Null)
            {
                if (ResourceCompound.IsPotion(pair.Key) || pair.Key == ResourceType.SpoiltPotion)
                {
                    return pair.Key;
                }
            }
        }

        return ResourceType.Empty;
    }


    /// <summary>
    /// Extension method which looks through items until it finds one that does not have the normal effect
    /// </summary>
    /// <param name="dict"></param>
    /// <returns></returns>
    public static ResourceType GetNotNormal(this Dictionary<ResourceType, ResourceEffect> dict)
    {
        foreach (KeyValuePair<ResourceType, ResourceEffect> pair in dict)
        {
            if (pair.Value != ResourceEffect.Normal && pair.Value != ResourceEffect.Null)
            {
                return pair.Key;
            }
        }

        return ResourceType.Empty;
    }

    /// <summary>
    /// Extension method which looks through items until it finds one stored without an effect
    /// </summary>
    /// <param name="dict"></param>
    /// <returns></returns>
    public static ResourceType GetNormal(this Dictionary<ResourceType, ResourceEffect> dict)
    {
        foreach (KeyValuePair<ResourceType, ResourceEffect> pair in dict)
        {
            if (pair.Value == ResourceEffect.Normal)
            {
                return pair.Key;
            }
        }

        return ResourceType.Empty;
    }

    /// <summary>
    /// Extension method which looks through items and returns a effect that's not null.
    /// </summary>
    /// <param name="dict"></param>
    /// <returns></returns>
    public static ResourceType GetAll(this Dictionary<ResourceType, ResourceEffect> dict)
    {
        foreach (KeyValuePair<ResourceType, ResourceEffect> pair in dict)
        {
            if (pair.Value != ResourceEffect.Null)
            {
                return pair.Key;
            }
        }

        return ResourceType.Empty;
    }

    public static bool Contains(this ResourceType[] me, ItemScript item)
    {
        bool accepted = false; //Will this object be accepted

        //Go through the list of accepted resources and check if player resource is accepted
        for (int i = 0; i < me.Length; i++)
        {
            //Check if Players item is accepted by this object
            if (me[i].Equals(item.resourceCompound.resourceType))
            {
                //The player held object can be accepted
                accepted = true;
                break;
            }
        }

        return accepted;
    }

    public static bool Contains(this ResourceCompound[] me, ItemScript item)
    {
        bool accepted = false; //Will this object be accepted

        //Go through the list of accepted resources and check if player resource is accepted
        for (int i = 0; i < me.Length; i++)
        {
            ResourceCompound temp = me[i];
            ResourceCompound other = item.resourceCompound;

            //Check if Players item is accepted by this object
            if (temp.resourceType.Equals(other.resourceType) 
                && temp.resourceEffect.Equals(other.resourceEffect))
            {
                //The player held object can be accepted
                accepted = true;
                break;
            }
        }

        return accepted;
    }

    /// <summary>
    /// Extension method which checks if the potion is in the currently unlocked list
    /// </summary>
    /// <param name="me"></param>
    /// <returns></returns>
    public static bool IsAPotion(this ResourceType me)
    {
        return GameControllerScript.local.currentUnlockedPotion.Contains(me);
    }

    public static ItemScript ClosestItem(this List<ItemScript> l, Vector3 pos, float maxDist, List<ResourceType> restriction)
    {
        ItemScript temp = null;

        float tempDist = maxDist;
        foreach (ItemScript item in l)
        {
            if (item == null)
            {
                l.Remove(item);
                temp = l.ClosestItem(pos, maxDist, restriction);
                break;
            }
            else if (item.busy) { continue; }
            else if (restriction.Count > 0 && restriction.Contains(item.resourceCompound.resourceType)) { continue; }
            
            Vector3 testPos = item.GetComponent<Collider>().ClosestPoint(pos);
            testPos.y = pos.y;
            float dist = Vector3.Distance(pos, testPos);
            if (dist < tempDist)
            {
                tempDist = dist;
                temp = item;
            }
        }

        return temp;
    }

    public static List<T> SimpleClone<T>(this List<T> listToClone)
    {
        List<T> newList = new List<T>();

        foreach (T element in listToClone)
        {
            newList.Add(element);
        }

        return newList;
    }
}

