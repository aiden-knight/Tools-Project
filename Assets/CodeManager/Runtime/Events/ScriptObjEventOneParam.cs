using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AidenK.CodeManager
{
    public abstract class ScriptObjEventOneParam<T> : ScriptObjEventBase
    {
        List<ScriptObjListenerOneParam<T>> _listeners = new ();

        [SerializeField] T _debugValue;

        private void OnEnable()
        {
            CallInvoke = () => { Invoke(_debugValue); };
        }

        public void Invoke(T value)
        {
            // Iterate backwards in case event involves removing themself as a listener
            for(int i = _listeners.Count - 1; i >= 0; i--)
            {
                _listeners[i].Invoke(value);
            }
        }

        public void AddListener(ScriptObjListenerOneParam<T> listener)
        {
            _listeners.Add(listener);
        }

        public void RemoveListener(ScriptObjListenerOneParam<T> listener)
        {
            _listeners.Remove(listener);
        }
    }
}