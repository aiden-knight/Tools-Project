using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace AidenK.CodeManager
{
    public class NoParamListener : ScriptObjListenerBase
    {
        public NoParamEvent Event;
        public UnityEvent Response;

        void OnEnable()
        {
            Event.AddListener(this);
        }

        private void OnDisable()
        {
            Event.RemoveListener(this);
        }

        public void Invoke()
        {
            Response?.Invoke();
        }
    }
}