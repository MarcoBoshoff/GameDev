using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlassBottleScript : ItemScript
{
    public GameObject HealthBottle;
    public GameObject PoisonBottle;
    public GameObject LoveBottle;
    public GameObject ManaBottle;

    public SpriteRenderer throwButton; //The button to throw the potion
    public int throwDistMax;

    public GameObject ArcPrefab;
    private ArcScript throwArc;

    private ResourceType _storedPotion = ResourceType.Empty;
    private Vector3 pos;

    private bool depositing = false;

    void Start()
    {
        col = GetComponent<Collider>();
        
    }

    void Update()
    {
        Vector3 portalPos = GameControllerScript.local.DepositChute.position;

        Color c = throwButton.color;
        if (_grabbed && Vector3.Distance(transform.position, portalPos) < throwDistMax)
        {
            c.a = Mathf.MoveTowards(c.a, 1, 0.05f);

            //Displays the throw arc
            if (throwArc == null)
            {
                GameObject temp = Instantiate(ArcPrefab, transform.position, Quaternion.identity);
                temp.transform.SetParent(this.transform);
                throwArc = temp.GetComponent<ArcScript>();
                throwArc.Start();
                GetComponent<Rigidbody>().useGravity = false;
            }
            else
            {
                Vector3 throwLocation = portalPos;
                throwArc.SetArc(transform.position, throwLocation, 0);
            }
        }
        else
        {
            //No arc to be displayed
            if (throwArc != null)
            {
                Destroy(throwArc.gameObject);
                throwArc = null;
            }
            c.a = Mathf.MoveTowards(c.a, 0, 0.05f);
        }
        throwButton.color = c;

        if (depositing & GetComponent<FollowArcScript>() == null)
        {
            GameControllerScript.local.DepositChute.GetComponent<DepositChuteScript>().CollectItem(this);
            depositing = false;
        }
    }

    public override void Grabbing(bool grabbing, PlayerScript p)
    {
        Debug.Log("Here");
        throwButton.GetComponent<SpriteRenderer>().sprite = GameControllerScript.BtnPromptSprite(p, BtnPromptEnum.InteractBtn);
        if (!grabbing)
        {
            Vector3 portalPos = GameControllerScript.local.DepositChute.position;
            if (throwArc != null)
            {
                FollowArcScript follow = gameObject.AddComponent<FollowArcScript>();
                busy = true;
                GetComponent<Collider>().enabled = false;
                List<Vector3> points = new List<Vector3>();
                int count = throwArc.line.positionCount - 1;
                for (int i = 0; i < count; i++)
                {
                    points.Add(throwArc.line.GetPosition(i));
                }
                follow.points = points;
                follow.myItem = this;

                rb.isKinematic = true;

                depositing = true; //Knows to go into portal after throw is complete
                busy = true;

                // Bottle Throw SFX
                FMOD_ControlScript.PlaySoundOneShot(FMOD_ControlScript.throwSoundEventPath, transform.position);
            }
            else
            {
                Debug.Log("throwArc is null :(");
                rb.isKinematic = false;
            }

            p.myAnimator.SetTrigger("Throw");
            //playersHoldingMe.Remove(p);
        }

        _grabbed = grabbing;

        Debug.Log($"_grabbed = {_grabbed}");
    }

    private void SetMesh()
    {
        pos = transform.position;

        //Enables appropriate potion bottle model
        switch (resourceCompound.resourceType)
        {
            case ResourceType.HealthPotion:
                {
                    HealthBottle.SetActive(true);
                    break;
                }
            case ResourceType.PoisonPotion:
                {
                    PoisonBottle.SetActive(true);
                    break;
                }
            case ResourceType.LovePotion:
                {
                    LoveBottle.SetActive(true);
                    break;
                }
            case ResourceType.ManaPotion:
                {
                    ManaBottle.SetActive(true);
                    break;
                }
            default:
                {
                    HealthBottle.SetActive(true);
                    break;
                }
        }
    }

    //Property
    public ResourceType StoredPotion
    {
        get
        {
            return _storedPotion;
        }

        set
        {
            _storedPotion = value;
            resourceCompound.resourceType = _storedPotion;
            resourceCompound.resourceEffect = ResourceEffect.Normal;
            SetMesh();
        }
    }
}
