using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(TutorialScript))]
public class TutorialScriptEditor : Editor
{

    protected virtual void OnEnable()
    {

    }

    public override void OnInspectorGUI()
    {
        this.serializedObject.Update();
        TutorialScript baseScript = (TutorialScript)target;

        DrawDefaultInspector();
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

        //Handle tutorial text here


        //Apply changes
        this.serializedObject.ApplyModifiedProperties();
    }
}
