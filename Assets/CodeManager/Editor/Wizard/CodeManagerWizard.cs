using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Newtonsoft.Json;
using Enum = System.Enum;
using UnityEditor.SceneManagement;
using System;

namespace AidenK.CodeManager
{
    public class CodeManagerWizard : EditorWindow
    {
        [SerializeField]
        private VisualTreeAsset _visualTreeAsset = default;
        [SerializeField]
        private StyleSheet _USS;

        VisualElement _scrollingContainerContent;
       
        public TextField ClassType;
        public DropdownField GenerateType;

        // For ensuring fields don't reset on unity reimport
        WizardData _wizardData;

        // container to put the created inspector in
        VisualElement _inspectorContainer = null;

        // reference to the created inspector to remove later
        VisualElement _currentInspector = null;

        // Window creation and showing
        [MenuItem("Window/Code Manager/Wizard", priority = -5)]
        public static void ShowExample()
        {
            GetWindow<CodeManagerWizard>(false, "Code Manager", true);
        }


        /// <summary>
        /// Given the info of a scriptable object asset, creates a button to select it
        /// </summary>
        void SetupButton(AssetInfo info)
        {
            Button button = new Button();
            button.text = info.Path.Substring("Assets/".Length);
            button.name = info.GUID;
            button.styleSheets.Add(_USS);
            button.RegisterCallback<ClickEvent, string>(SelectAssetCallback, info.GUID);
            _scrollingContainerContent.Add(button);
        }

        void GenerateClass(ClickEvent evt)
        {
            ClassType type = (ClassType)GenerateType.index;
            ClassGenerator.Generate(type, ClassType.value);
        }

        void Deselect()
        {
            if (_currentInspector != null)
            {
                _inspectorContainer.Remove(_currentInspector);
                _currentInspector = null;
            }
            _wizardData.SelectedAssetGUID = string.Empty;
        }
        void Deselect(ClickEvent evt) { Deselect(); }

        void FindAllReferences(ClickEvent evt)
        {
            AssetTracker.FindReferences();
        }

        void ClassTypeChanged(ChangeEvent<string> evt)
        {
            _wizardData.ClassType = ClassType.value;
        }

        void GenerateTypeChanged(ChangeEvent<string> evt)
        {
            _wizardData.DropdownIndex = GenerateType.index;
        }

        // Sets up the UI for the window on window creation
        public void CreateGUI()
        {
            // Each editor window contains a root VisualElement object
            VisualElement root = rootVisualElement;

            // Instantiate UXML
            VisualElement labelFromUXML = _visualTreeAsset.Instantiate();
            root.Add(labelFromUXML);

            var scrollV = root.Q<ScrollView>("ScriptableObjects");
            _scrollingContainerContent = scrollV.Q("unity-content-container");

            _inspectorContainer = root.Q("Inspector");

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
                _wizardData = AssetDatabase.LoadAssetAtPath<WizardData>(AssetDatabase.GUIDToAssetPath(wizDataGuid[0]));
            }
            else
            {
                _wizardData = ScriptableObject.CreateInstance<WizardData>();
                AssetDatabase.CreateAsset(_wizardData, "Assets/AidenK.CodeManager/WizardData.asset");
                AssetDatabase.SaveAssets();
            }

            // get the saved data for class generation
            ClassType.value = _wizardData.ClassType;
            GenerateType.index = _wizardData.DropdownIndex;

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
                string path = AssetTracker.JsonFolder[0] + AssetTracker.JsonFileName;
                AssetDatabase.CreateAsset(jsonAsset, path);
                AssetDatabase.SaveAssets();
            }
            else
            {
                // if any of the assets in assets info no longer exist remove them
                bool saveChanges = false;
                for (int index = AssetTracker.GetAssetCount() - 1; index >= 0; index--)
                {
                    AssetInfo assetInfo = AssetTracker.GetAssetAt(index);
                    if (AssetDatabase.GUIDToAssetPath(assetInfo.GUID) == string.Empty)
                    {
                        AssetTracker.RemoveAssetAt(index);
                        saveChanges = true;
                    }
                    else
                    {
                        SetupButton(assetInfo);
                    }
                }
                
                if(saveChanges)
                {
                    AssetTracker.SaveChanges();
                }
            }

            if (_wizardData.SelectedAssetGUID != string.Empty)
            {
                VisualElement elem = _scrollingContainerContent.Children().FirstOrDefault(elem => elem.name == _wizardData.SelectedAssetGUID);
                if (elem == null)
                {
                    _wizardData.SelectedAssetGUID = string.Empty;
                }
                else
                {
                    SelectAsset(_wizardData.SelectedAssetGUID, false);
                }
            }

            _scrollingContainerContent.Sort(CompareByName);
            AssetTracker.ChangedAssets.Clear();

            // if scene has changed refresh inspector to show changes
            EditorSceneManager.activeSceneChangedInEditMode += (sceneOne, sceneTwo) => RefreshInspector();
        }

        // show asset's inspector and select it in assets folder
        private void SelectAsset(string guid, bool fromButton = true)
        {
            if (_currentInspector != null)
            {
                _inspectorContainer.Remove(_currentInspector);
                _currentInspector = null;
            }

            UnityEngine.Object asset = AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(guid), typeof(UnityEngine.Object));
            if (fromButton)
            {
                EditorUtility.FocusProjectWindow();
                Selection.activeObject = asset;
                _wizardData.SelectedAssetGUID = guid;
                EditorGUIUtility.PingObject(asset);
            }

            _currentInspector = Editor.CreateEditor(asset).CreateInspectorGUI();
            _inspectorContainer.Add(_currentInspector);
        }

        /// <summary>
        /// If there have been any changes, refresh asset inspector
        /// </summary>
        public void RefreshInspector()
        {
            if (_currentInspector != null && _wizardData.SelectedAssetGUID != null)
            {
                UnityEngine.Object asset = AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(_wizardData.SelectedAssetGUID), typeof(UnityEngine.Object));

                _inspectorContainer.Remove(_currentInspector);
                _currentInspector = null;

                _currentInspector = Editor.CreateEditor(asset).CreateInspectorGUI();
                _inspectorContainer.Add(_currentInspector);
            }
        }

        // callback event for clicking a button that references a scriptable object
        void SelectAssetCallback(ClickEvent evt, string guid)
        {
            SelectAsset(guid);
            _wizardData.SelectedAssetGUID = guid;
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
            VisualElement deleted = _scrollingContainerContent.Children().FirstOrDefault(elem => elem.name == assetInfo.GUID);
            if (deleted == null) return;
            
            _scrollingContainerContent.Remove(deleted);

            // if the removed element was selected remove the inspector for it
            if (deleted.name == _wizardData.SelectedAssetGUID)
            {
                Deselect();
            }
        }

        void UpdateMovedAssetButton(AssetInfo assetInfo)
        {
            VisualElement moved = _scrollingContainerContent.Children().FirstOrDefault(elem => elem.name == assetInfo.GUID);
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

                _scrollingContainerContent.Sort(CompareByName);
                RefreshInspector();
                AssetTracker.ChangedAssets.Clear();
            }
        }
    }
}