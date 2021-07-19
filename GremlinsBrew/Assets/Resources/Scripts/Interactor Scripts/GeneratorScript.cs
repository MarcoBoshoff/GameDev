using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GeneratorScript : InteractorScript
{
    //## (Optional) Place variables here that arespecific to your interactor.
    public GameObject myElectricity = null;
    public static bool InProgress = false, Done = false;
    public Animator animator;

    // SFX
    [FMODUnity.EventRef]
    public string generatorStartEventPath;
    private FMOD.Studio.EventInstance generatorStartAudioEvent;

    void Start()
    {
        //## This is important if you want to be able to store items in this interactor
        storeOptionNames = new string[] { "Power Generator", "Power Generator" }; //Different store option names
        //## The ordering will relate directly to the store options method below

        cooldownAmount = 0f; //You can set a custom number for how long after a QTE it takes before this interactor can be used again

        Init(Interactables.Generator, "Prefabs/GridSystem/SnapGenerator", 1); //## Make sure init is called in the start method (used for interactor setup)

        generatorStartAudioEvent = FMODUnity.RuntimeManager.CreateInstance(generatorStartEventPath); // creates audioe event instance

        //FMODUnity.RuntimeManager.AttachInstanceToGameObject(generatorStartAudioEvent, 
        //FMOD_ControlScript.SetDefaultTransform(GetComponent<Transform>()), GetComponent<Rigidbody>()); // sets 3D attributes
        animator.SetBool("SwitchOn", false);
    }


    //## Set a limit when the first interaction can't be called (set to false to have no limit)
    protected override int Interact1Option(PlayerStats stats)
    {
        return 0;
    }

    protected override void OnQTEUpdate(QTE_Info info)
    {
        if (myElectricity == null)
        {
            myElectricity = Instantiate(electricalParticle, transform.position, Quaternion.identity);

            generatorStartAudioEvent.start();
            animator.SetBool("SwitchOn", true);
        }

        if (!GeneratorScript.Done)
        {

            GeneratorScript.InProgress = true;
            animator.SetBool("SwitchOn", true);

            if (info.Goal > 0)
            {
                TeslaInteractor.StoredBolt = true;
            }
        }
        else
        {
            GeneratorScript.Done = false;

            if (currentQTE != null)
            {
                currentQTE.Interrupt(0);
                generatorStartAudioEvent.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            }
        }
    }

    //## This method is called when the quick time event is successfully completed
    protected override void Success(int pref_num)
    {
        if (myElectricity != null)
        {
            myElectricity.GetComponent<ParticleSystem>().Stop();
            Destroy(myElectricity, 10f);
            myElectricity = null;

            // stops generator start sfx loop
            generatorStartAudioEvent.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            // success sfx here
            animator.SetBool("SwitchOn", false);
        }
        GeneratorScript.InProgress = false;

        GameControllerScript.UnisonAttempt(); //Attemps to show unison juicy text
    }

    //## Again, you can call this if the QTE fails
    protected override void Fail(int pref_num)
    {
        if (myElectricity != null)
        {
            myElectricity.GetComponent<ParticleSystem>().Stop();
            Destroy(myElectricity, 10f);
            myElectricity = null;

            generatorStartAudioEvent.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            // sfx fail if needed?
            animator.SetBool("SwitchOn", false);
        }

        GeneratorScript.InProgress = false;
    }
}
