using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Newtonsoft.Json;
using Enum = System.Enum;
using UnityEditor.SceneManagement;

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
        string selectedObjGUID = null;

        // Window creation and showing
        static CodeManagerWizard Instance;
        [MenuItem("Window/Code Manager/Wizard", priority = -5)]
        public static void ShowExample()
        {
            Instance = GetWindow<CodeManagerWizard>(false, "Code Manager", true);
        }


        /// <summary>
        /// Given the info of a scriptable object asset, creates a button to select it
        /// </summary>
        void SetupButton(AssetInfo info)
        {
            Button button = new Button();
            button.text = info.Path.Substring("Assets/".Length);
            button.name = info.GUID;
            button.styleSheets.Add(uss);
            button.RegisterCallback<ClickEvent, string>(SelectAssetCallback, info.GUID);
            ScrollingContainerContent.Add(button);
        }

        void GenerateClass(ClickEvent evt)
        {
            ClassType type = (ClassType)GenerateType.index;
            ClassGenerator.Generate(type, ClassType.value);
        }

        void Deselect()
        {
            if (currentInspector != null)
            {
                inspectorContainer.Remove(currentInspector);
                currentInspector = null;
            }
            wizardData.selectedAssetGUID = string.Empty;
        }
        void Deselect(ClickEvent evt) { Deselect(); }

        void FindAllReferences(ClickEvent evt)
        {
            Deselect();
            AssetTracker.FindReferences();
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

            root.Q<Button>("FindReferences").RegisterCallback<ClickEvent>(FindAllReferences);
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
            bool jsonLoaded = AssetTracker.CheckLoad();
            if (!jsonLoaded)
            {
                List<AssetInfo> assetInfos = new List<AssetInfo>();

                string[] typeFilters = { "t:ScriptObjVariableBase", "t:ScriptObjEventBase", "t:ScriptObjCollectionBase" };
                foreach (string filter in typeFilters)
                {
                    string[] guids = AssetDatabase.FindAssets(filter, null);
                    foreach (string guid in guids) 
                    {
                        AssetInfo assetInfo = new AssetInfo
                        {
                            GUID = guid,
                            Path = AssetDatabase.GUIDToAssetPath(guid),
                            AssetReferencesGUIDs = null,
                            SceneObjectReferences = null
                        };
                        SetupButton(assetInfo);
                        assetInfos.Add(assetInfo);
                    }
                }

                TextAsset jsonAsset = new TextAsset(JsonConvert.SerializeObject(assetInfos));
                string path = AssetTracker.jsonFolder[0] + AssetTracker.jsonFileName;
                AssetDatabase.CreateAsset(jsonAsset, path);
                AssetDatabase.SaveAssets();
            }
            else
            {
                bool saveChanges = false;
                foreach(AssetInfo assetinfo in AssetTracker.AssetInfos)
                {
                    if (AssetDatabase.GUIDToAssetPath(assetinfo.GUID) == string.Empty)
                    {
                        AssetTracker.AssetInfos.Remove(assetinfo);
                        saveChanges = true;
                    }
                    else
                    {
                        SetupButton(assetinfo);
                    }
                }
                if(saveChanges)
                {
                    AssetTracker.SaveChanges();
                }
            }

            if (wizardData.selectedAssetGUID != string.Empty)
            {
                VisualElement elem = ScrollingContainerContent.Children().FirstOrDefault(elem => elem.name == wizardData.selectedAssetGUID);
                if (elem == null)
                {
                    wizardData.selectedAssetGUID = string.Empty;
                }
                else
                {
                    SelectAsset(wizardData.selectedAssetGUID);
                }
            }

            ScrollingContainerContent.Sort(CompareByName);
            AssetTracker.ChangedAssets.Clear();
        }

        // show asset's inspector and select it in assets folder
        private void SelectAsset(string guid)
        {
            if (currentInspector != null)
            {
                inspectorContainer.Remove(currentInspector);
                currentInspector = null;
            }

            EditorUtility.FocusProjectWindow();
            UnityEngine.Object asset = AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(guid), typeof(UnityEngine.Object));
            Selection.activeObject = asset;
            selectedObjGUID = guid;

            currentInspector = Editor.CreateEditor(asset).CreateInspectorGUI();
            inspectorContainer.Add(currentInspector);
        }

        // callback event for clicking a button that references a scriptable object
        void SelectAssetCallback(ClickEvent evt, string guid)
        {
            SelectAsset(guid);
            wizardData.selectedAssetGUID = guid;
        }

        // compares visual elements by name to sort the scroll view
        public static int CompareByName(VisualElement first, VisualElement second)
        {
            Button firstAsButton = first as Button;
            Button secondAsButton = second as Button;

            // null checks
            if(firstAsButton == null)
            {
                return secondAsButton == null ? 0 : -1;
            }
            else if(secondAsButton == null)
            {
                return 1;
            }

            return firstAsButton.text.CompareTo(secondAsButton.text);
        }

        void RemoveDeletedAssetButton(AssetInfo assetInfo)
        {
            VisualElement deleted = ScrollingContainerContent.Children().FirstOrDefault(elem => elem.name == assetInfo.GUID);
            if (deleted == null) return;
            
            ScrollingContainerContent.Remove(deleted);

            // if the removed element was selected remove the inspector for it
            if (deleted.name == selectedObjGUID)
            {
                Deselect();
            }
        }

        void UpdateMovedAssetButton(AssetInfo assetInfo)
        {
            VisualElement moved = ScrollingContainerContent.Children().FirstOrDefault(elem => elem.name == assetInfo.GUID);
            if (moved == null) return;

            if (moved is Button button)
            {
                button.text = assetInfo.Path.Substring("Assets/".Length);
            }
        }

        // updates the scroll view container based off of changes to assets
        public void OnGUI()
        {
            if(AssetTracker.IsChanges())
            {
                // process asset changes
                foreach((AssetChanges change, AssetInfo info) in AssetTracker.ChangedAssets)
                {
                    switch(change)
                    {
                        case AssetChanges.Created:
                            SetupButton(info);
                            break;
                        case AssetChanges.Deleted:
                            RemoveDeletedAssetButton(info);
                            break;
                        case AssetChanges.Moved:
                            UpdateMovedAssetButton(info);
                            break;
                    }
                }

                ScrollingContainerContent.Sort(CompareByName);
                AssetTracker.ChangedAssets.Clear();
            }
        }
    }
}