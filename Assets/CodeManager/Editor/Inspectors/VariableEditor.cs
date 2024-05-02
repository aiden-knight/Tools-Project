using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

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
