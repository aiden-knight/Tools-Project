using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AidenK.CodeManager
{
    public abstract class ScriptObjCollectionBase : ScriptableObject
    {
        [SerializeField]
        protected bool _debug = false;
    }
}
