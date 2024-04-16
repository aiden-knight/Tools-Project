using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(menuName = "Code Manager/Float Scriptable Object")]
public class FloatVariable : ScriptableObject
{
    float m_value;

    [SerializeField]
    public float DefaultValue;

    public float Value
    {
        get => m_value;
        set
        {
            if(m_value != value)
            {
                m_value = value;
                onValueChanged?.Invoke(m_value);
            }
        }
    }

    public UnityEvent<float> onValueChanged;

    private void OnEnable()
    {
        m_value = DefaultValue;
    }
}
