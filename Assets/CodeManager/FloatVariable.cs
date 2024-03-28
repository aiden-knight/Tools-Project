using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class FloatEvent : UnityEvent<float>
{

}

[CreateAssetMenu(menuName = "Code Manager/Float Scriptable Object")]
public class FloatVariable : ScriptableObject
{
    [SerializeField]
    float m_value;

    public float Value
    {
        get => m_value;
        set
        {
            if(m_value != value)
            {
                m_value = value;
                m_OnValueChanged?.Invoke(m_value);
            }
        }
    }

    [SerializeField]
    public UnityEvent<float> m_OnValueChanged;
    public FloatVariable(float value)
    {
        m_value = value;
    }
}
