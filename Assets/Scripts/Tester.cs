using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tester : MonoBehaviour
{
    [SerializeField]
    Watchable<int> m_Health;

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log($"Update health to 5");
        m_Health.V = 5;

        int testInt = m_Health.V * 2;
        Debug.Log(testInt);

        m_Health.Watch(OnHealthChanged);
        Debug.Log($"Update health to 3");
        m_Health.V = 3;

        m_Health.UnWatch(OnHealthChanged);
        Debug.Log($"Update health to 7");
        m_Health.V = 7;

    }

    void OnHealthChanged(int health)
    {
        Debug.Log($"Health changed registered {health}");
    }
}
