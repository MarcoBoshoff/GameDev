using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TracerScript : MonoBehaviour
{
    [HideInInspector]
    public Transform[] pts;
    private float[] dists;

    private int i = 0;
    private const float leeway = 0.85f, max = 0.01f, multiplier = 0.001f;
    private float speed = 0, lerp_amount = 0, totalDist = 0;

    [SerializeField]
    private Image myImage;

    private Vector3 _start, _end;
    public void SetCurrent()
    {
        int count = transform.childCount;

        pts = new Transform[count];
        dists = new float[count - 1];
        for (int i = 0; i < count; i++)
        {
            pts[i] = transform.GetChild(i);
            if (i > 0)
            {
                float dist = Vector3.Distance(pts[i - 1].position, pts[i].position);
                dists[i - 1] = dist;
                totalDist += dist;
            }
        }

        _start = pts[0].position;
        _end = pts[count-1].position;
        myImage.material.SetFloat("_progress", 0.999f);
    }

    public void AddMovement(Vector2 direction)
    {
        Vector3 pos1 = (i < pts.Length) ? pts[i].position : _end;
        Vector3 pos2 = ((i + 1) < pts.Length) ? pts[i + 1].position : _end;

        Vector3 newPos = (pos1 + transform.right * direction.x);
        newPos += transform.up * direction.y;

        Vector3 movement = (newPos - pos1).normalized;

        float gen_dir = Vector3.Distance(movement, (pos2 - pos1).normalized);
        if (gen_dir <= leeway)
        {
            speed = Mathf.MoveTowards(speed, max, (Mathf.Abs(leeway - gen_dir) / leeway) * multiplier * Vector3.Distance(pos1, pos2));
        }
    }

    public bool Finished()
    {
        if (lerp_amount >= 0.99f)
        {
            return true;
        }

        return false;
    }

    void Update()
    {
       //TODO: Set sparkle position + move sparkle back to start point
    }

    public Vector3 PlayerPos()
    {
        speed = Mathf.MoveTowards(speed, 0, 0.1f*multiplier);
        lerp_amount = Mathf.MoveTowards(lerp_amount, 1, speed);

        myImage.material.SetFloat("_progress", Mathf.Clamp(1-lerp_amount, 0.001f, 0.999f));

        float distTarget = totalDist * lerp_amount;

        i = 0;
        while (distTarget > dists[i])
        {
            distTarget -= dists[i];
            i++;
        }

        Vector3 pos1 = (i < pts.Length) ? pts[i].position : _end;
        Vector3 pos2 = ((i+1) < pts.Length) ? pts[i+1].position : _end;
        return Vector3.Lerp(pts[i].position, pts[i+1].position, Mathf.Max(0, distTarget/dists[i]-0.1f));
    }
}
