using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AidenK.CodeManager;
public class Tester : MonoBehaviour
{
    [SerializeField]
    FloatReference m_Health;

    [SerializeField] ScriptObjVariable<float> m_Speed;
    [SerializeField]
    Vector3Event PositionUpdated;
    [SerializeField]
    List<int> ints = new List<int>();

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log(m_Health.Value);

        m_Health.Variable.Value /= 5;
        Vector3 v3 = new Vector3(m_Health.Value, 0.0f, m_Health.Value);
        PositionUpdated.Invoke(v3);
    }
}
