using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AidenK.CodeManager
{
    [CreateAssetMenu(menuName = "Code Manager/Events/No Parameter Event", order = 0)]
    public class NoParamEvent : ScriptObjEventBase
    {
        List<NoParamListener> _listeners = new();

        private void OnEnable()
        {
            CallInvoke = Invoke;
        }

        public void Invoke()
        {
            // Iterate backwards in case event involves removing themself as a listener
            for (int i = _listeners.Count - 1; i >= 0; i--)
            {
                _listeners[i].Invoke();
            }
        }

        public void AddListener(NoParamListener listener)
        {
            _listeners.Add(listener);
        }

        public void RemoveListener(NoParamListener listener)
        {
            _listeners.Remove(listener);
        }
    }
}
