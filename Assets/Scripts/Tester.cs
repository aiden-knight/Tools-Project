using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AidenK.CodeManager;
public class Tester : MonoBehaviour
{
    [SerializeField]
    FloatReference m_Speed;
    [SerializeField]
    Vector3Event PositionUpdated;

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log(m_Speed.Value);

        m_Speed.Variable.Value /= 5;
        Vector3 v3 = new Vector3(m_Speed.Value, 0.0f, m_Speed.Value);
        PositionUpdated.Invoke(v3);
    }
}
