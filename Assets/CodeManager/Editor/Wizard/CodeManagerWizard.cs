using System;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.SceneManagement;
using System.IO;
using Newtonsoft.Json;

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

        // For ensuring fields don't reset on unity reimport
        [SerializeField] WizardData wizardData;

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
            button.RegisterCallback<ClickEvent, string>(SelectAssetCallback, path);
            ScrollingContainerContent.Add(button);
        }

        void GenerateClass(ClickEvent evt)
        {
            ClassType type = (ClassType)GenerateType.index;
            ClassGenerator.Generate(type, ClassType.value);
        }

        void Deselect(ClickEvent evt)
        {
            if (currentInspector != null)
            {
                inspectorContainer.Remove(currentInspector);
                currentInspector = null;
            }

            Selection.activeObject = null;

            wizardData.selectedAssetPath = string.Empty;
        }

        void ClassTypeChanged(ChangeEvent<string> evt)
        {
            wizardData.classType = ClassType.value;
        }

        void GenerateTypeChanged(ChangeEvent<string> evt)
        {
            wizardData.dropdownIndex = GenerateType.index;
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

            root.Q<Button>("DeselectScriptObj").RegisterCallback<ClickEvent>(Deselect);
            root.Q<Button>("Generate").RegisterCallback<ClickEvent>(GenerateClass);

            // get generation Visual Elements
            ClassType = root.Q<TextField>("ClassType");
            GenerateType = root.Q<DropdownField>("GenerateType");

            // add list of choices for generation to dropdown
            GenerateType.choices.Clear();
            foreach(string name in Enum.GetNames(typeof(ClassType)))
            {
                GenerateType.choices.Add(name);
            }

            // get saved data for the UI
            string[] wizDataGuid = AssetDatabase.FindAssets("t:WizardData", new[] { "Assets/AidenK.CodeManager/Settings" });
            if(wizDataGuid.Length > 0)
            {
                wizardData = AssetDatabase.LoadAssetAtPath<WizardData>(AssetDatabase.GUIDToAssetPath(wizDataGuid[0]));
            }
            else
            {
                wizardData = ScriptableObject.CreateInstance<WizardData>();
                AssetDatabase.CreateAsset(wizardData, "Assets/AidenK.CodeManager/WizardData.asset");
                AssetDatabase.SaveAssets();
            }

            // get the saved data for class generation
            ClassType.value = wizardData.classType;
            GenerateType.index = wizardData.dropdownIndex;

            // setup generation callbacks
            ClassType.RegisterValueChangedCallback(ClassTypeChanged);
            GenerateType.RegisterValueChangedCallback(GenerateTypeChanged);

            // find scriptable objects in assets

            bool jsonLoaded = CodeManagerAssetPostprocessor.CheckLoad();
            if (!jsonLoaded)
            {
                List<AssetInfo> assetInfos = new List<AssetInfo>();

                string[] typeFilters = { "t:ScriptObjVariableBase", "t:ScriptObjEventBase", "t:ScriptObjCollectionBase" };
                foreach (string filter in typeFilters)
                {
                    string[] guids = AssetDatabase.FindAssets(filter, null);
                    foreach (string guid in guids) 
                    { 
                        string path = AssetDatabase.GUIDToAssetPath(guid);
                        SetupButtonFromPath(path);

                        AssetInfo assetInfo = new AssetInfo
                        {
                            GUID = guid,
                            path = path,
                            AssetReferencesGUIDs = new List<string>(),
                            GameObjectInstanceIDs = new List<int>()
                        };
                        assetInfos.Add(assetInfo);
                    }
                }

                TextAsset jsonAsset = new TextAsset(JsonConvert.SerializeObject(assetInfos));
                AssetDatabase.CreateAsset(jsonAsset, "Assets/AidenK.CodeManager/Settings/AidenK.CodeManager.AssetInfo.asset");
                AssetDatabase.SaveAssets();
            }
            else
            {
                foreach(AssetInfo assetinfo in CodeManagerAssetPostprocessor.assetInfos)
                {
                    SetupButtonFromGUID(assetinfo.GUID);
                }
            }

            if (wizardData.selectedAssetPath != string.Empty)
            {
                VisualElement elem = ScrollingContainerContent.Children().Where(elem => elem.name == wizardData.selectedAssetPath).FirstOrDefault();
                if (elem == null)
                {
                    wizardData.selectedAssetPath = string.Empty;
                }
                else
                {
                    SelectAsset(wizardData.selectedAssetPath);
                }
            }

            ScrollingContainerContent.Sort(CompareByName);
            CodeManagerAssetPostprocessor.AssetChanges.Clear();
        }

        // show asset's inspector and select it in assets folder
        private void SelectAsset(string path)
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

            currentInspector = Editor.CreateEditor(asset).CreateInspectorGUI();
            inspectorContainer.Add(currentInspector);
        }

        // callback event for clicking a button that references a scriptable object
        void SelectAssetCallback(ClickEvent evt, string path)
        {
            SelectAsset(path);
            wizardData.selectedAssetPath = path;
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
                        button.UnregisterCallback<ClickEvent, string>(SelectAssetCallback);
                        button.RegisterCallback<ClickEvent, string>(SelectAssetCallback, movedTo);
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