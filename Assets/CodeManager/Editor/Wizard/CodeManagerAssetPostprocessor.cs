using Type = System.Type;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Newtonsoft.Json;
using Codice.Client.BaseCommands.BranchExplorer;
using System.IO;
using System.Reflection;

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


    // Handles the moving, deleting, renaming and creation of assets
    public class CodeManagerAssetPostprocessor : AssetPostprocessor
    {
        /// <summary> List of asset changes, contains type of change and asset info of asset it refers to </summary>
        public static List<(AssetChanges, AssetInfo)> ChangedAssets = new List<(AssetChanges, AssetInfo)>();
        public static bool IsChanges()
        {
            return ChangedAssets.Count > 0;
        }

        public const string jsonFilter = "AidenK.CodeManager.AssetInfo t:TextAsset";
        public const string jsonFileName = "AidenK.CodeManager.AssetInfo.asset";
        public static readonly string[] jsonFolder = { "Assets/AidenK.CodeManager/Settings/" };

        /// <summary> List of all scriptable object's asset info </summary>
        public static List<AssetInfo> AssetInfos = new List<AssetInfo>();
        /// <summary> Whether AssetInfos have been loaded </summary>
        static bool loaded = false;

        /// <summary> Array of types that the processor should watch for </summary>
        static readonly Type[] watchedTypes =
            {
                typeof(ScriptObjVariableBase),
                typeof(ScriptObjEventBase),
                typeof(ScriptObjCollectionBase),
            };

        
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
                if(isOfWatchedType && !AssetInfos.Any(info => info.GUID == guid))
                {
                    AssetInfo info = new AssetInfo()
                    {
                        GUID = guid,
                        Path = path,
                        AssetReferencesGUIDs = null,
                        SceneObjectReferences = null
                    };
                    ChangedAssets.Add((AssetChanges.Created, info));
                    AssetInfos.Add(info);
                    changes = true;
                }
            }

            // any assets deleted
            foreach (string str in deletedAssets)
            {
                string guid = AssetDatabase.AssetPathToGUID(str);
                AssetInfo info = AssetInfos.FirstOrDefault(info => info.GUID == guid);

                if (info != null)
                {
                    ChangedAssets.Add((AssetChanges.Deleted, info));
                    AssetInfos.Remove(info);
                    changes = true;
                }
            }

            // any assets that have moved in assets
            for (int i = 0; i < movedAssets.Length; i++)
            {
                string guid = AssetDatabase.AssetPathToGUID(movedAssets[i]);
                AssetInfo info = AssetInfos.FirstOrDefault(info => info.GUID == guid);

                if (info != null)
                {
                    info.Path = movedAssets[i];
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
