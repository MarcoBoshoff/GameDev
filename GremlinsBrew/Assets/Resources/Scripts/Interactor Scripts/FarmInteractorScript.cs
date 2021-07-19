using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FarmInteractorScript : InteractorScript
{
    [SerializeField]
    GameObject itemToSpawn; //Which item this interactor spawns
    [SerializeField]
    ResourceType[] associatedPotions; //Which potions it is needed for (for tutorial purposes)
    private ItemScript myItem;
    public float spawnOffset = 0; //Used to align ingredient to spawner
    private float timer = 1f;
    private bool firstSpawn = true;

    public bool currentlyActive = false; //Whether currently spawning (updated by buy menu)

    public SpriteRenderer APrompt;

    void Start()
    {
        Init(Interactables.Farm, "Prefabs/GridSystem/SnapCauldron", 1); //## Make sure init is called in the start method (used for interactor setup)
    }

    public void UpdateCurrentlyActive()
    {
        currentlyActive = false;
        Debug.Log(name + "Checking:");
        foreach (ResourceType r in associatedPotions)
        {
            Debug.Log(" -- r = " + r.ToString());
            if (GameControllerScript.local.currentUnlockedPotion.Contains(r))
            {
                currentlyActive = true;
                Debug.Log("contains it. currentActive = " + currentlyActive);
                break;
            }
        }
    }

    protected override void OverridenUpdate()
    {
        if (myItem == null)
        {
            if (timer > 0)
            {
                timer -= Time.deltaTime;
            }
            else if (GameControllerScript.NightProgress == 0 && currentlyActive && (GameControllerScript.DayNight.IsDawn == false || GameControllerScript.DayNight.dawnFocus.Contains(associatedPotions[0])))
            {
                timer = 2;
                myItem = Instantiate(itemToSpawn, transform.position + (Vector3.up * (1.7f + spawnOffset)), Quaternion.identity).GetComponent<ItemScript>();
                myItem.transform.Translate(new Vector3(0, -myItem.BoxHeight, 0));
                myItem.transform.SetParent(this.transform);
                myItem.GetComponent<Rigidbody>().isKinematic = true;
                myItem.Grow();

                // ensures sound doesn't play on first spawn / at beginning of level
                if (!firstSpawn)
                {
                    // plays respawn sound
                    //FMOD_ControlScript.PlaySoundOneShot(FMOD_ControlScript.respawnSoundPath, myItem.transform.position);
                }

                else
                {
                    firstSpawn = false;
                }

            }
        }
        else if (myItem._grabbed)
        {
            myItem = null;
        }

        //Updates the sprite based on gamepad or keyboard
        if (availablePlayers.Count > 0)
        {
            foreach (KeyValuePair<PlayerScript, PlayerStats> availables in availablePlayers)
            {
                PlayerScript player = availables.Key;
                if (player.gamePad == false)
                {
                    if (player.PLAYERNUM == 1)
                    {
                        APrompt.sprite = GameControllerScript.local.key1Sprite_Interact;
                        break;
                    }
                    else
                    {
                        APrompt.sprite = GameControllerScript.local.key2Sprite_Interact;
                        break;
                    }
                }

                APrompt.sprite = GameControllerScript.BtnPromptSprite(player,BtnPromptEnum.InteractBtn);
                break;
            }
        }

        Color c = APrompt.color;
        c.a = Mathf.MoveTowards(c.a, (availablePlayers.Count > 0 && myItem != null) ? 1 : 0, Time.fixedDeltaTime);
        APrompt.color = c;
    }


    //## This is important for storing. If you don't wish to store anything, return -1;
    //## Return a 0 or larger if the player can store there
    //## That number (0+) is used to get the prompt from above (storeOptionNames) in Start()
    //## It is also used in the Store function below. It simply sees which option number is being
    //      returned, and performs the related action


    // ## And of course, you can have any method heres that are specific to your interactor. Probably best to make them private
    //     unless other classes need to call them.
}
