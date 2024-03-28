using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class Watchable<T>
{
    T m_value;

    public T V
    {
        get => m_value;

        set
        {
            m_value = value;
            m_OnChanged?.Invoke(m_value);
        }
    }

    UnityAction<T> m_OnChanged;


    public void Watch(UnityAction<T> ChangedCallback)
    {
        m_OnChanged += ChangedCallback;
    }

    public void UnWatch(UnityAction<T> ChangedCallback)
    {
        m_OnChanged -= ChangedCallback;
    }

    public Watchable(T value)
    {
        m_value = value;
    }
}
