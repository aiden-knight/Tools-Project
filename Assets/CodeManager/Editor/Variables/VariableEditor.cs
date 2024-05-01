using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

namespace AidenK.CodeManager
{
    [CustomEditor(typeof(ScriptObjVariable<>))]
    public class VariableEditor<T> : Editor
    {
        SerializedProperty propertyDefaultValue;

        private void OnEnable()
        {
            propertyDefaultValue = serializedObject.FindProperty("DefaultValue");
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

            return root;
        }

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
        }
    }
}
