using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using AssetProcessor = AidenK.CodeManager.CodeManagerAssetPostprocessor;
using Scene = UnityEngine.SceneManagement.Scene;

namespace AidenK.CodeManager
{
    public static class AssetFinder
    {
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
            }
            return prefabGUIDs;
        }

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

        static List<SceneObjectReference> GetSceneObjectReferences(List<Object> sceneObjects)
        {
            List<SceneObjectReference> sceneObjectReferences = new List<SceneObjectReference>();
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

        public static void FindReferences(Object toFind)
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

            List<string> activeSceneGUIDs = GetActiveScenes();

            List<string> prefabGUIDS = GetPrefabReferences(toFind);
            
            // find references in all scenes
            string[] sceneGUIDS = AssetDatabase.FindAssets("t:SceneAsset");
            List<SceneObjectReference> sceneObjectReferences = new List<SceneObjectReference>();
            foreach (string sceneGUID in sceneGUIDS)
            {
                string path = AssetDatabase.GUIDToAssetPath(sceneGUID);
                EditorSceneManager.OpenScene(path, OpenSceneMode.Single);

                List<Object> sceneObjects = GetSceneReferences(toFind);
                sceneObjectReferences.AddRange(GetSceneObjectReferences(sceneObjects));
            }

            // reopen previous active scenes
            OpenSceneMode mode = OpenSceneMode.Single;
            foreach(string sceneGUID in activeSceneGUIDs)
            {
                string path = AssetDatabase.GUIDToAssetPath(sceneGUID);
                EditorSceneManager.OpenScene(path, mode);

                mode = OpenSceneMode.Additive;
            }

            string assetGUID = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(toFind));
            AssetProcessor.UpdateReferences(prefabGUIDS, sceneObjectReferences, assetGUID);
        }
    }
}

