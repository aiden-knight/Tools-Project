using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AidenK.CodeManager
{
    public class ScriptObjCollection<T> : ScriptObjCollectionBase
    {
        [SerializeField]
        List<T> values = new List<T>();
    }
}
