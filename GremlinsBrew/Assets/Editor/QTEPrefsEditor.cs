using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

[CustomEditor(typeof(QTEPrefsScript))]
public class QTEPrefsEditor : Editor
{
    private ReorderableList prefs;

    protected virtual void OnEnable()
    {
        SerializedProperty ps = this.serializedObject.FindProperty("pref_values");
        prefs = new ReorderableList(this.serializedObject, ps, true, true, true, true);

        prefs.elementHeight = EditorGUIUtility.singleLineHeight * 8f;
        prefs.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
        {
            GUI.color = Color.white;
            EditorGUI.LabelField(rect, "Interact Option: " + index, EditorStyles.boldLabel);

            var prefElement = prefs.serializedProperty.GetArrayElementAtIndex(index);

            //QTE Type
            SerializedProperty typeEnum = prefElement.FindPropertyRelative("MY_TYPE");
            EditorGUI.PropertyField(new Rect(rect.x + 140, rect.y, 130, EditorGUIUtility.singleLineHeight), typeEnum, GUIContent.none);

            int enumVal = typeEnum.enumValueIndex;
            //0-None, 1-InstantSuccess, 2-Hold, 3-AxisPoints, 4-Rapid, 5-Thumbstick, 6-Rotate, 7-Charge, 8-Draw, 9-BoltCharge
            //10-Catcher, 11-Hug, 12-RotateCrush

            if (enumVal > 0)
            {
                //QTE Prompt Text/Sprite
                //EditorGUI.PropertyField(new Rect(rect.x + 10, rect.y + EditorGUIUtility.singleLineHeight * 1.1f, 250, EditorGUIUtility.singleLineHeight), prefElement.FindPropertyRelative("PROMPT_TEXT"), GUIContent.none);
                EditorGUI.PropertyField(new Rect(rect.x + 10, rect.y + EditorGUIUtility.singleLineHeight * 1.1f, 250, EditorGUIUtility.singleLineHeight), prefElement.FindPropertyRelative("PROMPT_BTN"), GUIContent.none);
            }


            if (enumVal == 1)
            {
                DrawProp("Required Players:", rect, 2.5f, prefElement.FindPropertyRelative("NUM_PLAYERS"));
            }
            else if (enumVal == 2)
            {
                DrawProp("Required Players:", rect, 2.5f, prefElement.FindPropertyRelative("NUM_PLAYERS"));
                DrawProp("Hold Time:", rect, 3.5f, prefElement.FindPropertyRelative("float_value"));
                DrawProp("Can Overhold:", rect, 4.5f, prefElement.FindPropertyRelative("bool_value"));
            }

            else if (enumVal == 3)
            {
                DrawArray(prefElement.FindPropertyRelative("vector_array"), index);
            }
            else if (enumVal == 4)
            {
                DrawProp("Required Players:", rect, 2.5f, prefElement.FindPropertyRelative("NUM_PLAYERS"));
                DrawProp("Target:", rect, 3.5f, prefElement.FindPropertyRelative("int_value"));
            }
            else if (enumVal == 5)
            {
                DrawProp("Left Thumbstick Pos:", rect, 2.5f, prefElement.FindPropertyRelative("vector_valueL"));
                DrawProp("Second Thumbstick:", rect, 3.5f, prefElement.FindPropertyRelative("bool_value"));
                DrawProp("Right Thumbstick Pos:", rect, 4.5f, prefElement.FindPropertyRelative("vector_valueR"));
                DrawProp("Speed:", rect, 5.5f, prefElement.FindPropertyRelative("vector2_value").FindPropertyRelative("x"));
                DrawProp("Max Hold:", rect, 6.5f, prefElement.FindPropertyRelative("vector2_value").FindPropertyRelative("y"));
            }
            else if (enumVal == 6 || enumVal == 12)
            {
                DrawProp("Required Players:", rect, 2.5f, prefElement.FindPropertyRelative("NUM_PLAYERS"));
                DrawProp("Clockwise:", rect, 3.5f, prefElement.FindPropertyRelative("bool_value"));
                DrawProp("Target:", rect, 4.5f, prefElement.FindPropertyRelative("int_value"));
            }
            else if (enumVal == 7)
            {
                DrawProp("MoveSpeed:", rect, 2.5f, prefElement.FindPropertyRelative("vector3_value").FindPropertyRelative("z"));
                DrawProp("Min:", rect, 3.5f, prefElement.FindPropertyRelative("vector3_value").FindPropertyRelative("x"));
                DrawProp("Max:", rect, 4.5f, prefElement.FindPropertyRelative("vector3_value").FindPropertyRelative("y"));
            }
            else if (enumVal == 8)
            {
                DrawProp("Required Players:", rect, 2.5f, prefElement.FindPropertyRelative("NUM_PLAYERS"));
                DrawProp("Draw Rune Number:", rect, 3.5f, prefElement.FindPropertyRelative("int_value"));
            }
            else if (enumVal == 9)
            {
                DrawProp("Required Players:", rect, 2.5f, prefElement.FindPropertyRelative("NUM_PLAYERS"));
                DrawProp("Size of indicator", rect, 3.5f, prefElement.FindPropertyRelative("float_value"));
                //DrawProp("Size 1:", rect, 3.5f, prefElement.FindPropertyRelative("vector3_value").FindPropertyRelative("x"));
                //DrawProp("Size 2:", rect, 4.5f, prefElement.FindPropertyRelative("vector3_value").FindPropertyRelative("y"));
                //DrawProp("Size 3:", rect, 5.5f, prefElement.FindPropertyRelative("vector3_value").FindPropertyRelative("z"));
            }
            else if (enumVal == 10)
            {
                DrawProp("Required Players:", rect, 2.5f, prefElement.FindPropertyRelative("NUM_PLAYERS"));
                DrawProp("Lightning Speed:", rect, 3.5f, prefElement.FindPropertyRelative("float_value"));
            }
            /*
            int test = ((StoryControl)target).elementTesting;
            if (test > index)
            
                GUI.color = Color.green;
            }

            rect.y += 3;
            EditorGUI.PropertyField(new Rect(rect.x, rect.y, 130, EditorGUIUtility.singleLineHeight), typeProp, GUIContent.none);
            if (enumVal > 2 && enumVal < 6) { EditorGUI.PropertyField(new Rect(rect.x + 140, rect.y, 130, EditorGUIUtility.singleLineHeight), storyElement.FindPropertyRelative("dialog"), GUIContent.none); }
            else if (enumVal == 2) { EditorGUI.PropertyField(new Rect(rect.x + 140, rect.y, 130, EditorGUIUtility.singleLineHeight), storyElement.FindPropertyRelative("val"), GUIContent.none); }

            GUI.color = Color.white;
            */
        };

        prefs.drawHeaderCallback = (Rect rect) =>
        {
            EditorGUI.LabelField(rect, "INTERACTION OPTIONS", EditorStyles.boldLabel);
        };
    }

    private void DrawProp(string _s, Rect rect, float _y, SerializedProperty _p)
    {
        EditorGUI.LabelField(new Rect(rect.x + 10, rect.y + EditorGUIUtility.singleLineHeight * _y, 130, EditorGUIUtility.singleLineHeight), _s);
        EditorGUI.PropertyField(new Rect(rect.x + 140, rect.y + EditorGUIUtility.singleLineHeight * _y, 130, EditorGUIUtility.singleLineHeight), _p, GUIContent.none);
    }

    public override void OnInspectorGUI()
    {
        this.serializedObject.Update();
        QTEPrefsScript baseScript = (QTEPrefsScript)target;

        EditorGUILayout.Separator();

        //EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

        /*
        EditorGUILayout.PropertyField(qteType1); //Type of Spawner (1)
        if (baseScript.QTEType1 != QTEType.None)
        {
            _text1.stringValue = EditorGUILayout.TextField("Popup text:", _text1.stringValue);

            switch (baseScript.QTEType1)
            {
                case QTEType.Hold:
                    _float1.floatValue = EditorGUILayout.Slider("Hold time:", _float1.floatValue, 0, 10);
                    _bool1.boolValue = EditorGUILayout.Toggle("Can overhold", _bool1.boolValue);
                    break;

                case QTEType.AxisPoints:
                    DrawArray(_points1);
                    break;

                case QTEType.Rotate:
                    _int1.intValue = EditorGUILayout.IntField("How Many Laps:", _int1.intValue);
                    _bool1.boolValue = EditorGUILayout.Toggle("Rotate Clockwise:", _bool1.boolValue);
                    break;

                case QTEType.Thumbstick:
                    _vector1.vector2Value = EditorGUILayout.Vector2Field("Target Position:", _vector1.vector2Value);
                    _SndJoyStick.boolValue = EditorGUILayout.Toggle("Use Second Thumbstick?:", _SndJoyStick.boolValue);
                    _vector1R.vector2Value = EditorGUILayout.Vector2Field("Target Position:", _vector1R.vector2Value);
                    break;

                case QTEType.Rapid:
                    _int1.intValue = EditorGUILayout.IntField("Number target:", _int1.intValue);
                    break;
            }

            //Convert number of players to a toggle
            bool toggle1 = (_numberOfPlayers1.intValue == 1) ? false : true;
            toggle1 = EditorGUILayout.Toggle("Requires 2 players: ", toggle1);
            _numberOfPlayers1.intValue = (toggle1) ? 2 : 1;
        }
        else
        {
            _text1.stringValue = "";
        }
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        */

        prefs.DoLayoutList();

        this.serializedObject.ApplyModifiedProperties();
    }


    //Thanks to this website for helping with the below
    //https://forum.unity.com/threads/working-with-arrays-as-serializedproperty-ies-in-editor-script.97356/
    protected void DrawArray(SerializedProperty array, int index)
    {
        int temp = array.arraySize;
        int count = EditorGUILayout.IntField("[I" + index + "] Points", temp);
        array.arraySize = count;

        EditorGUI.indentLevel++;
        for (int i = 0; i < count; i++)
        {
            var element = array.GetArrayElementAtIndex(i);
            EditorGUILayout.PropertyField(element);
        }
        EditorGUI.indentLevel--;
    }
}
