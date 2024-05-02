using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace AidenK.CodeManager
{
    [CustomEditor(typeof(ScriptObjVariableBase), true)]
    public class VariableEditorBase : Editor
    {
        public void SelectObject(ClickEvent evt, GameObject obj)
        {
            Selection.activeObject = obj;
            EditorApplication.ExecuteMenuItem("Window/General/Inspector");
            EditorGUIUtility.PingObject(obj);
        }

        VisualElement ScrollingContainerContent;
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

            string[] guids = AssetDatabase.FindAssets("AidenK.CodeManager.VariableEditor t:VisualTreeAsset");
            if (guids.Length > 1) Debug.LogError("Found more than one uxml file of given name: AidenK.CodeManager.VariableEditor");
            if (guids.Length == 0)
            {
                Debug.LogError("Could not find AidenK.CodeManager.VariableEditor uxml file");
                return new Label("Error with loading UXML on CreateInspectorGUI");
            }

            VisualTreeAsset visualTreeAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(AssetDatabase.GUIDToAssetPath(guids[0]));

            VisualElement uxmlElement = visualTreeAsset.Instantiate();
            root.Add(uxmlElement);

            var scrollV = root.Q<ScrollView>("References");
            ScrollingContainerContent = scrollV.Q("unity-content-container");

            List<GameObject> references = AssetFinder.FindReferences(serializedObject.targetObject);
            foreach (GameObject obj in references)
            {
                SetupButtonFromObject(obj);
            }

            return root;
        }

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
        }
    }
}
