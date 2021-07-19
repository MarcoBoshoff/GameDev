using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

[CustomEditor(typeof(MenuAiPointScript))]
public class MenuAiPointEditor : Editor
{
    private ReorderableList commands;

    protected virtual void OnEnable()
    {
        SerializedProperty ps = this.serializedObject.FindProperty("commands");
        commands = new ReorderableList(this.serializedObject, ps, true, true, true, true);

        commands.elementHeight = EditorGUIUtility.singleLineHeight * 7f;
        commands.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
        {
            GUI.color = Color.white;
            EditorGUI.LabelField(rect, "Interact Option: " + index, EditorStyles.boldLabel);

            var commandElement = commands.serializedProperty.GetArrayElementAtIndex(index);

            DrawProp("Point Connected", rect, 2.5f, commandElement.FindPropertyRelative("point"));
            DrawProp("Wait For:", rect, 3.5f, commandElement.FindPropertyRelative("waitFor"));
            DrawProp("Command Type:", rect, 4.5f, commandElement.FindPropertyRelative("command"));
        };

        commands.drawHeaderCallback = (Rect rect) =>
        {
            EditorGUI.LabelField(rect, "Stored Commands", EditorStyles.boldLabel);
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
        MenuAiPointScript baseScript = (MenuAiPointScript)target;

        DrawDefaultInspector();

        EditorGUILayout.Separator();

        commands.DoLayoutList();
        

        this.serializedObject.ApplyModifiedProperties();
    }
}
