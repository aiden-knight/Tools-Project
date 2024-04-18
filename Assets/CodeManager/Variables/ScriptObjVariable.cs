using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace AidenK.CodeManager
{
    public abstract class ScriptObjVariable<T> : ScriptableObject
    {
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
                    onValueChanged?.Invoke(m_value);
                }
            }
        }

        public UnityEvent<T> onValueChanged;

        void OnEnable()
        {
            m_value = DefaultValue;
        }
    }
}
