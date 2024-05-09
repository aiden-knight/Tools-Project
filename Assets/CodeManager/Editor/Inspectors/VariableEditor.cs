using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using AssetProcessor = AidenK.CodeManager.CodeManagerAssetPostprocessor;

namespace AidenK.CodeManager
{
    [CustomEditor(typeof(ScriptObjVariableBase), true)]
    public class VariableEditor : Editor
    {
        [SerializeField]
        VisualTreeAsset ReferencesEditor = null;
        VisualElement ScrollingContainerContent;
        public void SelectObject(ClickEvent evt, GameObject obj)
        {
            Selection.activeObject = obj;
            EditorApplication.ExecuteMenuItem("Window/General/Inspector");
            EditorGUIUtility.PingObject(obj);
        }

        void SetupButtonFromObject(GameObject obj)
        {
            Button button = new Button();

            string name;
            if (obj.scene.name != null)
            {
                name = "Scene: ";
            }
            else
            {
                name = "Prefab: ";
            }
            name += "[" + obj.name + "]";
            button.text = name;
            button.name = name;
            //button.styleSheets.Add(uss);
            button.RegisterCallback<ClickEvent, GameObject>(SelectObject, obj);
            ScrollingContainerContent.Add(button);
        }

        public override VisualElement CreateInspectorGUI()
        {
            VisualElement root = new VisualElement();
            root.Add(new IMGUIContainer(OnInspectorGUI));

            VisualElement uxmlElement = ReferencesEditor.Instantiate();
            root.Add(uxmlElement);

            var scrollV = root.Q<ScrollView>("References");
            if(scrollV == null)
            {
                Debug.LogError("Could not find ScrollView: References");
            }
            ScrollingContainerContent = scrollV.Q("unity-content-container");

            List<Object> references = AssetProcessor.FindReferences(serializedObject.targetObject);
            if(references == null)
            {
                List<GameObject> gameObjRefs = AssetFinder.FindReferences(serializedObject.targetObject);
                foreach(GameObject obj in gameObjRefs)
                {
                   SetupButtonFromObject(obj);
                }
            }
            else
            {
                foreach (Object obj in references)
                {
                    if (obj is GameObject gameObj)
                    {
                        SetupButtonFromObject(gameObj);
                    }
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
