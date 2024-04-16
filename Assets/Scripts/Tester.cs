using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tester : MonoBehaviour
{
    [SerializeField]
    FloatVariable m_Speed;

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log(m_Speed.Value);

        m_Speed.Value /= 5;
    }
}
