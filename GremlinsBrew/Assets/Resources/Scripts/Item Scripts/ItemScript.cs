using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemScript : MonoBehaviour
{
    [HideInInspector]
    protected bool _heavy = false;// Is this object heavy? If yes, it can only be pushed
    public float _weight = 1f; // Weight of the object. (Heavier objects slow down player more)
    private Transform _originSpawner;
    public bool busy = false, _grabbed;

    public ResourceCompound resourceCompound;

    private float boxHeight = -1;

    [SerializeField]
    protected Rigidbody rb;
    protected Collider col;

    public GameObject[] Models;

    private float grow = 1, fly = 0;
    private Vector3 startScale;

    public List<PlayerScript> PlayerAssists = new List<PlayerScript>();

    [FMODUnity.EventRef]
    public string dropItemEventPath;

    void Start()
    {
        col = GetComponent<Collider>();

        startScale = transform.localScale;
        if (grow == 0) { transform.localScale = Vector3.zero; }

        if (TutorialScript.localTute != null && TutorialScript.localTute.highlightingResource == resourceCompound.resourceType)
        {
            GetComponent<HighlightScript>().EnableGlow(); //Highlights this ingredient in the tutorial / dawn phase (if required)
        }
    }

    //Grows the ingredient
    void FixedUpdate()
    {
        if (_grabbed) { grow = 1; }

        if (grow < 1)
        {
            grow = Mathf.MoveTowards(grow, 1, 0.02f);

            transform.localScale = Vector3.Lerp(Vector3.zero, startScale, grow);

            if (grow == 1) { busy = false; }
        }

        if (fly > 0)
        {
            transform.Translate(Vector3.up * fly, Space.World);
        }
    }

    public void Grow()
    {
        grow = 0;
        busy = true;
    }

    //Fly away at the end of the day
    public void FlyAway()
    {
        fly = Random.Range(0.1f,0.4f);
        busy = true;
        rb.useGravity = false;
        Destroy(gameObject, 10f);

        Instantiate(Resources.Load("Prefabs/Interactors/InteractorDropTrail") as GameObject, transform.position, Quaternion.identity).transform.parent = this.transform;
    }

    public Vector3 ClosestPoint(Vector3 pos)
    {   
        return col.ClosestPoint(pos);
    }

    //Object has been grabbed or dropped
    public virtual void Grabbing(bool grabbing, PlayerScript p)
    {
        _grabbed = grabbing;

        rb.useGravity = (_heavy == grabbing) && !busy;
        rb.freezeRotation = (!_heavy && grabbing);
        rb.isKinematic = (_heavy != grabbing) || busy;

        if (grabbing)
        {
            //Tutorial trigger
            switch (resourceCompound.resourceType)
            {
                case ResourceType.Flower:
                    TutorialScript.Trigger(TutorialTrigger.FlowerPickup);
                    break;

                case ResourceType.Mushroom:
                    TutorialScript.Trigger(TutorialTrigger.MushroomPickup);
                    break;
            }
        }
    }

    //Uses one of the other models for the item
    public void NewForm(int index)
    {
        for (int i = 0; i < Models.Length; i++)
        {
            Models[i].SetActive(i == index);
        }
    }

    public bool Heavy
    {
        get
        {
            return _heavy;
        }
    }

    public float Weight
    {
        get
        {
            return _weight;
        }
    }

    public Transform OriginSpawner
    {
        get { return _originSpawner; }
        set { _originSpawner = value; }
    }

    public float BoxHeight
    {
        get
        {
            if (boxHeight < 0)
            {
                boxHeight = GetComponent<Collider>().bounds.extents.y;
            }

            return boxHeight;
        }
    }

    public void AddPlayerAssist(PlayerScript p)
    {
        if (PlayerAssists.Contains(p) == false && PlayerAssists.Count > 0)
        {
            Instantiate(GameControllerScript.juicyTextPrefab, transform.position, Quaternion.identity).GetComponent<TextMesh>().text = "ASSIST!";
        }

        PlayerAssists.Add(p);
    }

    //Drop sound
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            FMOD_ControlScript.PlaySoundOneShot(dropItemEventPath, transform.position);
        }
    }
}
