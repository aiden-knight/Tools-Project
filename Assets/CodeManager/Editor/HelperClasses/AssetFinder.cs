using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Scene = UnityEngine.SceneManagement.Scene;
using Type = System.Type;

namespace AidenK.CodeManager
{
    public static class AssetFinder
    {
        /// <summary>
        /// Types that functions are looking for references to 
        /// </summary>
        private static readonly Type[] s_watchedTypes =
            {
                typeof(ScriptObjVariableBase),
                typeof(ScriptObjEventBase),
                typeof(ScriptObjCollectionBase),
        };

        /// <summary>
        /// Loops over active scenes to return list of them
        /// </summary>
        /// <returns>List of the GUIDs of active scenes</returns>
        private static List<string> GetActiveScenes()
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
        /// Given a type check if it is a subclass of watched type
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private static bool IsWatched(Type type)
        {
            foreach (Type watched in s_watchedTypes)
            {
                if (type.IsSubclassOf(watched))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Loops over the components in a game object to determine if it contains a reference to the object
        /// </summary>
        /// <param name="gameObject">GameObject to look in</param>
        /// <param name="ignoredPaths">Paths to ignore should they already be on a prefab</param>
        /// <returns>Whether or not the gameobject was found</returns>
        private static List<string> GetReferencesInGameObject(GameObject gameObject, List<string> ignoredPaths = null)
        {
            List<string> paths = new List<string>();

            // look for target object in component's serialized fields
            Component[] componentsArray = gameObject.GetComponents(typeof(MonoBehaviour));
            foreach (Component component in componentsArray)
            {
                if (component == null) continue;

                SerializedObject serializedObject = new SerializedObject(component);

                // iterate over component's serialized fields
                SerializedProperty iterator = serializedObject.GetIterator();
                while (iterator.NextVisible(true))
                {
                    if (iterator.propertyType != SerializedPropertyType.ObjectReference) continue; // if not reference to object
                    if (iterator.objectReferenceValue == null) continue; // if reference value is null

                    string path = AssetDatabase.GetAssetPath(iterator.objectReferenceValue);
                    if (path == null || path == string.Empty) continue; // if path doesn't exist

                    Type type = iterator.objectReferenceValue.GetType();                    
                    if (IsWatched(type))
                    {
                        if (paths.Contains(path)) continue; // if already found
                        if (ignoredPaths != null && ignoredPaths.Contains(path)) continue; // for prefabs ignore duplicates from parent / other children
                        paths.Add(path);
                    }
                }
            }

            return paths;
        }

        /// <summary>
        /// Given a guid of a prefab, find all the references to scriptable object types in it (including children)
        /// </summary>
        /// <param name="guid"></param>
        public static void GetPrefabReferenceSingle(string guid)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(guid));

            List<string> references = GetReferencesInGameObject(prefab);
            foreach (Transform child in prefab.GetComponentsInChildren<Transform>())
            {
                List<string> childRef = GetReferencesInGameObject(child.gameObject, references);
                if (childRef.Count > 0)
                {
                    references.AddRange(childRef);
                }
            }

            AssetTracker.AddAllPrefabReferences(references, guid);
        }

        /// <summary>
        /// Checks all prefabs for references
        /// </summary>
        private static void GetPrefabReferences()
        {
            string[] allPrefabGUIDs = AssetDatabase.FindAssets("t:Prefab");
            foreach (string guid in allPrefabGUIDs)
            {
                GetPrefabReferenceSingle(guid);
            }
        }

        /// <summary>
        /// Loops over all objects in scene to check for references
        /// </summary>
        /// <param name="toFind">Object to find reference to</param>
        /// <returns>List of the objects in scene that have references</returns>
        public static void GetSceneReferences()
        {
            // All game objects in all active scenes and assets
            foreach (Object obj in Resources.FindObjectsOfTypeAll<Object>())
            {
                GameObject gameObject = obj as GameObject;
                if (gameObject == null) continue;
                if (gameObject.hideFlags == HideFlags.NotEditable || gameObject.hideFlags == HideFlags.HideAndDontSave) continue;
                if (EditorUtility.IsPersistent(gameObject)) continue;

                List<string> references = GetReferencesInGameObject(gameObject);
                if(references.Count > 0)
                {
                    AssetTracker.AddAllSceneObjectReferences(references, GetSceneObjectReference(gameObject));
                }
            }
        }

        /// <summary>
        /// Converts scene object to scene object reference
        /// </summary>
        /// <returns>Scene object reference of scene object</returns>
        private static SceneObjectReference GetSceneObjectReference(GameObject sceneObject)
        {
            string sceneGUID = AssetDatabase.AssetPathToGUID(sceneObject.scene.path);
            List<int> indexesFromRoot = new List<int>();

            // work up from gameobject to scene root to find indexes
            Transform transform = sceneObject.transform;
            while (transform != null)
            {
                indexesFromRoot.Add(transform.GetSiblingIndex());
                transform = transform.parent;
            }
            indexesFromRoot.Reverse();

            // Construct scene reference
            SceneObjectReference sceneObjectReference = new SceneObjectReference()
            {
                ObjectName = sceneObject.name,
                SceneGUID = sceneGUID,
                IndexesFromRoot = indexesFromRoot
            };
            return sceneObjectReference;
        }

        /// <summary>
        /// Slow function to generate all the references to objects to store it on the AssetTracker
        /// </summary>
        public static void FindReferences()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

            List<string> activeSceneGUIDs = GetActiveScenes();

            GetPrefabReferences();

            // find references in all scenes
            string[] sceneGUIDS = AssetDatabase.FindAssets("t:SceneAsset");
            OpenSceneMode mode = OpenSceneMode.Single;
            foreach (string sceneGUID in sceneGUIDS) // open all scenes
            {
                string path = AssetDatabase.GUIDToAssetPath(sceneGUID);
                EditorSceneManager.OpenScene(path, mode);
                mode = OpenSceneMode.Additive;
            }
            GetSceneReferences();

            // reopen previous active scenes
            mode = OpenSceneMode.Single;
            foreach(string sceneGUID in activeSceneGUIDs)
            {
                string path = AssetDatabase.GUIDToAssetPath(sceneGUID);
                EditorSceneManager.OpenScene(path, mode);

                mode = OpenSceneMode.Additive;
            }
        }
    }
}

