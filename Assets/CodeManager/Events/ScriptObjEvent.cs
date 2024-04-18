using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AidenK.CodeManager
{
    public abstract class ScriptObjEvent<T> : ScriptableObject
    {
        List<ScriptObjListener<T>> m_listeners = new ();

        public void Invoke(T value)
        {
            // Iterate backwards in case event involves removing themself as a listener
            for(int i = m_listeners.Count - 1; i >= 0; i--)
            {
                m_listeners[i].Invoke(value);
            }
        }

        public void AddListener(ScriptObjListener<T> listener)
        {
            m_listeners.Add(listener);
        }

        public void RemoveListener(ScriptObjListener<T> listener)
        {
            m_listeners.Remove(listener);
        }
    }
}