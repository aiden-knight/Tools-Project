using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using PopupWindow = UnityEditor.PopupWindow;

public class ReferencePopup : EditorWindow
{
    [SerializeField]
    VisualTreeAsset visualTreeAsset = null;

    // Add menu item
    [MenuItem("Example/Popup Example")]
    static void Init()
    {
        EditorWindow window = EditorWindow.CreateInstance<ReferencePopup>();
        window.Show();
    }

    private void CreateGUI()
    {
        visualTreeAsset.CloneTree(rootVisualElement);

        var button = rootVisualElement.Q<Button>();
        button.clicked += () => PopupWindow.Show(button.worldBound, new ReferencePopupContent());
    }
}