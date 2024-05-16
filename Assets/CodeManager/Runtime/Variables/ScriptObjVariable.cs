using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Events;
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
                    if(debug) Debug.Log(GetType().ToString() + " value changed to " + _value.ToString());
                    onValueChanged?.Invoke(_value);
                }
            }
        }

        [HideInInspector]
        public UnityEvent<T> onValueChanged;

        void OnEnable()
        {
            if(resetOn == ResetOn.Play)
            {
                _value = DefaultValue;
            }
            else if(resetOn == ResetOn.SceneLoad)
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
