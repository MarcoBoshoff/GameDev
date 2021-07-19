using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArcScript : MonoBehaviour
{
    //The arc showing the throw path
    public LineRenderer line;
    public Transform targetSprite;
    private int points = 12;
    private float height = 4;

    private int life = 2;

    public Transform start, end;

    public void Start()
    {
        line.positionCount = points + 1;
    }

    private void Update()
    {
        life--;
        if (life <= 0)
        {
            Destroy(gameObject);
        }
    }

    public void ArcColour(Color c)
    {
        c.a = 0.5f;
        line.material.color = c;
    }

    public void SetArc(Vector3 start, Vector3 end, float ground)
    {
        Vector3 direction = (end - start).normalized;
        float spacing = Vector3.Distance(start, end) / (points-1);
        float baseheight = start.y;

        life = 2;

        for (int i = 0; i < points; i++)
        {
            Vector3 point = start + (direction * spacing * i);
            if (i > 0)
            {
                float t = ((i * 2f) / (points - 1)) - 1;
                point.y = (1 - t * t) * height + baseheight;
            }

            line.SetPosition(i, point);
        }

        end.y = ground;
        line.SetPosition(points, end);
        end.y += 0.5f;
        targetSprite.transform.position = end;
    }
}
