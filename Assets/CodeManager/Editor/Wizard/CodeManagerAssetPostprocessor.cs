using Type = System.Type;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Newtonsoft.Json;
using Codice.Client.BaseCommands.BranchExplorer;
using System.IO;

namespace AidenK.CodeManager
{
    /// <summary>Asset info for scriptable object types</summary> 
    [System.Serializable]
    public class AssetInfo
    {
        /// <summary>
        /// path to the asset
        /// </summary>
        public string path;
        /// <summary>
        /// Globally unique identifier of the asset
        /// </summary>
        public string GUID;
        /// <summary>
        /// Guids of other assets that reference the asset
        /// </summary>
        public List<string> AssetReferencesGUIDs;
        /// <summary>
        /// Instance IDs of game objects in scenes that reference asset
        /// </summary>
        public List<int> GameObjectInstanceIDs;
    }
    // https://stackoverflow.com/questions/70782260/unique-unchanged-identifier-for-gameobject-in-unity
    // Instance IDs not saved between editor sessions / play mode

    public enum AssetChanges
    {
        Created,
        Deleted,
        Moved
    }


    // Handles the moving, deleting, renaming and creation of assets
    public class CodeManagerAssetPostprocessor : AssetPostprocessor
    {
        public static List<(AssetChanges, AssetInfo)> ChangedAssets = new List<(AssetChanges, AssetInfo)>();
        public static List<AssetInfo> assetInfos = new List<AssetInfo>();
        static bool loaded = false;

        public static bool IsChanges()
        {
            return ChangedAssets.Count > 0;
        }

        public const string jsonFilter = "AidenK.CodeManager.AssetInfo t:TextAsset";
        public const string jsonFileName = "AidenK.CodeManager.AssetInfo.asset";
        public static readonly string[] jsonFolder = { "Assets/AidenK.CodeManager/Settings/" };
        public static bool CheckLoad()
        {
            if (loaded) return true;

            string[] guids = AssetDatabase.FindAssets(jsonFilter, jsonFolder);
            if (guids.Length == 0) return false;

            TextAsset jsonData = AssetDatabase.LoadAssetAtPath<TextAsset>(AssetDatabase.GUIDToAssetPath(guids[0]));
            if (jsonData == null) return false;

            string json = jsonData.text;
            assetInfos = JsonConvert.DeserializeObject<List<AssetInfo>>(json);
            if (assetInfos == null) return false;

            loaded = true;
            return true;
        }

        public static void SaveChanges()
        {
            string[] guids = AssetDatabase.FindAssets(jsonFilter, jsonFolder);
            if (guids.Length == 0)
            {
                Debug.LogWarning(string.Format("Expected [{0}] to exist in folder: {1}", jsonFilter, jsonFolder[0]));
                return;
            }

            string path = AssetDatabase.GUIDToAssetPath(guids[0]);
            TextAsset jsonData = AssetDatabase.LoadAssetAtPath<TextAsset>(path);
            if (jsonData == null)
            {
                Debug.LogWarning(string.Format("Expected [{0}] to not load as null", jsonFilter));
                return;
            }

            jsonData = new TextAsset(JsonConvert.SerializeObject(assetInfos));
            AssetDatabase.CreateAsset(jsonData, path);
            AssetDatabase.SaveAssets();
        }

        public static List<Object> FindReferences(Object obj)
        {
            string assetGUID = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(obj));

            AssetInfo assetInfo = assetInfos.FirstOrDefault(info => info.GUID == assetGUID);
            if (assetInfo == null) return null;
            if (assetInfo.AssetReferencesGUIDs == null || assetInfo.GameObjectInstanceIDs == null) return null;

            List<Object> references = new List<Object>();
            foreach(string guid in assetInfo.AssetReferencesGUIDs)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                references.Add(AssetDatabase.LoadAssetAtPath<Object>(path));
            }
            foreach(int instanceID in assetInfo.GameObjectInstanceIDs)
            {
                references.Add(EditorUtility.InstanceIDToObject(instanceID));
            }

            return references;
        }

        public static void UpdateReferences(List<GameObject> objects, List<GameObject> prefabs, string assetGUID)
        {
            AssetInfo assetInfo = assetInfos.FirstOrDefault(info => info.GUID == assetGUID);
            if (assetInfo == null) return;

            List<string> prefabGUIDS = new List<string>();
            foreach(GameObject prefab in prefabs)
            {
                string prefabGUID = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(prefab));
                prefabGUIDS.Add(prefabGUID);
            }

            List<int> instanceIDs = new List<int>();
            foreach(GameObject obj in objects)
            {
                instanceIDs.Add(obj.GetInstanceID());
            }

            assetInfo.AssetReferencesGUIDs = prefabGUIDS;
            assetInfo.GameObjectInstanceIDs = instanceIDs;
            SaveChanges();
        }

        static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            bool jsonLoaded = CheckLoad();
            if(!jsonLoaded)
            {
                Debug.Log("Cancelled OnPostProcess as json file not loaded");
                return;
            }
            bool changes = false;
            
            // types to check for
            Type[] watchedTypes = 
            {
                typeof(ScriptObjVariableBase),
                typeof(ScriptObjEventBase),
                typeof(ScriptObjCollectionBase),
            };

            // created or modified assets (occurs when assets have moved)
            foreach (string path in importedAssets)
            {
                string guid = AssetDatabase.AssetPathToGUID(path);
                Type type = AssetDatabase.GetMainAssetTypeAtPath(path);

                bool isOfWatchedType = false;
                foreach (Type watchedType in watchedTypes)
                {
                    if(type.IsSubclassOf(watchedType))
                    {
                        isOfWatchedType = true;
                        break;
                    }
                }

                // check performance of this vs foreach to determine order
                if(isOfWatchedType && !assetInfos.Any(info => info.GUID == guid))
                {
                    AssetInfo info = new AssetInfo()
                    {
                        GUID = guid,
                        path = path,
                        AssetReferencesGUIDs = null,
                        GameObjectInstanceIDs = null
                    };
                    ChangedAssets.Add((AssetChanges.Created, info));
                    assetInfos.Add(info);
                    changes = true;
                }
            }

            // any assets deleted
            foreach (string str in deletedAssets)
            {
                string guid = AssetDatabase.AssetPathToGUID(str);
                AssetInfo info = assetInfos.FirstOrDefault(info => info.GUID == guid);

                if (info != null)
                {
                    ChangedAssets.Add((AssetChanges.Deleted, info));
                    assetInfos.Remove(info);
                    changes = true;
                }
            }

            // any assets that have moved in assets
            for (int i = 0; i < movedAssets.Length; i++)
            {
                string guid = AssetDatabase.AssetPathToGUID(movedAssets[i]);
                AssetInfo info = assetInfos.FirstOrDefault(info => info.GUID == guid);

                if (info != null)
                {
                    info.path = movedAssets[i];
                    ChangedAssets.Add((AssetChanges.Moved, info));
                    changes = true;
                }
            }


            if (changes)
            {
                SaveChanges();
            }
        }
    }
}
