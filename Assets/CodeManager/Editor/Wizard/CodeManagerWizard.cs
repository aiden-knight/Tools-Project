using System;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using static Codice.Client.BaseCommands.Import.Commit;
using System.IO;

namespace AidenK.CodeManager
{
    public struct ChangedAssets
    {
        public List<string> reimported;
        public List<string> deleted;
        public List<(string movedTo, string movedFrom)> moved;
        
        public void Init()
        {
            reimported = new List<string>();
            deleted = new List<string>();
            moved = new List<(string, string)>();
        }

        public bool IsEmpty()
        {
            if (reimported == null || deleted == null || moved == null) return true;

            return !(reimported.Any() || deleted.Any() || moved.Any());
        }

        public void Clear()
        {
            reimported?.Clear();
            deleted?.Clear();
            moved?.Clear();
        }
    }

    public class CodeManagerAssetPostprocessor : AssetPostprocessor
    {
        static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            ChangedAssets changedAssets = new ChangedAssets();
            changedAssets.Init();
            
            foreach (string str in importedAssets)
            {
                Type type = AssetDatabase.GetMainAssetTypeAtPath(str);
                if (type.IsSubclassOf(typeof(ScriptObjVariableBase)) || type.IsSubclassOf(typeof(ScriptObjEventBase)))
                {
                    changedAssets.reimported.Add(str);
                }
            }

            foreach (string str in deletedAssets)
            {
                changedAssets.deleted.Add(str);
            }

            for (int i = 0; i < movedAssets.Length; i++)
            {
                Type type = AssetDatabase.GetMainAssetTypeAtPath(movedAssets[i]);
                if (type.IsSubclassOf(typeof(ScriptObjVariableBase)) || type.IsSubclassOf(typeof(ScriptObjEventBase)))
                {
                    changedAssets.moved.Add((movedAssets[i], movedFromAssetPaths[i]));
                }
            }
        }
    }

    public class CodeManagerWizard : EditorWindow
    {
        public static ChangedAssets AssetChanges;

        [SerializeField]
        private VisualTreeAsset m_VisualTreeAsset = default;
        [SerializeField]
        private StyleSheet uss;

        // Window creation and showing
        static CodeManagerWizard Instance;
        [MenuItem("Window/Code Manager/Wizard", priority = -5)]
        public static void ShowExample()
        {
            Instance = GetWindow<CodeManagerWizard>(false, "Code Manager", true);
        }

        VisualElement ScrollingContainerContent;

        void SetupButtonFromGUID(string guid)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            SetupButtonFromPath(path);
        }

        void SetupButtonFromPath(string path)
        {
            Button button = new Button();
            button.text = path.Substring("Assets/".Length);
            button.name = path;
            button.styleSheets.Add(uss);
            button.clicked += () => { SelectAsset(path); };
            //button.RegisterCallback()
            ScrollingContainerContent.Add(button);
        }
        public void CreateGUI()
        {
            // Each editor window contains a root VisualElement object
            VisualElement root = rootVisualElement;

            // Instantiate UXML
            VisualElement labelFromUXML = m_VisualTreeAsset.Instantiate();
            root.Add(labelFromUXML);

            var scrollV = root.Q<ScrollView>("ScriptableObjects");
            ScrollingContainerContent = scrollV.Q("unity-content-container");
            
            string[] varGuids = AssetDatabase.FindAssets("t:ScriptObjVariableBase", null);
            string[] eventGuids = AssetDatabase.FindAssets("t:ScriptObjEventBase", null);
            foreach (string guid in varGuids) SetupButtonFromGUID(guid);
            foreach (string guid in eventGuids) SetupButtonFromGUID(guid);

            AssetChanges.Clear();
        }

        public void SelectAsset(string path)
        {
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = AssetDatabase.LoadAssetAtPath(path, typeof(UnityEngine.Object));
        }

        public void OnGUI()
        {
            if(!AssetChanges.IsEmpty())
            {
                foreach(string reimported in AssetChanges.reimported)
                {
                    // if already exists don't add
                    if (ScrollingContainerContent.Children().Where(elem => elem.name == reimported).First() != null) continue;
                    
                    SetupButtonFromPath(reimported);
                }

                foreach(string deleted in AssetChanges.deleted)
                {
                    VisualElement elem = ScrollingContainerContent.Children().Where(elem => elem.name == deleted).First();
                    if(elem != null)
                    {
                        ScrollingContainerContent.Remove(elem);
                    }
                }

                foreach((string movedTo, string movedFrom) in AssetChanges.moved)
                {
                    VisualElement elem = ScrollingContainerContent.Children().Where(elem => elem.name == movedFrom).First();
                    if (elem == null) continue;

                    if(elem is Button button)
                    {
                        button.name = movedTo;
                        button.text = movedTo.Substring("Assets/".Length);
                        
                    }
                }

                string[] varGuids = AssetDatabase.FindAssets("t:ScriptObjVariableBase", null);
                string[] eventGuids = AssetDatabase.FindAssets("t:ScriptObjEventBase", null);
                foreach (string guid in varGuids) SetupButtonFromGUID(guid);
                foreach (string guid in eventGuids) SetupButtonFromGUID(guid);

                AssetChanges.Clear();
                Debug.Log("Rebuilt UI");
            }
        }
    }
}