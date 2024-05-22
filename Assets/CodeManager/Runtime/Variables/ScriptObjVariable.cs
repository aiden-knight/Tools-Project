using System.Runtime.CompilerServices;
using UnityEngine;
using System;
using UnityEngine.SceneManagement;

namespace AidenK.CodeManager
{
    public abstract class ScriptObjVariable<T> : ScriptObjVariableBase
    {
        [SerializeField, ReadOnly]
        T _value;

        public T DefaultValue;

        public T Value
        {
            get => _value;
            set
            {
                if (!_value.Equals(value))
                {
                    _value = value;
                    if(_debug) Debug.Log(GetType().ToString() + " value changed to " + _value.ToString());
                    onValueChanged?.Invoke(_value);
                }
            }
        }

        [HideInInspector]
        public Action<T> onValueChanged;

        void OnEnable()
        {
            if(_resetOn == ResetOn.Play)
            {
                _value = DefaultValue;
            }
            else if(_resetOn == ResetOn.SceneLoad)
            {
                SceneManager.sceneLoaded += OnSceneLoaded;
            }
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            _value = DefaultValue;
        }
    }
}
