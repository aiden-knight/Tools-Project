using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace AidenK.CodeManager
{
    public abstract class ScriptObjVariable<T> : ScriptObjVariableBase
    {
        [SerializeField, ReadOnly]
        T m_value;

        public T DefaultValue;

        public T Value
        {
            get => m_value;
            set
            {
                if (!m_value.Equals(value))
                {
                    m_value = value;
                    if(debug) Debug.Log(GetType().ToString() + " value changed to " + m_value.ToString());
                    onValueChanged?.Invoke(m_value);
                }
            }
        }

        [HideInInspector]
        public UnityEvent<T> onValueChanged;

        void OnEnable()
        {
            if(resetOn == ResetOn.Play)
            {
                m_value = DefaultValue;
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
            m_value = DefaultValue;
        }
    }
}
