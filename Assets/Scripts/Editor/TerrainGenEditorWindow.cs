using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class TerrainGenEditorWindow : EditorWindow
{
    //[MenuItem("Terrain Generator/Main window")]
    public static void ShowWindow()
    {
        TerrainGenEditorWindow window = GetWindow<TerrainGenEditorWindow>();
        window.titleContent = new GUIContent("Cool Window");
    }

    public void OnGUI()
    {
        EditorGUILayout.LabelField("Well hello");
    }
}
