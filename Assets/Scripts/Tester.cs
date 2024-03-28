using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tester : MonoBehaviour
{
    [SerializeField]
    Watchable<int> m_Health;

    [SerializeField]
    FloatVariable m_Speed;

    // Start is called before the first frame update
    void Start()
    {
        m_Speed.m_OnValueChanged.AddListener(OnSpeedChanged);
        m_Speed.Value = 10;
    }

    void OnHealthChanged(int health)
    {
        Debug.Log($"Health changed registered {health}");
    }

    public void OnSpeedChanged(float speed)
    {
        Debug.Log(speed);
    }
}
