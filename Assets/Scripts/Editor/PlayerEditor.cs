using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.TerrainTools;
using Unity.VisualScripting;

[CustomEditor(typeof(Player))]
public class PlayerEditor : Editor
{
    SerializedProperty healthProperty;
    SerializedProperty speedProperty;
    SerializedProperty damageProperty;

    int selectedIndex = -1;

    bool toggleCustomEditor = false;
    bool toggleDefaultEditor = false;

    private void OnEnable()
    {
        SerializedProperty iterator = serializedObject.GetIterator();
        iterator.NextVisible(true);

        do
        {
            if (iterator.name.Equals("health")) healthProperty = iterator.Copy();
            if (iterator.name.Equals("speed")) speedProperty = iterator.Copy();
            if (iterator.name.Equals("damage")) damageProperty = iterator.Copy();
        } while (iterator.NextVisible(false));

    }


    void OnCustomInspectorGUI()
    {
        serializedObject.Update();

        string label = "";

        if (healthProperty != null)
        {
            EditorGUILayout.Slider(healthProperty, 0f, 100f);
            label += $"HP: {healthProperty.floatValue},";
        }
        if (speedProperty != null)
        {
            EditorGUILayout.Slider(speedProperty, 0f, 25f);
            label += $"Speed: {speedProperty.floatValue},";
        }
        if (damageProperty != null)
        {
            EditorGUILayout.Slider(damageProperty, 1f, 50f);
            label += $"DMG: {damageProperty.floatValue},";
        }

        if (label.Length > 0)
            label = label.Substring(0, label.Length - 1) + ".";

        EditorGUILayout.LabelField(label);

        EditorGUILayout.Space(10f);

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Reset Values"))
        {
            if (healthProperty != null) healthProperty.floatValue = 1f;
            if (speedProperty != null) speedProperty.floatValue = 1f;
            if (damageProperty != null) damageProperty.floatValue = 1f;
        }
        if (GUILayout.Button("Max Values"))
        {
            if (healthProperty != null) healthProperty.floatValue = 100f;
            if (speedProperty != null) speedProperty.floatValue = 25f;
            if (damageProperty != null) damageProperty.floatValue = 50f;
        }

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(20f);

        EditorGUILayout.BeginVertical();
        EditorGUILayout.BeginHorizontal();
        GUILayout.Button("1");
        GUILayout.Button("2");
        GUILayout.Button("3");
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginHorizontal();
        GUILayout.Button("4");
        GUILayout.Button("5");
        GUILayout.Button("6");
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginHorizontal();
        GUILayout.Button("7");
        GUILayout.Button("8");
        GUILayout.Button("9");
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.EndVertical();

        EditorGUILayout.Space(20f);

        selectedIndex = GUILayout.SelectionGrid(selectedIndex, new string[] { "1..", "2..", "3..", "4.." }, 2);
        EditorGUILayout.LabelField("Selected Index = " + selectedIndex.ToString());

        GUILayout.Box("Something");

        serializedObject.ApplyModifiedProperties();
    }

    public override void OnInspectorGUI()
    {
        EditorGUILayout.BeginHorizontal();
        toggleDefaultEditor = GUILayout.Toggle(toggleDefaultEditor, "Enable Default Editor");
        toggleCustomEditor = GUILayout.Toggle(toggleCustomEditor, "Enable Custom Editor");
        EditorGUILayout.EndHorizontal();

        if (toggleDefaultEditor)
        {
            EditorGUILayout.Space(30f);
            EditorGUILayout.LabelField("Default Editor:", EditorStyles.boldLabel);
            base.OnInspectorGUI();
        }

        if (toggleCustomEditor)
        {
            EditorGUILayout.Space(30f);
            EditorGUILayout.LabelField("Custom Editor:", EditorStyles.boldLabel);
            OnCustomInspectorGUI();
        }
    }
}
