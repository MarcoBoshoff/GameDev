using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using InControl;

public class PlayerScript : MonoBehaviour
{
    private Transform _playerFolder_;
    public Animator myAnimator;
    [HideInInspector]
    public Collider col;
    public SkinnedMeshRenderer myRenderer;

    public bool canFallover = false;

    [HideInInspector]
    public byte PLAYERNUM = 0;
    public string playerName = "Name";
    public TextMesh nameDisplay;
    private float nameDisplayTimer = 2f;
    [HideInInspector]
    public bool gamePad = true; //Switch between KB+M and GamePad

    [HideInInspector]
    public Color myColor;

    //Will change this method later on to be not hardcoded.
    private InputDevice _myInputDevice;

    private NightMenuScript _NightMenuUI;
    private BuySelection _BuyMenuUI;
    
    //Player Hand Control
    public Transform leftHand, rightHand;

    //reference to the shoot deposit
    //public GameObject shootDeposit;

    public float speedModifier = 0.1f;
    private float currentSpeed = 0, weightedSpeed = 0;
    private float distanceToGrabInteractor = 1.1f;
    private bool justGrabbed = false;

    private float knockedDown = 0;

    public LayerMask environmentLayer;

    //FMOD Level Music Variable
    private float _actions = 0; //Tracking Number of player actions. This will be reset during the night

    public float _MaxActionCounter = 1; //0 = Low volume music, 1 = high volume music?
    public float MusicLevelTimer = 10; //How many second until Music Level is updated

    private Vector3 currentDirection = new Vector3();

    private Transform _holding; // What item is being held or pushed
    private ItemScript _holdingScript;
    //private InteractorScript _holdingInteractorScript;
    [SerializeField]
    private QTEScript _qte = null;

    private InteractorScript taggedToMove = null;
    private InteractorScript _closestInteractor;
    [SerializeField]
    private DetectionSphereScript d;
    [HideInInspector]
    public List<ResourceType> restrictedResources = new List<ResourceType>();

    public CustomizeCosumeScript[] outfits;

    [SerializeField]
    public PlayerState myState = PlayerState.Normal;

    private float maxHandStretch = 0.4f;

    //Button values
    private Dictionary<GamepadEnum, ButtonStatus> button_status = new Dictionary<GamepadEnum, ButtonStatus>();
    private bool DPADDown = false, DPADUp = false, DPADLeft = false, DPADRight = false;
    private GamepadEnum[] GamepadEnumArray = new GamepadEnum[] { GamepadEnum.A, GamepadEnum.B, GamepadEnum.X, GamepadEnum.Y };

    //Delays after interacting with a station
    private float delayRegainControl = 0, delayRegainControlMax = 1f;

    //Throwing items and potions
    private int timeThrowingCharge = 30;
    private bool throwing = false, emote = false;
    private int throwingCharge = 0;
    private Vector3 throwLocation = Vector3.zero;
    private ArcScript throwArc = null;
    private GameObject arcPrefab;

    [SerializeField]
    private float JoyStickMenuDelayx, JoyStickMenuDelayy , MenuDelayMax = 0.25f;
    [SerializeField]
    private bool FirstJoystickMoveX = true, FirstJoystickMoveY = true;

    // Sound / FMOD
    [FMODUnity.EventRef]
    public string footstepEventPath = null; // player foostep, to be triggered via animation event
    [FMODUnity.EventRef]
    public string pickupIngredientPath = null, dropIngredientPath = null; // pickup & drop ingredient

    public void SetColourAndCostume(GameControllerScript control, int pNum)
    {
        //Colour
        if (GameControllerScript.PlayerColors == null)
        {
            myColor = control.playerColorsTesting[pNum];
        }
        else
        {
            myColor = GameControllerScript.PlayerColors[pNum];
        }
        //myRenderer.material.color = myColor;

        //Costume
        if (GameControllerScript.PlayerOutfits != null)
        {
            int outfitNum = GameControllerScript.PlayerOutfits[pNum];
            for (int i = 0; i < outfits.Length; i++)
            {
                outfits[i].gameObject.SetActive(i == outfitNum);
            }

            outfits[outfitNum].SetColour(myColor, GameControllerScript.playerCostumeMat[pNum]);
        }

        if (GameControllerScript.PlayerNames != null)
        {
            SetName(GameControllerScript.PlayerNames[pNum]);
        }
    }

    //Sets this player's name
    public void SetName(String newName)
    {
        playerName = newName;
        nameDisplay.text = playerName;
        nameDisplayTimer = 3f;

        Color c = nameDisplay.color;
        c = Color.Lerp(Color.white, myColor, 0.4f);
        nameDisplay.color = c;
    }



    // Start is called before the first frame update
    void Start()
    {
        PLAYERNUM = (byte)(tag.Equals("Player 1") ? 1 : 2);

        _playerFolder_ = transform.parent;
        //_BuyMenuUI.GetComponentInChildren<BuyButtonHandler>().Player = this.gameObject;

        //*****SET COLOUR AND COSTUME*****
        SetColourAndCostume(Camera.main.GetComponent<GameControllerScript>(), PLAYERNUM - 1);
        //********************************

        col = GetComponent<Collider>();

        button_status[GamepadEnum.A] = new ButtonStatus();
        button_status[GamepadEnum.B] = new ButtonStatus();
        button_status[GamepadEnum.X] = new ButtonStatus();
        button_status[GamepadEnum.Y] = new ButtonStatus();

        arcPrefab = Resources.Load("Prefabs/Arc") as GameObject;
    }

    // Update is called once per frame
    void Update()
    {
        if (_NightMenuUI == null)
        {
            _NightMenuUI = GameControllerScript.local.DayEndMenu.GetComponent<NightMenuScript>();
        }
        if (_BuyMenuUI == null)
        {
            _BuyMenuUI = GameControllerScript.local.BUYMENU;
        }
        if (InputManager.Devices != null && (InputManager.Devices.Count >= PLAYERNUM))
        {
            _myInputDevice = InputManager.Devices[PLAYERNUM - 1];
            gamePad = true;
        }
        else
        {
            gamePad = false;
        }


        ButtonInputs();

        if (!GameControllerScript.Paused && knockedDown == 0)
        {
            Interact();
            Menu();
            QuickTimeEvent();

        }
        else
        {
            //Exit QTE if game is paused
            if (_qte != null)
            {
                _qte.StopEarly();
            }
        }

        FindInteractor(); //Find closest interactor

        AlignItemWithHands();
    }

    void FixedUpdate()
    {
        if (!GameControllerScript.Paused && knockedDown == 0 && (_NightMenuUI == null || _NightMenuUI.gameObject.activeSelf == false))
        {
            Movement();
            if (throwing)
            {

                if (throwingCharge < timeThrowingCharge)
                {
                    throwingCharge++;

                    if (throwingCharge >= timeThrowingCharge && throwArc == null)
                    {
                        GameObject temp = Instantiate(arcPrefab, transform.position, Quaternion.identity);
                        temp.transform.SetParent(this.transform);
                        throwArc = temp.GetComponent<ArcScript>();
                        throwArc.ArcColour(myColor);
                        throwArc.Start();
                    }
                }

                if (throwArc != null)
                {
                    throwLocation = GameControllerScript.OtherPlayer(transform).position;
                    //throwLocation = shootDeposit.transform.position;
                    throwArc.SetArc(transform.position, throwLocation, 0);
                }
            }

            //Name disappears after a short amount of time
            if (nameDisplayTimer > 0)
            {
                nameDisplayTimer = Mathf.MoveTowards(nameDisplayTimer, 0, Time.fixedDeltaTime / 2);
                Color c = nameDisplay.color;
                c.a = Mathf.Min(1, nameDisplayTimer);
                nameDisplay.color = c;

                nameDisplay.transform.forward = Camera.main.transform.forward;
            }
        }
        else
        {
            myAnimator.SetBool("IsWalking", false);
        }

        if (knockedDown > 0)
        {
            knockedDown = Mathf.MoveTowards(knockedDown, 0, Time.fixedDeltaTime);
        }
    }

    void ButtonInputs()
    {
        //1,2,3,4 - A, B, X, Y

        //Set button status
        if (gamePad)
        {
            ButtonStatus _a = button_status[GamepadEnum.A];
            _a.buttonDown = _myInputDevice.Action1.IsPressed;
            _a.buttonPressed = _myInputDevice.Action1.WasPressed;
            _a.buttonReleased = _myInputDevice.Action1.WasReleased;

            ButtonStatus _b = button_status[GamepadEnum.B];
            _b.buttonDown = _myInputDevice.Action2.IsPressed;
            _b.buttonPressed = _myInputDevice.Action2.WasPressed;
            _b.buttonReleased = _myInputDevice.Action2.WasReleased;

            ButtonStatus _x = button_status[GamepadEnum.X];
            _x.buttonDown = _myInputDevice.Action3.IsPressed;
            _x.buttonPressed = _myInputDevice.Action3.WasPressed;
            _x.buttonReleased = _myInputDevice.Action3.WasReleased;

            ButtonStatus _y = button_status[GamepadEnum.Y];
            _y.buttonDown = _myInputDevice.Action4.IsPressed;
            _y.buttonPressed = _myInputDevice.Action4.WasPressed;
            _y.buttonReleased = _myInputDevice.Action4.WasReleased;

            DPADDown = _myInputDevice.DPadDown.WasPressed;
            DPADUp = _myInputDevice.DPadUp.WasPressed;
            DPADLeft = _myInputDevice.DPadLeft.WasPressed;
            DPADRight = _myInputDevice.DPadRight.WasPressed;

            //Call tutorial script - only does something if a tutorial prompt is displaying
            if (_myInputDevice.AnyButtonWasPressed) { TutorialScript.PressedAnyKey(); }
        }
        else
        {
            ButtonStatus _a = button_status[GamepadEnum.A];
            _a.buttonDown = (PLAYERNUM == 1) ? Input.GetButton("A1") : Input.GetButton("A2");
            _a.buttonPressed = (PLAYERNUM == 1) ? Input.GetButtonDown("A1") : Input.GetButtonDown("A2");
            _a.buttonReleased = (PLAYERNUM == 1) ? Input.GetButtonUp("A1") : Input.GetButtonUp("A2");

            ButtonStatus _b = button_status[GamepadEnum.B];
            _b.buttonDown = (PLAYERNUM == 1) ? Input.GetButton("B1") : Input.GetButton("B2");
            _b.buttonPressed = (PLAYERNUM == 1) ? Input.GetButtonDown("B1") : Input.GetButtonDown("B2");
            _b.buttonReleased = (PLAYERNUM == 1) ? Input.GetButtonUp("B1") : Input.GetButtonUp("B2");

            ButtonStatus _x = button_status[GamepadEnum.X];
            _x.buttonDown = (PLAYERNUM == 1) ? Input.GetButton("X1") : Input.GetButton("X2");
            _x.buttonPressed = (PLAYERNUM == 1) ? Input.GetButtonDown("X1") : Input.GetButtonDown("X2");
            _x.buttonReleased = (PLAYERNUM == 1) ? Input.GetButtonUp("X1") : Input.GetButtonUp("X2");

            ButtonStatus _y = button_status[GamepadEnum.Y];
            _y.buttonDown = (PLAYERNUM == 1) ? Input.GetButton("Menu1") : Input.GetButton("Menu2");
            _y.buttonPressed = (PLAYERNUM == 1) ? Input.GetButtonDown("Menu1") : Input.GetButtonDown("Menu2");
            _y.buttonReleased = (PLAYERNUM == 1) ? Input.GetButtonUp("Menu1") : Input.GetButtonUp("Menu2");

            //Call tutorial script - only does something if a tutorial prompt is displaying
            if (Input.anyKeyDown) { TutorialScript.PressedAnyKey(); }
        }

        justGrabbed = false;
    }

    //character movement method
    float movex = 0;
    float movez = 0;
    void Movement()
    {       
        if (gamePad == true)
        {         //axis
            movex = _myInputDevice.LeftStickX;
            movez = _myInputDevice.LeftStickY;
        }
        else
        {
            movex = (PLAYERNUM == 1) ? Input.GetAxis("Horizontal") : Input.GetAxis("Horizontal2");
            movez = (PLAYERNUM == 1) ? Input.GetAxis("Vertical") : Input.GetAxis("Vertical2");
        }
        if (myState != PlayerState.QTE)
        {
            if ((_BuyMenuUI == null || _BuyMenuUI.gameObject.activeSelf == false) && (_NightMenuUI == null || _NightMenuUI.gameObject.activeSelf == false) && delayRegainControl < 0.2f)
            {
                ApplyMovement(movex, movez); //Applies this x & z direction to move the player
                ApplyMovement(movex, movez); //Applies this x & z direction to move the player
            }

            if (delayRegainControl > 0)
            {
                delayRegainControl = Mathf.MoveTowards(delayRegainControl, 0, Time.fixedDeltaTime);
            }
        }
        else if (_qte != null)
        {
            delayRegainControl = delayRegainControlMax;
            _qte.MovementInput(this, new Vector2(movex, movez)); //Pass movement inputs into the Quick Time Event
        }
    }

    public void ApplyMovement(float x, float y)
    {
        //speed calculation
        currentSpeed = Mathf.MoveTowards(currentSpeed, Mathf.Min(Mathf.Abs(x) + Mathf.Abs(y), 1.1f), 0.1f);

        //movement calculation (for x and z axis)
        currentDirection.x = Mathf.MoveTowards(currentDirection.x, x, 0.1f);
        currentDirection.z = Mathf.MoveTowards(currentDirection.z, y, 0.1f);

        //character speed
        if (_holding == null)
        {
            weightedSpeed = currentSpeed * speedModifier;
        }
        else
        {
            weightedSpeed = (currentSpeed * speedModifier) / _holdingScript.Weight;
        }

        //Debug.Log("weightedSpeed = " + weightedSpeed);
        weightedSpeed *= Time.fixedDeltaTime * Time.timeScale;
        myAnimator.SetFloat("WalkSpeed", weightedSpeed*60);

        //moving character
        //if (myState == PlayerState.MovingInteractor)
        //{
        //    _holdingInteractorScript.AddForce(currentDirection.normalized * weightedSpeed, transform.position);
        //}
        //else if (myState == PlayerState.Pushing)
        if (myState == PlayerState.Pushing)
        {
            ((HeavyScript)_holdingScript).AddForce((currentDirection.normalized * weightedSpeed), transform.position);
        }
        else
        {
            transform.Translate(currentDirection.normalized * weightedSpeed, Space.World);
        }


        //character rotation
        if (currentDirection != Vector3.zero && currentSpeed >= 0.01f)
        {
            emote = false; //Cancel Emote
            //^ Freezes character rotation if pushing object
            transform.forward = currentDirection.normalized;
            myAnimator.SetBool("IsWalking", true);
            myAnimator.SetBool("IsCasting", false);
            myAnimator.SetBool("IsDancing", false);
            myAnimator.SetBool("IsWaving", false);
            //Debug.Log("Setting transform.forward");

        }
        else if (currentSpeed < 0.2f)
        {
            myAnimator.SetBool("IsWalking", false);
        }
    }

    void Interact()
    {
        if ((myState == PlayerState.Normal) && (_BuyMenuUI.gameObject.activeSelf == false && _NightMenuUI.gameObject.activeSelf == false) && delayRegainControl == 0)
        {
            if (DPADDown)
            {
                emote = true;
                myAnimator.SetBool("IsWaving", true);
                myAnimator.SetBool("IsCasting", false);
                myAnimator.SetBool("IsDancing", false);
            }
            if (DPADUp)
            {
                emote = true;
                myAnimator.SetBool("IsDancing", true);
                myAnimator.SetBool("IsCasting", false);
                myAnimator.SetBool("IsWaving", false);
            }
            if (DPADLeft)
            {
                emote = true;
                myAnimator.SetBool("IsCasting", true);
                myAnimator.SetBool("IsDancing", false);
                myAnimator.SetBool("IsWaving", false);
            }
            if (DPADRight)
            {
                emote = true;
                myAnimator.SetTrigger("Hug");
                myAnimator.SetBool("IsCasting", false);
                myAnimator.SetBool("IsDancing", false);
                myAnimator.SetBool("IsWaving", false);
            }
            if (emote == false)
            {
                myAnimator.SetBool("IsCasting", false);
                myAnimator.SetBool("IsDancing", false);
                myAnimator.SetBool("IsWaving", false);
            }
        }

        //**********************
        //GRABBING & INTERACTING
        //**********************
        if (myState != PlayerState.QTE && !_BuyMenuUI.gameObject.activeSelf)
        {

            if (delayRegainControl == 0)
            {

                bool actionTaken = false; //Whether an action has been taken this update or not

                //*******************
                //*** INTERACTION ***
                //*******************
                if ((button_status[GamepadEnum.A].buttonPressed
                    || button_status[GamepadEnum.X].buttonPressed) && _closestInteractor != null && _holdingScript == null)
                {
                    if (GameControllerScript.NightProgress != 1 || _closestInteractor.canBeMoved == false)
                    {
                        SetMyQTE(_closestInteractor.InterfaceInteractor(true, this)); // == GamepadEnum.A or GamepadEnum.X

                        if (QTE != null) { actionTaken = true; }
                    }
                }

                //******************
                //**** GRABBING ****
                //******************
                if (!actionTaken && _holding == null && button_status[GamepadEnum.A].buttonPressed)
                {
                    //Grab an item
                    if (GameControllerScript.NightProgress != 1)
                    {
                        ItemScript closestItem = d.nearbyItems.ClosestItem(transform.position, 2.1f, restrictedResources);
                        if (closestItem != null)
                        {
                            ActionCounter(1);

                            if (closestItem.Heavy)
                            {
                                HeavyScript hs = (HeavyScript)closestItem;
                                GrabItem(closestItem);
                                actionTaken = true;
                                Debug.Log("Grabbing A Heavy Item");
                            }
                            else
                            {
                                GrabItem(closestItem);
                                actionTaken = true;
                                Debug.Log("Grabbing An Item");
                            }
                        }
                    }

                    //If still holding nothing, try grabbing an interactor
                    //if (!actionTaken && GameControllerScript.NightProgress != 0)
                    //{
                    //    if (_closestInteractor != null && (_BuyMenuUI.gameObject.activeSelf == false))
                    //    {
                    //        if (_closestInteractor.canBeMoved)
                    //        {
                    //            Debug.Log("Tag Interactor To Move At Night");

                    //            if (taggedToMove == null)
                    //            {
                    //                InteractorScript tempTag = _closestInteractor.TagInteractorMove(-1);
                    //                if (tempTag != null)
                    //                {
                    //                    taggedToMove = tempTag;
                    //                }

                    //            }
                    //            else if (taggedToMove != null)
                    //            {
                    //                if (taggedToMove == _closestInteractor)
                    //                {
                    //                    _closestInteractor.UntagInteractor();
                    //                }
                    //                else
                    //                {
                    //                    InteractorScript tempTag = _closestInteractor.TagInteractorMove(taggedToMove.GridSpacesNeeded);

                    //                    if (tempTag != null)
                    //                    {
                    //                        //Swap the two stations
                    //                        gameObject.AddComponent<SwapInteractorsScript>().Swap(taggedToMove, tempTag);
                    //                        taggedToMove = null;
                    //                    }
                    //                }

                    //            }
                    //            actionTaken = true;
                    //        }
                    //    }
                    //}
                }


                //*******************
                //** STORING ITEMS **
                //*******************
                if (!actionTaken && _holdingScript != null)
                {
                    //Store holding item if applicable
                    if (_closestInteractor != null && button_status[GamepadEnum.A].buttonPressed
                        && _closestInteractor.StoreRequest(_holdingScript, this))
                    {
                        ActionCounter(1);

                        Debug.Log("Stored an item!");

                        GameObject stored = _holding.gameObject;
                        ReleaseItem();
                        Destroy(stored); //Delete item if it was successfully stored
                        actionTaken = true;
                    }

                    //*******************************
                    //*** DROP / THROW / REMOVING ***
                    //*******************************
                    else if (button_status[GamepadEnum.A].buttonPressed)
                    {
                        if (_holdingScript != null)
                        {
                            if (_holdingScript.Heavy)
                            {
                                ReleaseItem();
                            }
                            else
                            {
                                Debug.Log("Begin Throwing!");

                                if (_holdingScript._weight > 1)
                                {
                                    ReleaseItem();
                                }
                                else
                                {
                                    throwing = true;
                                }
                                actionTaken = true;
                            }
                        }
                    }
                }

                //Do throwing or dropping
                if (!actionTaken && button_status[GamepadEnum.A].buttonReleased && throwing)
                {
                    throwing = false;

                    if (throwingCharge >= timeThrowingCharge)
                    {
                        ThrowItem();
                    }
                    else
                    {
                        Debug.Log("Drop item!");
                        ReleaseItem();
                    }

                    throwingCharge = 0;

                    if (throwArc != null)
                    {
                        Destroy(throwArc.gameObject);
                        throwArc = null;
                    }

                    //FMOD_ControlScript.PlaySoundOneShot(dropIngredientPath, transform.position);
                    FMOD_ControlScript.PlaySoundOneShot(FMOD_ControlScript.throwSoundEventPath, transform.position);
                }

                //Release grip if too far away
                if (_holdingScript != null)
                {
                    float distance = Vector3.Distance(transform.position, _holdingScript.ClosestPoint(transform.position));
                    if (GameControllerScript.DayNight.nightAmount == 1 || distance > 3f || _holdingScript.busy)
                    {
                        Debug.Log("Release an item!");
                        ReleaseItem();
                    }
                    else if (!_holdingScript.transform.IsChildOf(transform) && !_holdingScript.Heavy)
                    {
                        _holding = null;
                        _holdingScript = null;
                    }
                }

                //If the player was assigned a QTE, face towards it
                if (_qte != null)
                {
                    myState = PlayerState.QTE;

                    ActionCounter(1);

                    Vector3 newFace = _closestInteractor.transform.position;
                    newFace.y = transform.position.y;
                    transform.forward = newFace - transform.position; //Face interactor
                }
            }
            else
            {
                //Still throw without movement
                if (_holdingScript != null && button_status[GamepadEnum.A].buttonPressed)
                {
                    if (_holdingScript != null)
                    {
                        if (!_holdingScript.Heavy &&_holdingScript._weight <= 1)
                        {
                            throwing = true;
                        }
                    }
                }
                else if (_holdingScript != null && button_status[GamepadEnum.A].buttonReleased && throwing)
                {
                    throwing = false;

                    if (throwingCharge >= timeThrowingCharge)
                    {
                        ThrowItem();
                    }

                    throwingCharge = 0;

                    if (throwArc != null)
                    {
                        Destroy(throwArc.gameObject);
                        throwArc = null;
                    }
                }
            }
        }

        if (_holding == null)
        {
            throwing = false;
        }
    }

    void Menu()
    {
        //If buy menu is open
        if (_BuyMenuUI.gameObject.activeSelf)
        {
            if (movex >= 0.5 || movex <= -0.5)
            {
                if (JoyStickMenuDelayx < MenuDelayMax)
                {
                    JoyStickMenuDelayx += 1 * Time.deltaTime;
                }
            }
            if (movez >= 0.5 || movez <= -0.5)
            {
                if (JoyStickMenuDelayy < MenuDelayMax)
                {
                    JoyStickMenuDelayy += 1 * Time.deltaTime;
                }
            }
            bool rangeStickX = JoyStickMenuDelayx >= MenuDelayMax;
            bool rangeStickY = JoyStickMenuDelayy >= MenuDelayMax;
            //Move
            //0 = up, 1 = right, 2 = down, 3 = left
            bool goLeft = false, goRight = false;

            if (gamePad)
            {
                goRight = movex >= 0.5f && (rangeStickX || FirstJoystickMoveX == true);
                goLeft = movex <= -0.5f && (rangeStickX || FirstJoystickMoveX == true);

            }
            else
            {
                goRight = (PLAYERNUM == 1) ? Input.GetKeyDown(KeyCode.D) : Input.GetKeyDown(KeyCode.RightArrow);
                goLeft = (PLAYERNUM == 1) ? Input.GetKeyDown(KeyCode.A) : Input.GetKeyDown(KeyCode.LeftArrow);
            }


            if (goRight)
            {
                FirstJoystickMoveX = false;
                JoyStickMenuDelayx = 0;
                _BuyMenuUI.MenuMove(true, PLAYERNUM);
            }
            else if (goLeft)
            {
                _BuyMenuUI.MenuMove(false, PLAYERNUM);
                FirstJoystickMoveX = false;
                JoyStickMenuDelayx = 0;
                    
            }
            else
            {
                if (movex > -0.5 && movex < 0.5)
                {
                    FirstJoystickMoveX = true;
                }
                if (movez > -0.5 && movez < 0.5)
                {
                    FirstJoystickMoveY = true;
                }
            }

            //Buy
            if (button_status[GamepadEnum.A].buttonPressed)
            {
                BuySelection BuyMenu = _BuyMenuUI.GetComponent<BuySelection>();
                _BuyMenuUI.MenuSelect(BuyMenu.CatalogButtons[BuyMenu.buttonSelected[PLAYERNUM-1]], PLAYERNUM);
            }
            else if (button_status[GamepadEnum.B].buttonPressed)
            {
                _BuyMenuUI.gameObject.SetActive(false);
                GameControllerScript.local.DayEndMenu.SetActive(true);
                _NightMenuUI.UpdatePotionIcons();

                // cancel audio
                FMOD_ControlScript.CancelSelection();
            }
        }
    }


    /// <summary>
    /// Finds nearest interactor. Returns whether the interactor reference was updated.
    /// </summary>
    /// <returns></returns>
    private void FindInteractor()
    {
        InteractorScript temp = null;
        if (d.myInteractors.Count > 0)
        {
            float tempDist = distanceToGrabInteractor;
            foreach (InteractorScript interactor in d.myInteractors)
            {
                if (interactor == null)
                {
                    d.myInteractors.Remove(interactor);
                }
                else if (interactor.gameObject == null)
                {
                    d.myInteractors.Remove(interactor);
                }
                else
                {
                    float dist = Vector3.Distance(transform.position, interactor.ClosestPoint(transform.position));
                    if (dist < tempDist)
                    {
                        tempDist = dist;
                        temp = interactor;
                    }
                }
            }
        }

        if (temp != _closestInteractor && _closestInteractor != null)
        {
            _closestInteractor.RemovePlayer(this); //Remove the available player from the old interactor
        }

        if (_closestInteractor != temp)
        {
            _closestInteractor = temp;

            if (_closestInteractor != null)
            {
                _closestInteractor.AddPlayer(this); //Add this player as a new available player for the interactor
            }
        }
    }

    private void QuickTimeEvent()
    {
        if (myState == PlayerState.QTE)
        {
            if (_qte == null)
            {
                myState = PlayerState.Normal; //If no qte obj is attached, return to normal state
            }
            else
            {
                //TODO: Handle other quick time event interactions
                if (button_status[GamepadEnum.B].buttonPressed)
                {
                    _qte.StopEarly();
                }

                if (button_status[GamepadEnum.A].buttonDown)
                {
                    _qte.FireHeld(this, 0);
                }
                if (button_status[GamepadEnum.X].buttonDown)
                {
                    _qte.FireHeld(this, 1);
                }

                foreach (GamepadEnum btn in GamepadEnumArray)
                {
                    if (button_status[btn].buttonPressed)
                    {
                        _qte.ButtonPressed(this, btn);
                    }
                }

            }
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (canFallover)
        {
            if (collision.gameObject.layer == 9 && knockedDown == 0)
            {
                if (currentSpeed >= 0.9f && collision.gameObject.GetComponent<PlayerScript>().currentSpeed >= 0.9f)
                {
                    transform.forward = (collision.gameObject.transform.position - gameObject.transform.position).normalized;

                    knockedDown = 2.12f;
                    myAnimator.SetTrigger("KnockedDown");
                }
            }
        }
    }

    //public void GrabInteractor(InteractorScript interactor, bool bought)
    //{
    //    Vector3 facing = interactor.transform.position;
    //    facing.y = transform.position.y;
    //    transform.forward = facing - transform.position;

    //    interactor.MoveInteractor(this, bought);
    //    //if (bought == true)
    //    //{
    //    //    _BuyMenuUI.selected = false;
    //    //    _BuyMenuUI.IconUpdate(true,false);
    //    //}

    //    _holding = interactor.transform;
    //    _holdingInteractorScript = interactor;

    //    this.transform.SetParent(interactor.transform);

    //    myState = PlayerState.MovingInteractor;
    //    justGrabbed = true;
    //}

    public void GrabItem(ItemScript item)
    {
        //Prevent Items from being held while emoting
        emote = false;

        // Plays Pickup Sound
        FMOD_ControlScript.PlaySoundOneShot(pickupIngredientPath, transform.position);

        Vector3 facing = item.transform.position;
        facing.y = transform.position.y;
        transform.forward = (facing - transform.position);

        bool isHeavyItem = item.Heavy;

        item.Grabbing(true, this);
        //Set the Item as a child of Player
        if (isHeavyItem)
        {
            this.transform.SetParent(item.transform);
            myAnimator.SetTrigger("PickupHeavy");
            myAnimator.SetBool("IsCarrying", true);
        }
        else
        {
            //Call Held method in Object. Set to True to tell object it is being held
         
            item.transform.SetParent(this.transform);

            myAnimator.SetTrigger("Pickup");
            myAnimator.SetBool("IsCarrying", true);
        }


        //Set hit objecty to the Holding variable
        _holding = item.transform;
        _holdingScript = item;

        myState = (isHeavyItem) ? PlayerState.Pushing : PlayerState.Carrying;
        justGrabbed = true;

        //Updates interactor on new stats
        UpdateInteractorStats();
    }

    public void AlignItemWithHands()
    {
        if (_holdingScript != null && !_holdingScript.Heavy)
        {
            Vector3 pos;

            switch (_holdingScript.resourceCompound.resourceType)
            {
                case ResourceType.Flower:
                    pos = rightHand.position;
                    break;
                case ResourceType.Mushroom:
                    pos = (rightHand.position + leftHand.position) / 2 + Vector3.up * 0.7f;
                    break;
                default:
                    pos = (rightHand.position + leftHand.position) / 2;
                    break;
            }
            //movement Speed to Players hands 
            _holding.position = Vector3.MoveTowards(_holding.position, pos, 3f * Time.deltaTime);
        }
    }

    public void SetMyQTE(QTEScript newQTE)
    {
        myAnimator.SetBool("IsWalking", false); //Make sure walking animation doesn't contine
        _qte = newQTE;
    }

    //Also found in Cauldron Script and DepositChuteScript
    public void ActionCounter(int increment)
    {
        _actions += increment;
        GameControllerScript.AddActionsThisMinute(tag.Equals("Player 1"), increment);
    }

    //public void DestroyInteractor()
    //{
    //    //Returns to normal state
    //    myState = PlayerState.Normal;

    //    this.transform.SetParent(_playerFolder_);

    //    //Tell Holdable object that it is no longer being held.

    //    d.myInteractors.Remove(_holdingInteractorScript);

    //    Destroy(_holding.gameObject);

    //    Destroy(_holdingInteractorScript.SnapObj);
    //    _holdingInteractorScript = null;

    //    _holding = null;
    //}

    public void ReleaseItem()
    {
        //Prevent Items from being held while emoting
        emote = false;

        if (_holdingScript == null)
        {
            myState = PlayerState.Normal;
            return;
        }


        _holdingScript.Grabbing(false, this);

        //raycast to see if held item is clipping through environment
        Ray itemRay = new Ray(_holding.position, transform.forward);

        //Item is no longer child of Player
        if (myState == PlayerState.Pushing)
        {
            this.transform.SetParent(_playerFolder_);
        }
        else
        {
            if (_holdingScript.transform.IsChildOf(transform))
            {
                _holding.transform.parent = null;
            }

            //if clipping, places item above player's head
            if (Physics.Raycast(itemRay, 10, environmentLayer))
            {
                _holding.position = transform.position + transform.up;
            }
        }

        //Remove Item from Holding
        _holdingScript = null;

        _holding = null;


        //Returns to normal state
        myState = PlayerState.Normal;

        //Updates interactor on new stats
        UpdateInteractorStats();

        //Stop carrying
        myAnimator.SetBool("IsCarrying", false);
    }

    public void ThrowItem()
    {
        // Player prepares to throw sfx

        if (throwArc == null || throwArc.line == null)
        {
            ReleaseItem();
            return;
        }

        //Tell Holdable object that it is no longer being held.
        _holdingScript.Grabbing(false, this);

        //raycast to see if held item is clipping through environment
        Ray itemRay = new Ray(_holding.position, transform.forward);

        //Item is no longer child of Player
        if (myState == PlayerState.Pushing)
        {
            this.transform.SetParent(_playerFolder_);
        }
        else
        {
            if (_holdingScript.transform.IsChildOf(transform))
            {
                _holding.transform.parent = null;
            }
        }

        //Vector3 middle = (throwLocation - transform.position) / 2 + Vector3.up * 4;
        //_holding.GetComponent<Rigidbody>().AddForce(middle*2, ForceMode.Impulse);
        FollowArcScript follow = _holding.gameObject.AddComponent<FollowArcScript>();
        _holdingScript.busy = true;
        _holdingScript.GetComponent<Collider>().enabled = false;
        List<Vector3> p = new List<Vector3>();
        int count = throwArc.line.positionCount - 1;
        for (int i = 0; i < count; i++)
        {
            p.Add(throwArc.line.GetPosition(i));
        }
        follow.points = p;
        follow.myItem = _holdingScript;

        Rigidbody rb = _holdingScript.GetComponent<Rigidbody>();
        if (rb != null){ rb.isKinematic = true; }
        
        //Remove Item from Holding
        _holdingScript = null;
        _holding = null;


        //Returns to normal state
        myState = PlayerState.Normal;

        //Updates interactor on new stats
        UpdateInteractorStats();

        //Stop carrying
        myAnimator.SetBool("IsCarrying", false);
    }

    private void UpdateInteractorStats()
    {
        if (_closestInteractor != null)
        {
            _closestInteractor.UpdateStats(this);
        }
    }

    public Transform Holding
    {
        get { return _holding; }
        set { _holding = value; }
    }
    public BuySelection MenuUI
    {
        get { return _BuyMenuUI; }
        set { _BuyMenuUI = value; }
    }
    public InputDevice myInputDevice
    {
        get { return _myInputDevice; }
        set { _myInputDevice = value; }
    }
    public QTEScript QTE
    {
        get { return _qte; }
        set { _qte = value; }
    }
    public DetectionSphereScript D
    {
        get
        {
            return d;
        }
        set
        {
            d = value;
        }
    }
    public Transform PlayerFolder
    {
        get { return _playerFolder_; }
    }


    public bool IsHolding()
    {
        return _holding != null;
    }

    private class ButtonStatus
    {
        public bool buttonPressed = false, buttonDown = false, buttonReleased = false;
    }

    // FMOD / SOUND **************************

    // Plays walking sound, called from walking animation event
    public void PlayFootstep()
    {
        FMOD_ControlScript.PlaySoundOneShot(footstepEventPath, transform.position);
    }

    // ******************************************
}

[System.Serializable]
public enum PlayerState
{
    Normal, Carrying, Pushing, MovingInteractor, QTE
};

[System.Serializable]
public enum GamepadEnum
{
    A, B, X, Y //R, Q, E, 1 on keyboard respectively
}



// Matthew C = This town ain't big enough for 2 'Pro...' Matts