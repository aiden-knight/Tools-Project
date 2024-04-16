using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class EventListener : MonoBehaviour
{
    [SerializeField]
    FloatVariable Variable;

    [SerializeField]
    UnityEvent<float> Event;

    private void Awake()
    {
        Variable.onValueChanged.AddListener(Invoke);
    }

    void Invoke(float v)
    {
        Event?.Invoke(v);
    }
}
