using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

[CustomEditor(typeof(CustomerControlScript))]
public class CustomerScriptEditor : Editor
{
    SerializedProperty _patience;

    SerializedProperty _customersAmount, _customersPerIncrease, _customersMax, _customerMoveScript, _patienceStart, _patienceEnd, _patienceChange;

    protected virtual void OnEnable()
    {
        _patience = this.serializedObject.FindProperty("patience");

        _customersAmount = this.serializedObject.FindProperty("numberOfCustomers");
        _customersPerIncrease = this.serializedObject.FindProperty("_custNumInc");
        _customersMax = this.serializedObject.FindProperty("_custNumMax");
        _customerMoveScript = this.serializedObject.FindProperty("customerMove");

        _patienceStart = this.serializedObject.FindProperty("patienceStart");
        _patienceEnd = this.serializedObject.FindProperty("patienceEnd");
        _patienceChange = this.serializedObject.FindProperty("patienceChange");

        //SerializedProperty ps = this.serializedObject.FindProperty("presets");
        //presets = new ReorderableList(this.serializedObject, ps, true, true, true, true);

        //presets.drawElementBackgroundCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
        //{
        //    if (isActive)
        //    {
        //        Texture2D tex = new Texture2D(1, 1);
        //        tex.SetPixel(0, 0, new Color(0.33f, 0.66f, 1f, 0.66f));
        //        tex.Apply();
        //        GUI.DrawTexture(rect, tex as Texture);
        //    }
        //    else
        //    {
        //        Texture2D tex = new Texture2D(1, 1);
        //        tex.SetPixel(0, 0, (index % 2 == 0)?new Color(0.7f, 0.7f, 0.7f):Color.white);
        //        tex.Apply();
        //        GUI.DrawTexture(rect, tex as Texture);
        //    }
        //};

        // presets.elementHeight = EditorGUIUtility.singleLineHeight * 6f;
        //presets.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
        //{
        //    GUI.color = Color.white;
        //    var prefElement = presets.serializedProperty.GetArrayElementAtIndex(index);

        //    var _howMany = prefElement.FindPropertyRelative("howMany");
        //    _howMany.intValue = EditorGUI.IntSlider(new Rect(rect.x + 150, rect.y, 110, EditorGUIUtility.singleLineHeight), _howMany.intValue, 0, 10);

        //    EditorGUI.LabelField(rect, "DAY " + (index + 1).ToString() + ": " + _howMany.intValue + " customers", EditorStyles.boldLabel);

        //    DrawSlider("Health %:", rect, 1.7f, prefElement.FindPropertyRelative("healthPercent"));
        //    DrawSlider("Poison %:", rect, 2.7f, prefElement.FindPropertyRelative("poisonPercent"));
        //    DrawSlider("Love %:", rect, 3.7f, prefElement.FindPropertyRelative("lovePercent"));
        //    DrawSlider("Mana %:", rect, 4.7f, prefElement.FindPropertyRelative("manaPercent"));
        //};

        //presets.drawHeaderCallback = (Rect rect) =>
        //{
        //    EditorGUI.LabelField(rect, "Customer Presets", EditorStyles.boldLabel);
        //};
    }

    //private void DrawProp(string _s, Rect rect, float _y, SerializedProperty _p)
    //{
    //    EditorGUI.LabelField(new Rect(rect.x + 10, rect.y + EditorGUIUtility.singleLineHeight * _y, 160, EditorGUIUtility.singleLineHeight), _s);
    //    EditorGUI.PropertyField(new Rect(rect.x + 100, rect.y + EditorGUIUtility.singleLineHeight * _y, 160, EditorGUIUtility.singleLineHeight), _p, GUIContent.none);
    //}

    //private void DrawSlider(string _s, Rect rect, float _y, SerializedProperty _p)
    //{
    //    EditorGUI.LabelField(new Rect(rect.x + 10, rect.y + EditorGUIUtility.singleLineHeight * _y, 160, EditorGUIUtility.singleLineHeight), _s);
    //    _p.floatValue = EditorGUI.Slider(new Rect(rect.x + 100, rect.y + EditorGUIUtility.singleLineHeight * _y, 160, EditorGUIUtility.singleLineHeight), _p.floatValue, 0, 1f);
    //}


    public override void OnInspectorGUI()
    {
        this.serializedObject.Update();
        CustomerControlScript baseScript = (CustomerControlScript)target;

        EditorGUILayout.Separator();

        EditorGUILayout.LabelField("Customers Count", EditorStyles.boldLabel);
        if (EditorApplication.isPlaying)
        {
            EditorGUILayout.LabelField(string.Format("Total Customers Today = {0}({2}) / max({1})", baseScript.numberOfCustomers, (int)baseScript.MaxCustomers, (int)baseScript.numberOfCustomers));
            EditorGUILayout.LabelField(string.Format("Last requested: {0}", baseScript.Requesting));
        }
        else
        {
            _customersAmount.floatValue = EditorGUILayout.FloatField("Start amount", _customersAmount.floatValue);
            _customersPerIncrease.floatValue = EditorGUILayout.Slider("Increase %", _customersPerIncrease.floatValue, 0, 1);
            _customersMax.floatValue = EditorGUILayout.FloatField("Max", _customersMax.floatValue);
        }

        EditorGUILayout.Separator();
        EditorGUILayout.Separator();

        EditorGUILayout.LabelField("Patience", EditorStyles.boldLabel);
        if (EditorApplication.isPlaying)
        {
            EditorGUILayout.LabelField(string.Format("Current patience level = {0}", baseScript.CurrentPatience));
        }
        else
        {
            _patienceStart.floatValue = EditorGUILayout.FloatField("Start at", _patienceStart.floatValue);
            _patienceEnd.floatValue = EditorGUILayout.FloatField("Minimum", _patienceEnd.floatValue);
            _patienceChange.floatValue = EditorGUILayout.Slider("Change amount", _patienceChange.floatValue, 0.01f, 1f);
        }

        EditorGUILayout.Separator();
        EditorGUILayout.Separator();

        EditorGUILayout.LabelField("References", EditorStyles.boldLabel);

        EditorGUILayout.PropertyField(_customerMoveScript);

        this.serializedObject.ApplyModifiedProperties();
    }
}
