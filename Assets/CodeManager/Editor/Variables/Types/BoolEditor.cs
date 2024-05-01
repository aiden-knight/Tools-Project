using AidenK.CodeManager;
using UnityEditor;

namespace AidenK.CodeManager
{
    [CustomEditor(typeof(BoolVariable))]
    public class BoolEditor : VariableEditor<bool> { }
}