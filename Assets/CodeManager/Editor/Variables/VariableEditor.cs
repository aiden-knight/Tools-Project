using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UIElements;

namespace AidenK.CodeManager
{
    [CustomEditor(typeof(ScriptObjVariable<>))]
    public class VariableEditor<T> : Editor
    {
        List<GameObject> FindReferences()
        {
            List<GameObject> objects = new();

            foreach (GameObject obj in Resources.FindObjectsOfTypeAll<GameObject>().ToArray())
            {
                Component[] componentsArray = obj.GetComponents<Component>();
                foreach(Component component in componentsArray)
                {
                    SerializedObject serializedObject = new SerializedObject(component);
                    SerializedProperty iterator = serializedObject.GetIterator();

                    bool found = false;
                    while(iterator.NextVisible(true))
                    {
                        if(iterator.propertyType != SerializedPropertyType.ObjectReference)
                        {
                            continue;
                        }

                        var first = iterator.objectReferenceValue;
                        var second = this.serializedObject.targetObject;
                        if(first == second)
                        {
                            found = true;
                            break;
                        }
                    }

                    if (found)
                    {
                        objects.Add(obj);
                        break;
                    }
                }
            }

            return objects;
        }

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
            if(obj.scene.name != null)
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

            List<GameObject> references = FindReferences();
            foreach(GameObject obj in references)
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
