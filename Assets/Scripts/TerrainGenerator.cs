using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class TerrainGenerator : MonoBehaviour
{
    [SerializeField] int genWidth = 128;
    [SerializeField] int genHeight = 128;

    private void Update()
    {
        Debug.Log($"W:{genWidth} H:{genHeight}");
    }
}

[CustomEditor(typeof(TerrainGenerator)), CanEditMultipleObjects]
public class TerrainGeneratorEditor : Editor
{
    public override VisualElement CreateInspectorGUI()
    {
        VisualElement root = new VisualElement();

        VisualTreeAsset asset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/UI/UXML/terrainGeneratorEditor.uxml");
        asset.CloneTree(root);

        StyleSheet sheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/UI/USS/terrainGeneratorEditor.uss");
        root.styleSheets.Add(sheet);

        return root;
    }
}
