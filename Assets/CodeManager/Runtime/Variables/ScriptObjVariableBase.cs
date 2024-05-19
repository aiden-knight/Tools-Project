using UnityEngine;
using UnityEngine.Events;

namespace AidenK.CodeManager
{
    public enum ResetOn
    {
        SceneLoad,
        Play
    }

    public abstract class ScriptObjVariableBase : ScriptableObject
    {
        [SerializeField]
        protected bool _debug = false;
        [SerializeField, Tooltip("When to reset the variable back to default value")]
        protected ResetOn _resetOn = ResetOn.Play;
    }
}
