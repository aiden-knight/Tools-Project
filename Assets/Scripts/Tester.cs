using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AidenK.CodeManager;
public class Tester : MonoBehaviour
{
    [SerializeField]
    FloatReference m_Speed;

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log(m_Speed.Value);

        m_Speed.Variable.Value /= 5;
    }
}
