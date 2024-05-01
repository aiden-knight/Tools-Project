using AidenK.CodeManager;
using UnityEditor;

namespace AidenK.CodeManager
{
    [CustomEditor(typeof(LongVariable))]
    public class LongEditor : VariableEditor<long> { }
}