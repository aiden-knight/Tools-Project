using UnityEngine;
using UnityEngine.Events;

namespace AidenK.CodeManager
{
    public abstract class ScriptObjVariableBase : ScriptableObject
    {
        [SerializeField]
        protected bool debug = false;
    }
}
