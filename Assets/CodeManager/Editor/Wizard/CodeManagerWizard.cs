using System;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.SceneManagement;
using System.IO;

namespace AidenK.CodeManager
{
    public class CodeManagerWizard : EditorWindow
    {
        [SerializeField]
        private VisualTreeAsset m_VisualTreeAsset = default;
        [SerializeField]
        private StyleSheet uss;

        VisualElement ScrollingContainerContent;
       
        public TextField ClassType;
        public DropdownField GenerateType;

        // container to put the created inspector in
        VisualElement inspectorContainer = null;

        // reference to the created inspector to remove later
        VisualElement currentInspector = null;
        // path of the selected object so that when it is deleted the inspector can be deleted too
        string selectedObjectPath = null;

        // Window creation and showing
        static CodeManagerWizard Instance;
        [MenuItem("Window/Code Manager/Wizard", priority = -5)]
        public static void ShowExample()
        {
            Instance = GetWindow<CodeManagerWizard>(false, "Code Manager", true);
        }


        // Given the guid of a scriptable object asset, creates a button to select it
        void SetupButtonFromGUID(string guid)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            SetupButtonFromPath(path);
        }

        // Given the path of a scriptable object asset, creates a button to select it
        void SetupButtonFromPath(string path)
        {
            Button button = new Button();
            button.text = path.Substring("Assets/".Length);
            button.name = path;
            button.styleSheets.Add(uss);
            button.RegisterCallback<ClickEvent, string>(SelectAsset, path);
            ScrollingContainerContent.Add(button);
        }

        void GenerateClass(ClickEvent evt)
        {
            ClassType type = (ClassType)GenerateType.index;
            ClassGenerator.Generate(type, ClassType.text);
        }

        // Sets up the UI for the window on window creation
        public void CreateGUI()
        {
            // Each editor window contains a root VisualElement object
            VisualElement root = rootVisualElement;

            // Instantiate UXML
            VisualElement labelFromUXML = m_VisualTreeAsset.Instantiate();
            root.Add(labelFromUXML);

            var scrollV = root.Q<ScrollView>("ScriptableObjects");
            ScrollingContainerContent = scrollV.Q("unity-content-container");

            inspectorContainer = root.Q("Inspector");

            root.Q<Button>("Generate").RegisterCallback<ClickEvent>(GenerateClass);

            ClassType = root.Q<TextField>("ClassType");
            GenerateType = root.Q<DropdownField>("GenerateType");

            GenerateType.choices.Clear();
            foreach(string name in Enum.GetNames(typeof(ClassType)))
            {
                GenerateType.choices.Add(name);
            }
            GenerateType.index = 1;

            string[] varGuids = AssetDatabase.FindAssets("t:ScriptObjVariableBase", null);
            string[] eventGuids = AssetDatabase.FindAssets("t:ScriptObjEventBase", null);
            foreach (string guid in varGuids) SetupButtonFromGUID(guid);
            foreach (string guid in eventGuids) SetupButtonFromGUID(guid);


            ScrollingContainerContent.Sort(CompareByName);
            CodeManagerAssetPostprocessor.AssetChanges.Clear();
        }

        // callback event for clicking a button that references a scriptable object
        public void SelectAsset(ClickEvent evt, string path)
        {
            if (currentInspector != null)
            {
                inspectorContainer.Remove(currentInspector);
                currentInspector = null;
            }

            EditorUtility.FocusProjectWindow();
            UnityEngine.Object asset = AssetDatabase.LoadAssetAtPath(path, typeof(UnityEngine.Object));
            Selection.activeObject = asset;
            selectedObjectPath = path;

            currentInspector =  Editor.CreateEditor(asset).CreateInspectorGUI();
            inspectorContainer.Add(currentInspector);

        }

        // compares visual elements by name to sort the scroll view
        static int CompareByName(VisualElement first, VisualElement second)
        {
            // null checks
            if(first == null)
            {
                return second == null ? 0 : -1;
            }
            else if(second == null)
            {
                return 1;
            }

            return first.name.CompareTo(second.name);
        }

        // updates the scroll view container based off of changes to assets
        public void OnGUI()
        {
            if(CodeManagerAssetPostprocessor.IsChanges())
            {
                // removes buttons that reference the deleted asset from the scroll view container
                foreach(string deleted in CodeManagerAssetPostprocessor.AssetChanges.deleted)
                {
                    VisualElement elem = ScrollingContainerContent.Children().Where(elem => elem.name == deleted).FirstOrDefault();
                    if(elem != null)
                    {
                        ScrollingContainerContent.Remove(elem);

                        // if the removed element was selected remove the inspector for it
                        if(elem.name == selectedObjectPath)
                        {
                            inspectorContainer.Remove(currentInspector);
                            currentInspector = null;
                            selectedObjectPath = null;
                        }
                    }
                }

                // updates buttons that reference the moved asset
                foreach((string movedTo, string movedFrom) in CodeManagerAssetPostprocessor.AssetChanges.moved)
                {
                    VisualElement elem = ScrollingContainerContent.Children().Where(elem => elem.name == movedFrom).FirstOrDefault();
                    if (elem == null) continue;

                    if(elem is Button button)
                    {
                        button.name = movedTo;
                        button.text = movedTo.Substring("Assets/".Length);
                        button.UnregisterCallback<ClickEvent, string>(SelectAsset);
                        button.RegisterCallback<ClickEvent, string>(SelectAsset, movedTo);
                    }
                }

                // have to do this after move changes or will create duplicate
                // assets that are changed (also created)
                foreach (string reimported in CodeManagerAssetPostprocessor.AssetChanges.reimported)
                {
                    // if already exists don't add
                    if (ScrollingContainerContent.Children().Where(elem => elem.name == reimported).FirstOrDefault() != null) continue;

                    SetupButtonFromPath(reimported);
                }


                ScrollingContainerContent.Sort(CompareByName);
                CodeManagerAssetPostprocessor.AssetChanges.Clear();
            }
        }
    }
}