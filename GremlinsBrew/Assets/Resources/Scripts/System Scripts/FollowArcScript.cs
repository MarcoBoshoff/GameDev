using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowArcScript : MonoBehaviour
{
    public ItemScript myItem;

    public List<Vector3> points = new List<Vector3>();
    private float timer = 0;

    private GameObject trail;

    void Start()
    {
        trail = Instantiate(Resources.Load("Prefabs/ThrowTrail") as GameObject, transform.position, Quaternion.identity);
        trail.transform.SetParent(this.transform);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        Collider tempCol;
        if (myItem != null)
        {
            tempCol = myItem.GetComponent<Collider>();

            if (tempCol == null)
            {
                myItem.busy = false;
                Destroy(this);
                Destroy(trail, 1f);
                trail.transform.SetParent(null);
                return;
            }
        }
        else { Destroy(this); return; }

        if (points.Count > 0 && timer < 1f)
        {
            Vector3 target = points[0];
            timer = Time.deltaTime;

            Vector3 pos = transform.position;
            pos = Vector3.MoveTowards(pos, target, 0.01f* timer);
            transform.position = target;

            if (Vector3.Distance(pos, target) < 0.1f)
            {
                timer = 0;
                points.RemoveAt(0);
            }
        }
        else
        {
            tempCol.enabled = true;
            myItem.busy = false;
            Destroy(this);

            Destroy(trail, 1f);
            trail.transform.SetParent(null);
        }
    }
}
