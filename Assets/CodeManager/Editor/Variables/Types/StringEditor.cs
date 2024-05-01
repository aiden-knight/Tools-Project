using AidenK.CodeManager;
using UnityEditor;

namespace AidenK.CodeManager
{
    [CustomEditor(typeof(StringVariable))]
    public class StringEditor : VariableEditor<string> { }
}