using Type = System.Type;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Newtonsoft.Json;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

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
       
        // Values to use when attempting to interface with the json file
        public const string JsonFilter = "AidenK.CodeManager.AssetInfo t:TextAsset";
        public const string JsonFileName = "AidenK.CodeManager.AssetInfo.asset";
        public static readonly string[] JsonFolder = { "Assets/AidenK.CodeManager/Settings/" };

        /// <summary> List of all scriptable object's asset info </summary>
        static List<AssetInfo> _assetInfos = new List<AssetInfo>();

        public static bool FindingAssetReferences = false;

        /// <summary> Whether AssetInfos have been loaded </summary>
        private static bool s_loaded = false;

        /// <summary> Array of types that the processor should watch for </summary>
        private static readonly Type[] s_watchedTypes =
            {
                typeof(ScriptObjVariableBase),
                typeof(ScriptObjEventBase),
                typeof(ScriptObjCollectionBase),
                typeof(SceneAsset),
                typeof(GameObject),
            };

        /// <summary>
        /// Whether there was tracked changes to assets so PostProcess should save them
        /// </summary>
        static bool s_changes = false;

        // must have same order as above array
        private enum TypeIndex
        {
            Untracked = -1,
            ScriptObj = 1,
            ScriptObjEnd = 2,
            Scene,
            Prefab,
        }

        public static int GetAssetCount()
        {
            return _assetInfos.Count;
        }

        public static AssetInfo GetAssetAt(int index)
        {
            if(_assetInfos.Count <= index)
            {
                return null;
            }
            else
            {
                return _assetInfos[index];
            }
        }

        public static void RemoveAssetAt(int index)
        {
            _assetInfos.RemoveAt(index);
        }

        public static bool IsChanges()
        {
            return ChangedAssets.Count > 0;
        }

        public static void FindReferences()
        {
            FindingAssetReferences = true;
            AssetFinder.FindReferences();
            SaveChanges();
            FindingAssetReferences = false;
        }

        static bool ExistsInList<T>(List<T> list, T item)
        {
            return list.Contains(item);
        }

        static bool CheckEqualReference(SceneObjectReference first, SceneObjectReference second)
        {
            if (first.SceneGUID != second.SceneGUID) return false;
            return first.IndexesFromRoot.SequenceEqual(second.IndexesFromRoot);
        }

        static bool ExistsInList(List<SceneObjectReference> list, SceneObjectReference item)
        {
            return list.Any(sceneRef => CheckEqualReference(sceneRef, item));
        }

        /// <summary>
        /// Given a prefab GUID and paths of assets it references, add guid to asset's references list if not already contained
        /// </summary>
        /// <param name="paths"></param>
        /// <param name="prefabGUID"></param>
        public static void AddAllPrefabReferences(List<string> paths, string prefabGUID)
        {
            foreach (string path in paths)
            {
                AssetInfo info = _assetInfos.FirstOrDefault(info => info.Path == path);
                if (info == null) continue;

                if (info.AssetReferencesGUIDs == null)
                {
                    info.AssetReferencesGUIDs = new List<string> { prefabGUID };
                }
                else
                {
                    if (ExistsInList(info.AssetReferencesGUIDs, prefabGUID)) return;
                    info.AssetReferencesGUIDs.Add(prefabGUID);
                }
            }
        }

        public static void AddAllSceneObjectReferences(List<string> paths, SceneObjectReference reference)
        {
            foreach (string path in paths)
            {
                AssetInfo info = _assetInfos.FirstOrDefault(info => info.Path == path);
                if (info == null) continue;

                if (info.SceneObjectReferences == null)
                {
                    info.SceneObjectReferences = new List<SceneObjectReference> { reference };
                }
                else
                {
                    if (ExistsInList(info.SceneObjectReferences, reference)) return;
                    info.SceneObjectReferences.Add(reference);
                }
            }
        }

        /// <summary>
        /// Checks whether json is loaded or attemps to load
        /// </summary>
        /// <returns>Whether the json is or was loaded</returns>
        public static bool CheckLoad()
        {
            if (s_loaded) return true;

            string[] guids = AssetDatabase.FindAssets(JsonFilter, JsonFolder);
            if (guids.Length == 0) return false;

            TextAsset jsonData = AssetDatabase.LoadAssetAtPath<TextAsset>(AssetDatabase.GUIDToAssetPath(guids[0]));
            if (jsonData == null) return false;

            string json = jsonData.text;
            _assetInfos = JsonConvert.DeserializeObject<List<AssetInfo>>(json);
            if (_assetInfos == null) return false;

            s_loaded = true;
            return true;
        }

        /// <summary>
        /// Saves the changes to AssetInfos to the json file
        /// </summary>
        public static void SaveChanges()
        {
            string[] guids = AssetDatabase.FindAssets(JsonFilter, JsonFolder);
            if (guids.Length == 0)
            {
                Debug.LogWarning(string.Format("Expected [{0}] to exist in folder: {1}", JsonFilter, JsonFolder[0]));
                return;
            }

            string path = AssetDatabase.GUIDToAssetPath(guids[0]);
            TextAsset jsonData = AssetDatabase.LoadAssetAtPath<TextAsset>(path);
            if (jsonData == null)
            {
                Debug.LogWarning(string.Format("Expected [{0}] to not load as null", JsonFilter));
                return;
            }

            jsonData = new TextAsset(JsonConvert.SerializeObject(_assetInfos));
            AssetDatabase.CreateAsset(jsonData, path);
            AssetDatabase.SaveAssets();
            EditorWindow.GetWindow<CodeManagerWizard>().RefreshInspector();
        }

        /// <summary>
        /// Gets the asset info of the object if it exists
        /// </summary>
        /// <param name="obj">Object to find asset info for</param>
        /// <returns>Asset Info or null if it doesn't exist</returns>
        public static AssetInfo GetAssetInfo(Object obj)
        {
            string assetGUID = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(obj));

            AssetInfo assetInfo = _assetInfos.FirstOrDefault(info => info.GUID == assetGUID);
            return assetInfo;
        }

        private static TypeIndex GetType(string path)
        {
            Type assetType = AssetDatabase.GetMainAssetTypeAtPath(path);
            for (int index = 0; index < s_watchedTypes.Length; index++)
            {
                if (index <= ((int)TypeIndex.ScriptObjEnd) && assetType.IsSubclassOf(s_watchedTypes[index]))
                {
                    return TypeIndex.ScriptObj;
                }
                else if (assetType == s_watchedTypes[index])
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
                    if (_assetInfos.Any(info => info.GUID == guid)) return;
                    AssetInfo info = new AssetInfo()
                    {
                        GUID = guid,
                        Path = path,
                        AssetReferencesGUIDs = null,
                        SceneObjectReferences = null
                    };
                    ChangedAssets.Add((AssetChanges.Created, info));
                    _assetInfos.Add(info);
                    break;
                case TypeIndex.Prefab:
                    AssetFinder.GetPrefabReferenceSingle(guid);
                    break;

                default:
                    return;
            }
            s_changes = true;
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
            if (guid == string.Empty) return;

            TypeIndex type = GetType(path);

            switch (type)
            {
                case TypeIndex.ScriptObj:
                    AssetInfo info = _assetInfos.FirstOrDefault(info => info.GUID == guid);
                    if (info == null) return;

                    ChangedAssets.Add((AssetChanges.Deleted, info));
                    _assetInfos.Remove(info);
                    break;
                case TypeIndex.Scene:
                    RemoveSceneReferences(path);
                    break;
                case TypeIndex.Prefab:
                    RemovePrefabReferences(path);
                    break;
                default:
                    return;
            }

            s_changes = true;
        }

        private static void HandleMoved(string path)
        {
            string guid = AssetDatabase.AssetPathToGUID(path);
            TypeIndex type = GetType(path);

            switch (type)
            {
                case TypeIndex.ScriptObj:
                    AssetInfo info = _assetInfos.FirstOrDefault(info => info.GUID == guid);
                    if (info == null) return;

                    info.Path = path;
                    ChangedAssets.Add((AssetChanges.Moved, info));
                    break;

                default:
                    return;
            }

            s_changes = true;
        }

        static void RemovePrefabReferences(string path)
        {
            string prefabGUID = AssetDatabase.AssetPathToGUID(path);
            foreach (AssetInfo info in _assetInfos)
            {
                if (info.AssetReferencesGUIDs == null) continue;

                int count = 0;
                int startIndex = -1;
                int length = info.AssetReferencesGUIDs.Count;
                for (int index = 0; index < length; ++index)
                {
                    if (info.AssetReferencesGUIDs[index] != prefabGUID)
                    {
                        if (startIndex != -1) break;
                        else continue;
                    }

                    if (startIndex == -1) startIndex = index;
                    count++;
                }

                if(startIndex != -1)
                {
                    info.AssetReferencesGUIDs.RemoveRange(startIndex, count);
                }
            }
        }

        static void RemoveSceneReferences(string path)
        {
            string sceneGUID = AssetDatabase.AssetPathToGUID(path);
            foreach (AssetInfo info in _assetInfos)
            {
                if (info.SceneObjectReferences == null) continue;

                int count = 0;
                int startIndex = -1;
                int length = info.SceneObjectReferences.Count;
                for (int index = 0; index < length; ++index)
                {
                    SceneObjectReference sceneRef = info.SceneObjectReferences[index];
                    if (sceneRef.SceneGUID != sceneGUID)
                    {
                        if (startIndex != -1) break;
                        else continue;
                    }

                    if (startIndex == -1) startIndex = index;
                    count++;
                }

                if( startIndex != -1)
                {
                    info.SceneObjectReferences.RemoveRange(startIndex, count);
                }
            }
        }

        public static void HandleSceneSaved(string path)
        {
            if(!CheckLoad()) return;

            Scene scene = EditorSceneManager.GetSceneByPath(path);
            if (!scene.IsValid()) return;

            RemoveSceneReferences(path);

            AssetFinder.GetSceneReferences();
            SaveChanges();
        }

        public static void HandlePrefabSaved(string path)
        {
            if (!CheckLoad()) return;

            RemovePrefabReferences(path);

            AssetFinder.GetPrefabReferenceSingle(AssetDatabase.AssetPathToGUID(path));
            SaveChanges();
        }

        // Handles any moved assets
        private static void HandleAllMoved(string[] movedAssets)
        {
            foreach (string path in movedAssets)
            {
                HandleMoved(path);
            }
        }

        // When inheriting from AssetPostprocessor, implementing this function captures changes to assets
        private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            bool jsonLoaded = CheckLoad();
            if(!jsonLoaded)
            {
                Debug.Log("Cancelled OnPostprocess as json file not loaded, try opening code manager window");
                return;
            }

            HandleAllImported(importedAssets);
            HandleAllMoved(movedAssets);
            
            if (s_changes)
            {
                SaveChanges();
                s_changes = false;
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
                    Type type = AssetDatabase.GetMainAssetTypeAtPath(path);
                    if(type == typeof(SceneAsset))
                    {
                        AssetTracker.HandleSceneSaved(path);
                    }
                    else if(type == typeof(GameObject))
                    {
                        AssetTracker.HandlePrefabSaved(path);
                    }
                }
            }
            
            return paths;
        }
    }
}
