using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AidenK.CodeManager
{
    [CreateAssetMenu(menuName = "Code Manager/Events/No Parameter Event", order = 0)]
    public class EventNoParam : ScriptObjEventBase
    {
        List<ListenerNoParam> m_listeners = new();

        public void Invoke()
        {
            // Iterate backwards in case event involves removing themself as a listener
            for (int i = m_listeners.Count - 1; i >= 0; i--)
            {
                m_listeners[i].Invoke();
            }
        }

        public void AddListener(ListenerNoParam listener)
        {
            m_listeners.Add(listener);
        }

        public void RemoveListener(ListenerNoParam listener)
        {
            m_listeners.Remove(listener);
        }
    }
}
