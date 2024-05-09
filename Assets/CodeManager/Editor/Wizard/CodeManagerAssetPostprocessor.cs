using System;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Newtonsoft.Json;

namespace AidenK.CodeManager
{
    // Holds lists to store all changed assets
    public struct ChangedAssets
    {
        public List<string> reimported;
        public List<string> deleted;
        public List<(string movedTo, string movedFrom)> moved;

        public void Init()
        {
            reimported = new List<string>();
            deleted = new List<string>();
            moved = new List<(string, string)>();
        }

        public bool IsEmpty()
        {
            if (reimported == null || deleted == null || moved == null) return true;

            return !(reimported.Any() || deleted.Any() || moved.Any());
        }

        public void Clear()
        {
            reimported?.Clear();
            deleted?.Clear();
            moved?.Clear();
        }

        public void Add(ChangedAssets other)
        {
            reimported.AddRange(other.reimported);
            deleted.AddRange(other.deleted);
            moved.AddRange(other.moved);
        }
    }

    // Handles the moving, deleting, renaming and creation of assets
    public class CodeManagerAssetPostprocessor : AssetPostprocessor
    {
        public static ChangedAssets AssetChanges;
        public static List<AssetInfo> assetInfos = new List<AssetInfo>();
        static bool loaded = false;

        public static bool IsChanges()
        {
            return !(AssetChanges.IsEmpty());
        }

        public static bool CheckLoad()
        {
            if (loaded) return true;

            string[] guids = AssetDatabase.FindAssets("AidenK.CodeManager.AssetInfo t:TextAsset", new[] { "Assets/AidenK.CodeManager/Settings" });
            if (guids.Length == 0) return false;

            TextAsset jsonData = AssetDatabase.LoadAssetAtPath<TextAsset>(AssetDatabase.GUIDToAssetPath(guids[0]));
            if (jsonData == null) return false;

            string json = jsonData.text;
            assetInfos = JsonConvert.DeserializeObject<List<AssetInfo>>(json);
            if (assetInfos == null) return false;

            loaded = true;
            return true;
        }

        static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            bool jsonLoaded = CheckLoad();

            ChangedAssets changedAssets = new ChangedAssets();
            changedAssets.Init();
            
            // types to check for
            Type[] watchedTypes = 
            {
                typeof(ScriptObjVariableBase),
                typeof(ScriptObjEventBase),
                typeof(ScriptObjCollectionBase),
            };

            // created or modified assets (occurs when assets have moved)
            foreach (string str in importedAssets)
            {
                Type type = AssetDatabase.GetMainAssetTypeAtPath(str);
                foreach (Type watchedType in watchedTypes)
                {
                    if(type.IsSubclassOf(watchedType))
                    {
                        changedAssets.reimported.Add(str);
                        break;
                    }
                }
            }

            // any assets deleted
            foreach (string str in deletedAssets)
            {
                changedAssets.deleted.Add(str);
            }

            // any assets that have moved in assets
            for (int i = 0; i < movedAssets.Length; i++)
            {
                Type type = AssetDatabase.GetMainAssetTypeAtPath(movedAssets[i]);
                foreach (Type watchedType in watchedTypes)
                {
                    if (type.IsSubclassOf(watchedType))
                    {
                        changedAssets.moved.Add((movedAssets[i], movedFromAssetPaths[i]));
                        break;
                    }
                }
            }


            if (!changedAssets.IsEmpty())
            {
                if (AssetChanges.IsEmpty())
                {
                    AssetChanges = changedAssets;
                }
                else
                {
                    AssetChanges.Add(changedAssets);
                }
            }
        }
    }
}
