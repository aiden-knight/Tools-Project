using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace AidenK.CodeManager
{
    public abstract class ScriptObjListenerOneParam<T> : ScriptObjListenerBase
    {
        public ScriptObjEventOneParam<T> Event;
        public UnityEvent<T> Response;

        void OnEnable()
        {
            Event.AddListener(this);
        }

        private void OnDisable()
        {
            Event.RemoveListener(this);
        }

        public void Invoke(T value)
        {
            Response?.Invoke(value);
        }
    }
}