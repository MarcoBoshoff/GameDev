using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeavyScript : ItemScript
{
    Vector3 totalForces = Vector3.zero;
    public List<PlayerScript> playersHoldingMe = new List<PlayerScript>();

    public void AddForce(Vector3 force, Vector3 pusher)
    {
        //Add force
        totalForces += force;

        //Add rotation
        //Quaternion qRot = transform.rotation;
        //qRot *= Quaternion.rotat
    }

    public void UnattachPlayers()
    {
        foreach (PlayerScript p in playersHoldingMe)
        {
            p.ReleaseItem();
        }
        playersHoldingMe.Clear();
    }

    void Start()
    {
        _heavy = true;
        col = GetComponent<Collider>();
        //rb = GetComponent<Rigidbody>();
    }

    void LateUpdate()
    {
        Vector3 zero = Vector3.zero;
        if (totalForces != zero) {
            //if (Physics.Raycast(new Ray(transform.position, totalForces.normalized), out RaycastHit hit, totalForces.magnitude, GameControllerScript.local.environmentLayer))
            //{
            //    if (hit.collider != null)
            //    {

            transform.Translate(totalForces, Space.World);
            totalForces = zero;
        }

        foreach (PlayerScript player in playersHoldingMe)
        {
            if (col == null) { col = GetComponent<Collider>(); }

            Vector3 playerPos = player.transform.position + player.transform.forward*0.1f, closestPos = col.ClosestPoint(playerPos);

            if (Vector3.Distance(playerPos, closestPos) > 0.2f)
            {
                player.transform.position = Vector3.MoveTowards(player.transform.position, closestPos, 0.05f);
                transform.position = Vector3.MoveTowards(transform.position, player.transform.position, 0.05f);
            }

            Vector3 potionToForward = transform.position;
            potionToForward.y = player.transform.position.y;
            player.transform.forward = potionToForward - player.transform.position;
        }
    }
}
