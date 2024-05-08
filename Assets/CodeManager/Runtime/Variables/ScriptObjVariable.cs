using UnityEngine;
using UnityEngine.Events;

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
                    if(debug) Debug.Log("Value Changed");
                    onValueChanged?.Invoke(m_value);
                }
            }
        }

        [HideInInspector]
        public UnityEvent<T> onValueChanged;

        void OnEnable()
        {
            m_value = DefaultValue;
        }
    }
}
