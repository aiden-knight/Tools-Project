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
    bool SphereHandle = true;

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

        if(Handles.Button(pos, tr.rotation, 2f, 2f, Handles.RectangleHandleCap))
        {
            SphereHandle = !SphereHandle;
        }

        if(SphereHandle)
        {
            EditorGUI.BeginChangeCheck();
            float radius = Handles.RadiusHandle(tr.rotation, pos, sp.radius);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(target, "Changed Radius");
                sp.radius = radius;
            }
        }
        else
        {
            Handles.DrawWireDisc(pos, tr.up, sp.radius);
            EditorGUI.BeginChangeCheck();
            float r = Handles.ScaleValueHandle(sp.radius, pos + tr.forward * sp.radius, Quaternion.identity, 2f, Handles.ArrowHandleCap, 0.1f);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(target, "Changed Radius");
                if (r < 0.0f) r = 0.0f;
                sp.radius = r;
            }
        }

        GUI.color = color;
        Handles.Label(pos, sp.radius.ToString("F1"));
    }

    private void OnDisable()
    {
        Tools.current = previousTool;
    }
}