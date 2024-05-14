using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UIElements;
using AssetProcessor = AidenK.CodeManager.CodeManagerAssetPostprocessor;
using Scene = UnityEngine.SceneManagement.Scene;

namespace AidenK.CodeManager
{
    public class ScriptObjEditor : Editor
    {
        [SerializeField]
        VisualTreeAsset ReferencesEditor = null;
        VisualElement ScrollingContainerContent;

        public void SelectObjectWithInspector(ClickEvent evt, Object obj)
        {
            SelectObject(evt, obj);
            EditorApplication.ExecuteMenuItem("Window/General/Inspector");
        }

        public void SelectObject(ClickEvent evt, Object obj)
        {
            Selection.activeObject = obj;
            EditorUtility.FocusProjectWindow();
            EditorGUIUtility.PingObject(obj);
        }

        void SetupButtonFromObject(string scenePath, string objectName)
        {
            Button button = new Button();

            string name = scenePath.Substring("Assets/".Length);
            name += " [" + objectName + "]";
            button.text = name;
            button.name = objectName;
            button.RegisterCallback<ClickEvent, Object>(SelectObject, AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePath));
            ScrollingContainerContent.Add(button);
        }

        void SetupButtonFromSceneObject(Object obj)
        {
            Button button = new Button();

            string name;
            if (obj is GameObject gameObj)
            {
                name = string.Format("{0} ", gameObj.scene.name);
            }
            else
            {
                return;
            }

            name += "[" + obj.name + "]";
            button.text = name;
            button.name = obj.name;
            button.RegisterCallback<ClickEvent, Object>(SelectObjectWithInspector, obj);
            ScrollingContainerContent.Add(button);
        }

        void SetupButtonFromPrefab(string path)
        {
            GameObject obj = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            Button button = new Button();

            string name = path.Substring("Assets/".Length);
            button.text = name;
            button.name = obj.name;
            button.RegisterCallback<ClickEvent, Object>(SelectObject, obj);
            ScrollingContainerContent.Add(button);
        }

        protected virtual VisualElement ExtraContent()
        {
            return null;
        }
            

        public override VisualElement CreateInspectorGUI()
        {
            VisualElement root = new VisualElement();
            root.Add(new IMGUIContainer(OnInspectorGUI));

            VisualElement extra = ExtraContent();
            if(extra != null) root.Add(extra);

            VisualElement uxmlElement = ReferencesEditor.Instantiate();
            root.Add(uxmlElement);

            var scrollV = root.Q<ScrollView>("References");
            if (scrollV == null)
            {
                Debug.LogError("Could not find ScrollView: References");
            }
            ScrollingContainerContent = scrollV.Q("unity-content-container");

            // get references from json
            AssetInfo assetInfo = AssetProcessor.GetAssetInfo(serializedObject.targetObject);
            bool showReferences = assetInfo != null && assetInfo.AssetReferencesGUIDs != null && assetInfo.SceneObjectReferences != null;
            if(showReferences)
            {
                // for each prefab
                foreach (string guid in assetInfo.AssetReferencesGUIDs)
                {
                    string path = AssetDatabase.GUIDToAssetPath(guid);
                    SetupButtonFromPrefab(path);
                }

                // get active scenes
                List<string> activeScenePaths = new List<string>();
                for(int i = 0; i < EditorSceneManager.sceneCount; i++)
                {
                    Scene scene = EditorSceneManager.GetSceneAt(i);
                    activeScenePaths.Add(scene.path);
                }
                // for each game object in scenes
                foreach (SceneObjectReference objectReference in assetInfo.SceneObjectReferences)
                {
                    string path = AssetDatabase.GUIDToAssetPath(objectReference.SceneGUID);
                    int sceneIndex = activeScenePaths.IndexOf(path);
                    if (sceneIndex == -1) // if object not in an active scene
                    {
                        SetupButtonFromObject(path, objectReference.ObjectName);
                        continue;
                    }

                    // object is in active scene so find it to reference
                    Scene scene = EditorSceneManager.GetSceneAt(sceneIndex);
                    Transform transform = null;
                    foreach(int index in objectReference.IndexesFromRoot)
                    {
                        if (transform == null)
                            transform = scene.GetRootGameObjects()[index].transform;
                        else
                            transform = transform.GetChild(index);
                    }

                    SetupButtonFromSceneObject(transform.gameObject);
                }

                ScrollingContainerContent.Sort(CodeManagerWizard.CompareByName);
            }

            return root;
        }

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
        }
    }
}