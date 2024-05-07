using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace AidenK.CodeManager
{
    public static class AssetFinder
    {
        public static List<GameObject> FindReferences(Object toFind)
        {
            List<GameObject> objects = new();

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
                        objects.Add(obj);
                        break;
                    }
                }
            }

            return objects;
        }
    }
}

