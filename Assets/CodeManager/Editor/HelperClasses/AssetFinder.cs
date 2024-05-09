using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using AssetProcessor = AidenK.CodeManager.CodeManagerAssetPostprocessor;

namespace AidenK.CodeManager
{
        public static class AssetFinder
    {
        public static List<GameObject> FindReferences(Object toFind)
        {
            List<GameObject> objects = new();
            List<GameObject> prefabs = new();
            string assetGUID = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(toFind));

            // All game objects in all scenes and 
            foreach (GameObject obj in Resources.FindObjectsOfTypeAll<GameObject>().ToArray())
            {
                Component[] componentsArray = obj.GetComponents<Component>();
                foreach (Component component in componentsArray)
                {
                    if (component == null) continue;

                    SerializedObject serializedObject = new SerializedObject(component);
                    SerializedProperty iterator = serializedObject.GetIterator();

                    bool found = false;
                    while (iterator.NextVisible(true))
                    {
                        if (iterator.propertyType != SerializedPropertyType.ObjectReference)
                        {
                            continue;
                        }

                        if (iterator.objectReferenceValue == toFind)
                        {
                            found = true;
                            break;
                        }
                    }

                    if (found)
                    {
                        if (obj.scene.name != null)
                            objects.Add(obj);
                        else
                            prefabs.Add(obj);
                        break;
                    }
                }
            }

            AssetProcessor.UpdateReferences(objects, prefabs, assetGUID);

            objects.AddRange(prefabs);
            return objects;
        }
    }
}

