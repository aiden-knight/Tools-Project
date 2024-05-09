using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AidenK.CodeManager
{
    public abstract class ScriptObjCollection<T> : ScriptObjCollectionBase
    {
        public List<T> Items = new List<T>();

        public void Add(T item)
        {
            Items.Add(item);
            if (debug) Debug.Log(item.ToString() + " added to collection");
        }

        public void AddUnique(T item)
        {
            if (!Items.Contains(item))
            {
                Add(item);
            }
        }

        public void Remove(T item)
        {
            Items.Remove(item);
            if (debug) Debug.Log(item.ToString() + " removed from collection");
        }
    }
}
