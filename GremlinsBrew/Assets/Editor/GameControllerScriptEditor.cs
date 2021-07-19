using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(GameControllerScript))]
public class GameControllerScriptEditor : Editor
{
    SerializedProperty _dynamicCamera, _cameraDistance, _poisonPrice, _lovePrice, _manaPrice, _fourthPotionPrice, _tutorialActive;
    SerializedProperty _startCoins, _startLives, _dayCap, _maxPotions;
    SerializedProperty _redFlash1, _redFlash0;
    SerializedProperty _highlightInteractor, _highlightResource;
    SerializedProperty _environmentLayer, _wallInvisLayer;
    SerializedProperty _btnSpriteInteract, _key1SpriteInteract, _key2SpriteInteract, _PSSprite_Interact;
    SerializedProperty _playerColors;
    SerializedProperty _vendingControl, _coinCounter, _buymenu, _dayendmenu,_paxgameover , _player1, _player2, _overlayCamera, _depositChute, _qteuiParent, _booksParent;
    SerializedProperty _bedScript1, _bedScript2, _pauseMenu, _UI;
    SerializedProperty _camShakeMagnitude, _camShakeRough, _camShakeFadeout;
    SerializedProperty _gridTopLeft, _gridSpacing, _gridIndentAt, _customGrid, _customGridSize, _startingInteractors;
    SerializedProperty _allInteractors;

    public static bool[,] tempArray;
    public bool builtArray = false, showInteractors = false;

    public int MachineStatMode, GridMode = 0, RefMode = 0, ControlsMode = 0;
    private static string[] gridModeOptions = new string[] { "Grid Layout", "Interactor Placement" };
    private static string[] playerOptions = new string[] { "Colors for Testing" };
    private static string[] refModeOptions = new string[] { "Player References", "Game Controller References", "Layer References", "Sprites", "-ALL-" };
    private static string[] machineStatOptions = new string[] { "Constants", "Recipe Prices", "Camera", "Lighting and Highlighs", "Tutorial" };

    private static bool SHOWMACHINE = false, SHOWPLAYERSTUFF = false, SHOWREFERENCES = false, SHOWSHAKE = false, SHOWGRID = false;

    protected virtual void OnEnable()
    {
        _allInteractors = this.serializedObject.FindProperty("existingInteractors");

        //Machine Stats
        _dynamicCamera = this.serializedObject.FindProperty("dynamicCamera");
        _cameraDistance = this.serializedObject.FindProperty("cameraDistance");
        _startLives = this.serializedObject.FindProperty("STARTLIVES");
        _poisonPrice = this.serializedObject.FindProperty("PoisonPotionPrice");
        _lovePrice = this.serializedObject.FindProperty("LovePotionPrice");
        _manaPrice = this.serializedObject.FindProperty("ManaPotionPrice");
        _fourthPotionPrice = this.serializedObject.FindProperty("FourthPotionPrice");

        _startCoins = this.serializedObject.FindProperty("startCoins");
        _dayCap = this.serializedObject.FindProperty("DayCap");
        _maxPotions = this.serializedObject.FindProperty("maxPotions");
        _tutorialActive = this.serializedObject.FindProperty("tutorialActive");
        _redFlash1 = this.serializedObject.FindProperty("redFlash1"); //Lantern Flashing red
        _redFlash0 = this.serializedObject.FindProperty("redFlash0"); //Lantern Flashing black
        _highlightInteractor = this.serializedObject.FindProperty("interactorHighlight");
        _highlightResource = this.serializedObject.FindProperty("resourceHighlight");

        //References
        _vendingControl = this.serializedObject.FindProperty("vendingControlScript");
        _UI = this.serializedObject.FindProperty("UI");
        _coinCounter = this.serializedObject.FindProperty("coinCounter");
        _buymenu = this.serializedObject.FindProperty("BUYMENU");
        _player1 = this.serializedObject.FindProperty("Player1");
        _player2 = this.serializedObject.FindProperty("Player2");
        _overlayCamera = this.serializedObject.FindProperty("overlayCamera");
        _depositChute = this.serializedObject.FindProperty("DepositChute");
        _bedScript1 = this.serializedObject.FindProperty("bedScript1");
        _bedScript2 = this.serializedObject.FindProperty("bedScript2");
        _booksParent = this.serializedObject.FindProperty("BooksParent");
        _qteuiParent = this.serializedObject.FindProperty("QTEUIParent");
        _pauseMenu = this.serializedObject.FindProperty("PauseMenu");
        _dayendmenu = this.serializedObject.FindProperty("DayEndMenu");
        _paxgameover = this.serializedObject.FindProperty("PaxGameOver");
        
        //Layers
        _environmentLayer = this.serializedObject.FindProperty("environmentLayer");
        _wallInvisLayer = this.serializedObject.FindProperty("wallInvisLayer");

        //Sprites
        _btnSpriteInteract = this.serializedObject.FindProperty("btnSprite_Interact");

        _key1SpriteInteract = this.serializedObject.FindProperty("key1Sprite_Interact");

        _key2SpriteInteract = this.serializedObject.FindProperty("key2Sprite_Interact");
        _PSSprite_Interact  = this.serializedObject.FindProperty("PSSprite_Interact");
        

        //Players
        _playerColors = this.serializedObject.FindProperty("playerColorsTesting");

        //Camera shake
        _camShakeMagnitude = this.serializedObject.FindProperty("shake_distance");
        _camShakeRough = this.serializedObject.FindProperty("shake_roughness");
        _camShakeFadeout = this.serializedObject.FindProperty("shake_fadeout");

        //Grid system
        _gridTopLeft = this.serializedObject.FindProperty("gridTopLeft");
        _gridSpacing = this.serializedObject.FindProperty("gridSpacing");
        _gridIndentAt = this.serializedObject.FindProperty("gridIndentAt");
        _customGrid = this.serializedObject.FindProperty("CustomGrid");
        _customGridSize = this.serializedObject.FindProperty("cgSize");
        _startingInteractors = this.serializedObject.FindProperty("StartingInteractors");

        if (tempArray == null)
        {
            tempArray = new bool[100, 100];
            GameControllerScript baseScript = (GameControllerScript)target;
            Vector2Int size = baseScript.cgSize;

            for (int y = 0; y < size.y; y++)
            {
                for (int x = 0; x < size.x; x++)
                {
                    int i = x + (y * size.x);

                    tempArray[x, y] = _customGrid.GetArrayElementAtIndex(i).boolValue;
                }
            }
        }
    }

    public override void OnInspectorGUI()
    {
        this.serializedObject.Update();
        GameControllerScript baseScript = (GameControllerScript)target;        

        EditorGUILayout.LabelField("Select sections to modify");

        SHOWMACHINE = EditorGUILayout.Toggle("[Machine Stats]", SHOWMACHINE);
        SHOWPLAYERSTUFF = EditorGUILayout.Toggle("[Players & Input]", SHOWPLAYERSTUFF);
        SHOWREFERENCES = EditorGUILayout.Toggle("[References]", SHOWREFERENCES);
        SHOWSHAKE = EditorGUILayout.Toggle("[Camera Shake]", SHOWSHAKE);
        SHOWGRID = EditorGUILayout.Toggle("[Grid System]", SHOWGRID);

        if (SHOWMACHINE)
        {
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            EditorGUILayout.LabelField("Machine Stats", EditorStyles.boldLabel);

            MachineStatMode = EditorGUILayout.Popup(MachineStatMode, machineStatOptions);
            EditorGUILayout.Separator();

            switch (MachineStatMode)
            {
                case 0:
                    _maxPotions.intValue = EditorGUILayout.IntField("Max Potions", _maxPotions.intValue);
                    _startCoins.intValue = EditorGUILayout.IntField("Start Coins", _startCoins.intValue);
                    _dayCap.intValue = EditorGUILayout.IntField("Day Cap", _dayCap.intValue);
                    _startLives.intValue = EditorGUILayout.IntField("Start Lives", _startLives.intValue);
                    if (GUILayout.Button("Damage Machine"))
                    {
                        GameControllerScript.Damage();
                    }
                    break;
                case 1:
                    _fourthPotionPrice.floatValue = EditorGUILayout.FloatField("Health Potion Price", _fourthPotionPrice.floatValue);
                    _poisonPrice.floatValue = EditorGUILayout.FloatField("Poison Potion Recipe", _poisonPrice.floatValue);
                    _lovePrice.floatValue = EditorGUILayout.FloatField("Love Potion Recipe", _lovePrice.floatValue);
                    _manaPrice.floatValue = EditorGUILayout.FloatField("Mana Potion Recipe", _manaPrice.floatValue);
                    break;
                case 2:
                    _dynamicCamera.boolValue = EditorGUILayout.Toggle("Dynamic camera", _dynamicCamera.boolValue);
                    _cameraDistance.floatValue = EditorGUILayout.Slider(_cameraDistance.floatValue, 5, 30);
                    break;

                case 3:
                    _redFlash1.colorValue = EditorGUILayout.ColorField("(Red) Lantern Light", _redFlash1.colorValue);
                    _redFlash0.colorValue = EditorGUILayout.ColorField("(Red) Lantern Dark", _redFlash0.colorValue);
                    EditorGUILayout.Separator();
                    _highlightInteractor.colorValue = EditorGUILayout.ColorField("HIGHLIGHT Interactor", _highlightInteractor.colorValue);
                    _highlightResource.colorValue = EditorGUILayout.ColorField("HIGHLIGHT Resource", _highlightResource.colorValue);
                    break;

                case 4:
                    _tutorialActive.boolValue = EditorGUILayout.Toggle("Use Tutorial", _tutorialActive.boolValue);
                    break;

            }
        }


        if (SHOWPLAYERSTUFF)
        {
            EditorGUILayout.LabelField("Players & Input", GUI.skin.horizontalSlider);
            EditorGUILayout.LabelField("Cont", EditorStyles.boldLabel);

            ControlsMode = EditorGUILayout.Popup(ControlsMode, playerOptions);
            EditorGUILayout.Separator();

            switch (ControlsMode)
            {
                case 0:
                    _playerColors.arraySize = 2;
                    EditorGUILayout.PropertyField(_playerColors.GetArrayElementAtIndex(0));
                    EditorGUILayout.PropertyField(_playerColors.GetArrayElementAtIndex(1));
                    break;
            }
        }

        //Show references
        if (SHOWREFERENCES)
        {
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            EditorGUILayout.LabelField("References", EditorStyles.boldLabel);

            RefMode = EditorGUILayout.Popup(RefMode, refModeOptions);

            int all = 4;
            if (RefMode == 0 || RefMode == all)
            {
                EditorGUILayout.PropertyField(_player1);
                EditorGUILayout.PropertyField(_player2);
                EditorGUILayout.PropertyField(_buymenu);
                EditorGUILayout.PropertyField(_dayendmenu);
                EditorGUILayout.PropertyField(_paxgameover);
            }
            if (RefMode == 1 || RefMode == all)
            {
                EditorGUILayout.PropertyField(_UI);
                EditorGUILayout.PropertyField(_vendingControl);
                EditorGUILayout.PropertyField(_depositChute);
                EditorGUILayout.PropertyField(_bedScript1);
                EditorGUILayout.PropertyField(_bedScript2);
                EditorGUILayout.PropertyField(_booksParent);
                EditorGUILayout.PropertyField(_qteuiParent);
                EditorGUILayout.PropertyField(_pauseMenu);

                EditorGUILayout.PropertyField(_coinCounter);
                EditorGUILayout.PropertyField(_overlayCamera);
            }
            if (RefMode == 2 || RefMode == all)
            {
                 EditorGUILayout.PropertyField(_environmentLayer);
                 EditorGUILayout.PropertyField(_wallInvisLayer);
            }
            if (RefMode == 3 || RefMode == all)
            {
                EditorGUILayout.PropertyField(_btnSpriteInteract);

                EditorGUILayout.PropertyField(_key1SpriteInteract);

                EditorGUILayout.PropertyField(_key2SpriteInteract);

                EditorGUILayout.PropertyField(_PSSprite_Interact);
                
            }

            //EditorGUILayout.PropertyField(qteType1); //Type of Spawner (1)
        }

        //Camera Shake
        if (SHOWSHAKE)
        {
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            EditorGUILayout.LabelField("Camera Shake", EditorStyles.boldLabel);

            _camShakeMagnitude.floatValue = EditorGUILayout.FloatField("Shake Distance/Magnitude", _camShakeMagnitude.floatValue);
            _camShakeRough.floatValue = EditorGUILayout.FloatField("Shake Roughness", _camShakeRough.floatValue);
            _camShakeFadeout.floatValue = EditorGUILayout.FloatField("Shake Fadeout", _camShakeFadeout.floatValue);

            if (GUILayout.Button("Shake camera"))
            {
                GameControllerScript.ShakeCamera();
            }

            //EditorGUILayout.PropertyField(qteType1); //Type of Spawner (1)
        }

        //Grid system
        if (SHOWGRID)
        {
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            EditorGUILayout.LabelField("Grid System", EditorStyles.boldLabel);
            
            _gridTopLeft.vector2Value = EditorGUILayout.Vector2Field("Top left Grid Point", _gridTopLeft.vector2Value);
            _gridSpacing.intValue = EditorGUILayout.IntSlider("Grid Spacing", _gridSpacing.intValue, 1, 3);
            _gridIndentAt.intValue = EditorGUILayout.IntField("Grid Indent index", _gridIndentAt.intValue);

            EditorGUILayout.Separator();

            _customGridSize.vector2IntValue = EditorGUILayout.Vector2IntField("Custom Grid Size", _customGridSize.vector2IntValue);

            EditorGUILayout.Separator();

            GridMode = EditorGUILayout.Popup(GridMode, gridModeOptions);

            Vector2Int v = _customGridSize.vector2IntValue;

            for (int y = 0; y < v.y; y++)
            {
                EditorGUILayout.BeginHorizontal();
                for (int x = 0; x < v.x; x++)
                {
                    //Adding interactors to the grid
                    if (GridMode == 1)
                    {
                        if (tempArray[x, y])
                        {
                            SerializedProperty element = _startingInteractors.GetArrayElementAtIndex(x + (y * v.x));
                            int t = element.intValue;

                            bool pressed = false;

                            switch (t) {
                                case 0:
                                    pressed = GUILayout.Button("X", GUILayout.Width(20));
                                    break;
                                case 1:
                                    pressed = GUILayout.Button("1", GUILayout.Width(20));
                                    break;
                                case 2:
                                    pressed = GUILayout.Button("2", GUILayout.Width(20));
                                    break;
                                case 3:
                                    pressed = GUILayout.Button("E", GUILayout.Width(20));
                                    break;
                                case 4:
                                    pressed = GUILayout.Button("C", GUILayout.Width(20));
                                    break;
                                case 5:
                                    pressed = GUILayout.Button("H", GUILayout.Width(20));
                                    break;
                                case 6:
                                    pressed = GUILayout.Button("G", GUILayout.Width(20));
                                    break;
                                case 7:
                                    pressed = GUILayout.Button("T", GUILayout.Width(20));
                                    break;
                                default:
                                    break;
                            }

                            if (pressed)
                            {
                                t += 1;
                                if (t >= 8)
                                {
                                    t -= 8;
                                }

                                element.intValue = t;
                            }
                        }
                        else
                        {
                            GUI.enabled = false;
                            EditorGUILayout.Toggle(false, GUILayout.Width(20));
                            GUI.enabled = true;
                        }
                    }

                    //Choosing which grid points are availabel
                    else if (GridMode == 0)
                    {
                        tempArray[x, y] = EditorGUILayout.Toggle(tempArray[x, y], GUILayout.Width(10));
                    }
                }
                EditorGUILayout.EndHorizontal();
            }

            if (GridMode == 0 && GUILayout.Button("Apply Grid"))
            {
                _customGrid.arraySize = v.x * v.y;
                _startingInteractors.arraySize = v.x * v.y;

                for (int y = 0; y < v.y; y++)
                {
                    for (int x = 0; x < v.x; x++)
                    {
                        bool b = tempArray[x, y];

                        //baseScript.CustomGrid[] ;
                        _customGrid.GetArrayElementAtIndex(x + (y * v.x)).boolValue = b;

                        if (!b)
                        {
                            _startingInteractors.GetArrayElementAtIndex(x + (y * v.x)).intValue = 0;
                        }
                        
                    }
                }
                builtArray = true;
            }

            if (builtArray)
            {
                GUIStyle style = new GUIStyle();
                style.richText = true;
                EditorGUILayout.LabelField("<color=green>Custom Grid has been applied!</color>", style);
            }
            //EditorGUILayout.PropertyField(qteType1); //Type of Spawner (1)
        }

        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

        showInteractors = EditorGUILayout.Toggle("Existing Interactors", showInteractors);
        if (showInteractors)
        {
            int size = _allInteractors.arraySize;
            for (int i = 0; i < size; i++)
            {
                EditorGUILayout.LabelField(_allInteractors.GetArrayElementAtIndex(i).ToString());
                //EditorGUILayout.PropertyField(); //All interactors
            }
        }

        //Apply changes
        this.serializedObject.ApplyModifiedProperties();
    }
}
