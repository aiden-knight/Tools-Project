using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Scene = UnityEngine.SceneManagement.Scene;

namespace AidenK.CodeManager
{
    public static class AssetFinder
    {
        /// <summary>
        /// Loops over active scenes to return list of them
        /// </summary>
        /// <returns>List of the GUIDs of active scenes</returns>
        static List<string> GetActiveScenes()
        {
            List<string> activeScenesGUIDs = new List<string>();
            int sceneCount = EditorSceneManager.sceneCount;
            for (int i = sceneCount - 1; i >= 0; i--)
            {
                Scene scene = EditorSceneManager.GetSceneAt(i);
                string guid = AssetDatabase.AssetPathToGUID(scene.path);
                activeScenesGUIDs.Add(guid);
            }

            return activeScenesGUIDs;
        }

        /// <summary>
        /// Loops over the components in a game object to determine if it contains a reference to the object
        /// </summary>
        /// <param name="toFind">Object to find reference to</param>
        /// <param name="gameObject">GameObject to look in</param>
        /// <returns>Whether or not the gameobject was found</returns>
        static bool ReferenceInGameObject(Object toFind, GameObject gameObject)
        {
            // look for target object in component's serialized fields
            Component[] componentsArray = gameObject.GetComponents<Component>();
            foreach (Component component in componentsArray)
            {
                if (component == null) continue;

                SerializedObject serializedObject = new SerializedObject(component);

                // iterate over component's serialized fields
                SerializedProperty iterator = serializedObject.GetIterator();
                while (iterator.NextVisible(true))
                {
                    if (iterator.propertyType != SerializedPropertyType.ObjectReference) continue;

                    if (iterator.objectReferenceValue == toFind)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Checks all prefabs for a reference to the  object
        /// </summary>
        /// <param name="toFind">Object to find reference to</param>
        /// <returns>List of the GUIDs of the prefabs that have references</returns>
        static List<string> GetPrefabReferences(Object toFind)
        {
            string[] allPrefabGUIDs = AssetDatabase.FindAssets("t:Prefab");
            List<string> prefabGUIDs = new List<string>();

            foreach (string guid in allPrefabGUIDs)
            {
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(guid));

                if(ReferenceInGameObject(toFind, prefab))
                {
                    prefabGUIDs.Add(guid);
                }
                else
                {
                    foreach (Transform child in prefab.GetComponentsInChildren<Transform>())
                    {
                        if (ReferenceInGameObject(toFind, child.gameObject))
                        {
                            prefabGUIDs.Add(guid);
                            break;
                        }
                    }
                }                
            }
            return prefabGUIDs;
        }

        /// <summary>
        /// Loops over all objects in scene to check for references
        /// </summary>
        /// <param name="toFind">Object to find reference to</param>
        /// <returns>List of the objects in scene that have references</returns>
        static List<Object> GetSceneReferences(Object toFind)
        {
            List<Object> references = new List<Object>();

            // All game objects in all active scenes and assets
            foreach (Object obj in Resources.FindObjectsOfTypeAll<Object>())
            {
                GameObject gameObject = obj as GameObject;
                if (gameObject == null) continue;
                if (gameObject.hideFlags == HideFlags.NotEditable || gameObject.hideFlags == HideFlags.HideAndDontSave) continue;
                if (EditorUtility.IsPersistent(gameObject)) continue;

                if(ReferenceInGameObject(toFind, gameObject))
                {
                    references.Add(obj);
                }
            }

            return references;
        }

        /// <summary>
        /// Converts list of scene objects to a list of scene object references
        /// </summary>
        /// <param name="sceneObjects">List of scene objects</param>
        /// <returns>List of scene object references</returns>
        static List<SceneObjectReference> GetSceneObjectReferences(List<Object> sceneObjects)
        {
            List<SceneObjectReference> sceneObjectReferences = new List<SceneObjectReference>(sceneObjects.Count);
            foreach (Object obj in sceneObjects)
            {
                GameObject gameObj = obj as GameObject;

                string sceneGUID = AssetDatabase.AssetPathToGUID(gameObj.scene.path);
                List<int> indexesFromRoot = new List<int>();

                Transform transform = gameObj.transform;
                while (transform != null)
                {
                    indexesFromRoot.Add(transform.GetSiblingIndex());
                    transform = transform.parent;
                }
                indexesFromRoot.Reverse();

                // Construct scene reference
                SceneObjectReference sceneObjectReference = new SceneObjectReference()
                {
                    ObjectName = gameObj.name,
                    SceneGUID = sceneGUID,
                    IndexesFromRoot = indexesFromRoot
                };
                sceneObjectReferences.Add(sceneObjectReference);
            }
            return sceneObjectReferences;
        }

        /// <summary>
        /// Slow function to generate all the references to an object to store it on the AssetPostprocessor
        /// </summary>
        /// <param name="toFind">Object to find</param>
        public static (List<string>, List<SceneObjectReference>) FindReferences(Object toFind)
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return (null, null);

            List<string> activeSceneGUIDs = GetActiveScenes();

            List<string> prefabGUIDS = GetPrefabReferences(toFind);

            // find references in all scenes
            string[] sceneGUIDS = AssetDatabase.FindAssets("t:SceneAsset");
            OpenSceneMode mode = OpenSceneMode.Single;
            foreach (string sceneGUID in sceneGUIDS) // open all scenes
            {
                string path = AssetDatabase.GUIDToAssetPath(sceneGUID);
                EditorSceneManager.OpenScene(path, mode);
                mode = OpenSceneMode.Additive;
            }
            List<Object> sceneObjects = GetSceneReferences(toFind);
            List<SceneObjectReference> sceneObjectReferences = GetSceneObjectReferences(sceneObjects);

            // reopen previous active scenes
            mode = OpenSceneMode.Single;
            foreach(string sceneGUID in activeSceneGUIDs)
            {
                string path = AssetDatabase.GUIDToAssetPath(sceneGUID);
                EditorSceneManager.OpenScene(path, mode);

                mode = OpenSceneMode.Additive;
            }

            return (prefabGUIDS, sceneObjectReferences);
        }
    }
}

