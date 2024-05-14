using Type = System.Type;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Newtonsoft.Json;

namespace AidenK.CodeManager
{
    [System.Serializable]
    public class SceneObjectReference
    {
        public string ObjectName;
        public string SceneGUID;
        public List<int> IndexesFromRoot;
    }

    /// <summary>Asset info for scriptable object types</summary> 
    [System.Serializable]
    public class AssetInfo
    {
        /// <summary>
        /// path to the asset
        /// </summary>
        public string Path;
        /// <summary>
        /// Globally unique identifier of the asset
        /// </summary>
        public string GUID;
        /// <summary>
        /// Guids of other assets that reference the asset
        /// </summary>
        public List<string> AssetReferencesGUIDs;
        /// <summary>
        /// Scene object reference for objects in scene that have reference to the asset
        /// </summary>
        public List<SceneObjectReference> SceneObjectReferences;
    }

    public enum AssetChanges
    {
        Created,
        Deleted,
        Moved
    }

    /// <summary>
    /// Tracks assets and their changes
    /// </summary>
    internal class AssetTracker : AssetPostprocessor
    {
        /// <summary> List of asset changes, contains type of change and asset info of asset it refers to </summary>
        public static List<(AssetChanges, AssetInfo)> ChangedAssets = new List<(AssetChanges, AssetInfo)>();
        public static bool IsChanges()
        {
            return ChangedAssets.Count > 0;
        }

        // Values to use when attempting to reference the json file
        public const string jsonFilter = "AidenK.CodeManager.AssetInfo t:TextAsset";
        public const string jsonFileName = "AidenK.CodeManager.AssetInfo.asset";
        public static readonly string[] jsonFolder = { "Assets/AidenK.CodeManager/Settings/" };

        /// <summary> List of all scriptable object's asset info </summary>
        public static List<AssetInfo> AssetInfos = new List<AssetInfo>();

        public static bool FindingAssetReferences = false;

        /// <summary> Whether AssetInfos have been loaded </summary>
        private static bool loaded = false;

        /// <summary> Array of types that the processor should watch for </summary>
        private static readonly Type[] WatchedTypes =
            {
                typeof(ScriptObjVariableBase),
                typeof(ScriptObjEventBase),
                typeof(ScriptObjCollectionBase),
                typeof(SceneAsset),
                typeof(GameObject),
            };
        // must have same order as above array
        private enum TypeIndex
        {
            Untracked = -1,
            ScriptObj = 1,
            ScriptObjEnd = 2,
            Scene,
            Prefab,
        }

        /// <summary>
        /// Checks whether json is loaded or attemps to load
        /// </summary>
        /// <returns>Whether the json is or was loaded</returns>
        public static bool CheckLoad()
        {
            if (loaded) return true;

            string[] guids = AssetDatabase.FindAssets(jsonFilter, jsonFolder);
            if (guids.Length == 0) return false;

            TextAsset jsonData = AssetDatabase.LoadAssetAtPath<TextAsset>(AssetDatabase.GUIDToAssetPath(guids[0]));
            if (jsonData == null) return false;

            string json = jsonData.text;
            AssetInfos = JsonConvert.DeserializeObject<List<AssetInfo>>(json);
            if (AssetInfos == null) return false;

            loaded = true;
            return true;
        }

        /// <summary>
        /// Saves the changes to AssetInfos to the json file
        /// </summary>
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

            jsonData = new TextAsset(JsonConvert.SerializeObject(AssetInfos));
            AssetDatabase.CreateAsset(jsonData, path);
            AssetDatabase.SaveAssets();
        }

        /// <summary>
        /// Gets the asset info of the object if it exists
        /// </summary>
        /// <param name="obj">Object to find asset info for</param>
        /// <returns>Asset Info or null if it doesn't exist</returns>
        public static AssetInfo GetAssetInfo(Object obj)
        {
            string assetGUID = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(obj));

            AssetInfo assetInfo = AssetInfos.FirstOrDefault(info => info.GUID == assetGUID);
            return assetInfo;
        }

        private static TypeIndex GetType(string path)
        {
            Type assetType = AssetDatabase.GetMainAssetTypeAtPath(path);
            for (int index = 0; index < WatchedTypes.Length; index++)
            {
                if (index <= ((int)TypeIndex.ScriptObjEnd) && assetType.IsSubclassOf(WatchedTypes[index]))
                {
                    return TypeIndex.ScriptObj;
                }
                else if (assetType == WatchedTypes[index])
                {
                    return (TypeIndex)index;
                }
            }
            return TypeIndex.Untracked;
        }

        private static void HandleImported(string path)
        {
            string guid = AssetDatabase.AssetPathToGUID(path);
            TypeIndex type = GetType(path);

            switch (type)
            {
                case TypeIndex.ScriptObj: // If type is scriptable object add if doesn't exist
                    if (AssetInfos.Any(info => info.GUID == guid)) return;
                    AssetInfo info = new AssetInfo()
                    {
                        GUID = guid,
                        Path = path,
                        AssetReferencesGUIDs = null,
                        SceneObjectReferences = null
                    };
                    ChangedAssets.Add((AssetChanges.Created, info));
                    AssetInfos.Add(info);
                    break;

                default:
                    return;
            }
            changes = true;
        }

        // Handle created or modified assets (also occurs when assets have moved)
        public static void HandleAllImported(string[] importedAssets)
        {
            foreach (string path in importedAssets)
            {
                HandleImported(path);
            }
        }

        // Handles deleted assets
        public static void HandleDeleted(string path)
        {
            string guid = AssetDatabase.AssetPathToGUID(path);
            TypeIndex type = GetType(path);

            switch (type)
            {
                case TypeIndex.ScriptObj:
                    AssetInfo info = AssetInfos.FirstOrDefault(info => info.GUID == guid);
                    if (info == null) return;

                    ChangedAssets.Add((AssetChanges.Deleted, info));
                    AssetInfos.Remove(info);
                    break;

                default:
                    return;
            }

            changes = true;
        }

        private static void HandleMoved(string path)
        {
            string guid = AssetDatabase.AssetPathToGUID(path);
            TypeIndex type = GetType(path);

            switch (type)
            {
                case TypeIndex.ScriptObj:
                    AssetInfo info = AssetInfos.FirstOrDefault(info => info.GUID == guid);
                    if (info == null) return;

                    info.Path = path;
                    ChangedAssets.Add((AssetChanges.Moved, info));
                    break;

                default:
                    return;
            }

            changes = true;
        }

        // Handles any moved assets
        private static void HandleAllMoved(string[] movedAssets)
        {
            foreach (string path in movedAssets)
            {
                HandleMoved(path);
            }
        }

        // Whether there was changes
        static bool changes = false;
        // When inheriting from AssetPostprocessor, implementing this function captures changes to assets
        private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            bool jsonLoaded = CheckLoad();
            if(!jsonLoaded)
            {
                Debug.Log("Cancelled OnPostprocess as json file not loaded");
                return;
            }

            HandleAllImported(importedAssets);
            HandleAllMoved(movedAssets);
            
            if (changes)
            {
                SaveChanges();
                changes = false;
            }
        }
    }

    public class AssetModifier : AssetModificationProcessor
    {
        private static AssetDeleteResult OnWillDeleteAsset(string path, RemoveAssetOptions opt)
        {
            AssetTracker.HandleDeleted(path);
            return AssetDeleteResult.DidNotDelete;
        }

        private static string[] OnWillSaveAssets(string[] paths)
        {
            if(!AssetTracker.FindingAssetReferences)
            {
                foreach (string path in paths)
                {
                    Debug.Log(path);
                }
            }
            
            return paths;
        }
    }
}
