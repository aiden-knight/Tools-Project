using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace AidenK.CodeManager
{
    public abstract class ScriptObjListener<T> : MonoBehaviour
    {
        public ScriptObjEvent<T> ScriptableEvent;
        public UnityEvent<T> CalledEvent;

        void Awake()
        {
            ScriptableEvent.AddListener(this);
        }

        public void Invoke(T value)
        {
            CalledEvent?.Invoke(value);
        }
    }
}