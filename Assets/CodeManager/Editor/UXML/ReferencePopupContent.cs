using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class ReferencePopupContent : PopupWindowContent
{
    //Set the window size
    public override Vector2 GetWindowSize()
    {
        return new Vector2(200, 100);
    }

    public override void OnGUI(Rect rect)
    {
        // Intentionally left empty
    }

    public override void OnOpen()
    {
        Debug.Log("Popup opened: " + this);

        string[] guids = AssetDatabase.FindAssets("ReferencePopupContent t:VisualTreeAsset");
        string path = AssetDatabase.GUIDToAssetPath(guids[0]);

        var visualTreeAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(path);
        visualTreeAsset.CloneTree(editorWindow.rootVisualElement);
    }

    public override void OnClose()
    {
        Debug.Log("Popup closed: " + this);
    }
}