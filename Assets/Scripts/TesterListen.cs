using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TesterListen : MonoBehaviour
{
    public void Listen(Vector3 pos)
    {
        Debug.Log(pos);
    }

    public void TupleResponse((int, string) value)
    {
        Debug.Log(string.Format("Testing tuple, int: {0}, string: {1}", value.Item1, value.Item2));
    }
}
