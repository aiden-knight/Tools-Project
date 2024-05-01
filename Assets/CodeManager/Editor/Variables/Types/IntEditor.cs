using AidenK.CodeManager;
using UnityEditor;

namespace AidenK.CodeManager
{
    [CustomEditor(typeof(IntVariable))]
    public class IntEditor : VariableEditor<int> { }
}