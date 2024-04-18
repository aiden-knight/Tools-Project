using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace AidenK.CodeManager
{
    public abstract class ScriptObjListener<T> : MonoBehaviour
    {
        public ScriptObjEvent<T> Event;
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