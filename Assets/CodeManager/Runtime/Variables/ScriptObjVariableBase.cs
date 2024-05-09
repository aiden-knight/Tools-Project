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
        protected bool debug = false;
        [SerializeField]
        protected ResetOn resetOn = ResetOn.Play;
    }
}
