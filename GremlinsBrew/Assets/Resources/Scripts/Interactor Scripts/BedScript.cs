using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(QTEPrefsScript))]
public class BedScript : InteractorScript
{
    protected static QTEScript allBeds; //A static reference to check if the other player is sleeping when one player sleeps

    //public GameObject signModel;
    private float nightAmount = 0;
    public int pNum;

    [SerializeField]
    MeshRenderer bedMesh;

    private GameObject myTrail = null;
    public GameObject trailPrefab, smokeParticle;

    [FMODUnity.EventRef]
    public string interactionSound;

    void Start()
    {
        cooldownAmount = 0.1f; //How long before you can sleep again
       
        transform.Translate(Vector3.up * 14, Space.World);

        Init(Interactables.Bed, "Prefabs/GridSystem/SnapCauldron", 0);

        //Colour of the bed = player's colour
        if (GameControllerScript.PlayerColors != null)
        {
            bedMesh.materials[Mathf.Abs(1-pNum)].color = GameControllerScript.PlayerColors[pNum];
        }
    }

    protected override int Interact1Option(PlayerStats stats)
    {
        return (nightAmount >= 1) ? 0 : -1; //Can only be used at night
    }


    //SLEEP!
    protected override void Success(int pref_num)
    {
        FMOD_ControlScript.PlaySoundOneShot(interactionSound, transform.position);  // play bedflip sound on player action

        if (pref_num == 0 && nightAmount >= 1)
        {
            GameControllerScript.DayNight.ResetToDay();
            TutorialScript.Trigger(TutorialTrigger.WentToBed);
        }
    }

    protected override void OverridenUpdate()
    {
        float bedHeight = 0.5f;
        nightAmount = GameControllerScript.DayNight.nightAmount;

        //float targetRot = (nightAmount >= 1) ? 180 : 0;

        if (nightAmount == 1)
        {
            if (myTrail == null)
            {
                myTrail = Instantiate(trailPrefab, transform.position, Quaternion.identity);
                myTrail.transform.parent = this.transform;
            }

            Vector3 pos = transform.position;
            if (pos.y > bedHeight)
            {
                pos.y = Mathf.MoveTowards(pos.y, bedHeight, 0.2f);

                if (pos.y == bedHeight) { GameControllerScript.ShakeCameraMini(); }
            }
            transform.position = pos;
        }
        else if (myTrail != null)
        {
            Destroy(myTrail);
            Destroy(Instantiate(smokeParticle, transform.position + Vector3.up, Quaternion.identity), 5f);
            transform.Translate(Vector3.up * 14, Space.World);
        }

        //Vector3 rot = signModel.transform.eulerAngles;
        //if (rot.z != targetRot)
        //{
        //    if (rot.z == 0)
        //    {
        //        FMOD_ControlScript.PlaySoundOneShot(interactionSound, transform.position);
        //    }

        //    rot.z = Mathf.MoveTowards(rot.z, targetRot, 4f);
        //    signModel.transform.eulerAngles = rot;
        //}
    }

    protected override void OnQTEUpdate(QTE_Info info)
    {
        if (allBeds != null)
        {
            if (allBeds != currentQTE)
            {
                allBeds.Interrupt(0);
                currentQTE.Interrupt(0);
            }
        }
        else
        {
            allBeds = currentQTE;
        }
    }
}
