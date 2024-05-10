using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using AssetProcessor = AidenK.CodeManager.CodeManagerAssetPostprocessor;

namespace AidenK.CodeManager
{
    public class ScriptObjEditor : Editor
    {
        [SerializeField]
        VisualTreeAsset ReferencesEditor = null;
        VisualElement ScrollingContainerContent;
        public void SelectObject(ClickEvent evt, Object obj)
        {
            Selection.activeObject = obj;
            EditorApplication.ExecuteMenuItem("Window/General/Inspector");
            EditorGUIUtility.PingObject(obj);
        }

        void SetupButtonFromObject(Object obj)
        {
            Button button = new Button();

            string name;
            if (obj is GameObject gameObj)
            {
                if(gameObj.scene.name != null)
                {
                    name = "Scene: ";
                }
                else
                {
                    name = "Prefab: ";
                }
            }
            else
            {
                name = "Unknown: ";
            }
            name += "[" + obj.name + "]";
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

            AssetInfo assetInfo = AssetProcessor.GetReferences(serializedObject.targetObject);
            if(assetInfo!= null)
            {
                List<Object> references = new List<Object>();
                foreach (string guid in assetInfo.AssetReferencesGUIDs)
                {
                    string path = AssetDatabase.GUIDToAssetPath(guid);
                    references.Add(AssetDatabase.LoadAssetAtPath<Object>(path));
                }
                foreach (SceneObjectReference instanceID in assetInfo.SceneObjectReferences)
                {
                    
                }

                foreach (Object obj in references)
                {
                    SetupButtonFromObject(obj);
                }
            }
            else
            {
                List<Object> gameObjRefs = AssetFinder.FindReferences(serializedObject.targetObject);
                foreach (Object obj in gameObjRefs)
                {
                    SetupButtonFromObject(obj);
                }
            }


            return root;
        }

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
        }
    }
}