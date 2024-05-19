using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UIElements;
using Scene = UnityEngine.SceneManagement.Scene;

namespace AidenK.CodeManager
{
    public class ScriptObjEditor : Editor
    {
        [SerializeField]
        VisualTreeAsset _referencesEditor = null;
        VisualElement _scrollingContainerContent;

        internal struct SceneAssetCallbackData
        {
            public string Path;
            public Object SceneObject;
            public SceneObjectReference SceneObjectReference;
        }

        void SelectObjectWithInspector(ClickEvent evt, Object obj)
        {
            SelectObject(evt, obj);
            EditorApplication.ExecuteMenuItem("Window/General/Inspector");
        }

        void SelectObject(ClickEvent evt, Object obj)
        {
            Selection.activeObject = obj;
            EditorUtility.FocusProjectWindow();
            EditorGUIUtility.PingObject(obj);
        }

        void SelectScene(ClickEvent evt, SceneAssetCallbackData data)
        {
            if(Selection.activeObject == data.SceneObject)
            {
                EditorSceneManager.OpenScene(data.Path);
                Object sceneObject = GetObjectInScene(data.SceneObjectReference);
                SelectObjectWithInspector(evt, sceneObject);
            }
            else
            {
                Selection.activeObject = data.SceneObject;
                EditorUtility.FocusProjectWindow();
                EditorGUIUtility.PingObject(data.SceneObject);
            }
        }

        void SetupButtonFromSceneAsset(string scenePath, SceneObjectReference objectReference)
        {
            Button button = new Button();

            string name = scenePath.Substring("Assets/".Length);
            name += " [" + objectReference.ObjectName + "]";
            button.text = name;
            button.name = objectReference.ObjectName;

            SceneAssetCallbackData sceneObject = new SceneAssetCallbackData()
            {
                Path = scenePath,
                SceneObject = AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePath),
                SceneObjectReference = objectReference
            };

            button.RegisterCallback<ClickEvent, SceneAssetCallbackData>(SelectScene, sceneObject);
            _scrollingContainerContent.Add(button);
        }

        void SetupButtonFromSceneObject(Object sceneObject)
        {
            Button button = new Button();

            string name;
            if (sceneObject is GameObject gameObj)
            {
                name = string.Format("{0} ", gameObj.scene.name);
            }
            else
            {
                return;
            }

            name += "[" + sceneObject.name + "]";
            button.text = name;
            button.name = sceneObject.name;
            button.RegisterCallback<ClickEvent, Object>(SelectObjectWithInspector, sceneObject);
            _scrollingContainerContent.Add(button);
        }

        void SetupButtonFromPrefab(string path)
        {
            GameObject obj = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            Button button = new Button();

            string name = path.Substring("Assets/".Length);
            button.text = name;
            button.name = obj.name;
            button.RegisterCallback<ClickEvent, Object>(SelectObject, obj);
            _scrollingContainerContent.Add(button);
        }

        protected virtual VisualElement ExtraContent()
        {
            return null;
        }
        
        Object GetObjectInScene(SceneObjectReference objectReference, int sceneIndex = 0)
        {
            // object is in active scene so find it to reference
            Scene scene = EditorSceneManager.GetSceneAt(sceneIndex);
            Transform transform = null;
            foreach (int index in objectReference.IndexesFromRoot)
            {
                if (transform == null)
                {
                    GameObject[] rootObjects = scene.GetRootGameObjects();
                    if (index >= rootObjects.Length) break;
                    else transform = scene.GetRootGameObjects()[index].transform;

                }
                else
                {
                    if (index >= transform.childCount)
                    {
                        transform = null;
                        break;
                    }
                    transform = transform.GetChild(index);
                }
            }

            if (transform != null && transform.name == objectReference.ObjectName)
            {
                return transform.gameObject;
            }
            else { return null; }
        }

        public override VisualElement CreateInspectorGUI()
        {
            VisualElement root = new VisualElement();
            root.Add(new IMGUIContainer(OnInspectorGUI));

            VisualElement extra = ExtraContent();
            if(extra != null) root.Add(extra);

            VisualElement uxmlElement = _referencesEditor.Instantiate();
            root.Add(uxmlElement);

            var scrollV = root.Q<ScrollView>("References");
            if (scrollV == null)
            {
                Debug.LogError("Could not find ScrollView: References");
            }
            _scrollingContainerContent = scrollV.Q("unity-content-container");

            // get references from json
            AssetInfo assetInfo = AssetTracker.GetAssetInfo(serializedObject.targetObject);
            if(assetInfo != null)
            {
                // if the asset has prefab references
                if(assetInfo.AssetReferencesGUIDs != null)
                {
                    // for each prefab
                    foreach (string guid in assetInfo.AssetReferencesGUIDs)
                    {
                        string path = AssetDatabase.GUIDToAssetPath(guid);
                        SetupButtonFromPrefab(path);
                    }
                }
                
                // if the asset has scene object references
                if(assetInfo.SceneObjectReferences != null)
                {
                    // get active scenes
                    List<string> activeScenePaths = new List<string>();
                    for (int i = 0; i < EditorSceneManager.sceneCount; i++)
                    {
                        Scene scene = EditorSceneManager.GetSceneAt(i);
                        activeScenePaths.Add(scene.path);
                    }

                    // for each game object in scenes
                    foreach (SceneObjectReference objectReference in assetInfo.SceneObjectReferences)
                    {
                        string scenePath = AssetDatabase.GUIDToAssetPath(objectReference.SceneGUID);
                        int sceneIndex = activeScenePaths.IndexOf(scenePath);
                        if (sceneIndex == -1) // if object not in an active scene
                        {
                            SetupButtonFromSceneAsset(scenePath, objectReference);
                            continue;
                        }


                        Object sceneObject = GetObjectInScene(objectReference, sceneIndex);
                        if (sceneObject != null) SetupButtonFromSceneObject(sceneObject);
                    }
                }

                _scrollingContainerContent.Sort(CodeManagerWizard.CompareByName);
            }

            return root;
        }

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
        }
    }
}