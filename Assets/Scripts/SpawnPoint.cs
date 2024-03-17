using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class SpawnPoint : MonoBehaviour
{
    public float radius = 7.0f;
}

[CustomEditor(typeof(SpawnPoint))]
public class SpawnPointEditor : Editor
{
    Tool previousTool;

    private void OnEnable()
    {
        previousTool = Tools.current;
        Tools.current = Tool.None;
    }
    private void OnSceneGUI()
    {
        SpawnPoint sp = target as SpawnPoint;
        Transform tr = sp.transform;
        Vector3 pos = tr.position;

        Color color = new Color(1, 0.8f, 0.4f, 1);
        Handles.color = color;

        EditorGUI.BeginChangeCheck();
        float radius = Handles.RadiusHandle(tr.rotation, pos, sp.radius);
        if(EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(target, "Changed Radius");
            sp.radius = radius;
        }

        GUI.color = color;
        Handles.Label(pos, sp.radius.ToString("F1"));
    }

    private void OnDisable()
    {
        Tools.current = previousTool;
    }
}