using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AidenK.CodeManager
{
    public abstract class ScriptObjCollection<T> : ScriptObjCollectionBase
    {
        [SerializeField, Tooltip("Whether to clear item list when you start play")]
        bool _reset;

        [SerializeField]
        List<T> _items = new List<T>();
        public List<T> Items { get => _items; }

        public void Add(T item)
        {
            Items.Add(item);
            if (_debug) Debug.Log(item.ToString() + " added to collection");
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
            if (_debug) Debug.Log(item.ToString() + " removed from collection");
        }

        private void OnEnable()
        {
            if (_reset)
            {
                _items.Clear();
            }
        }
    }
}
