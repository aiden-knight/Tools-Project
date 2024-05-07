using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AidenK.CodeManager;
public class Tester : MonoBehaviour
{
    [SerializeField]
    FloatReference m_Health;

    [SerializeField]
    Vector3Event PositionUpdated;

    [SerializeField]
    IntStringEvent TupleEvent;

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log(m_Health.Value);

        m_Health.Variable.Value /= 5;
        Vector3 v3 = new Vector3(m_Health.Value, 0.0f, m_Health.Value);
        PositionUpdated.Invoke(v3);

        TupleEvent.Invoke(((int)m_Health.Value, "I'm part of a tuple"));
    }
}
