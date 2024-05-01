using AidenK.CodeManager;
using UnityEditor;

namespace AidenK.CodeManager
{
    [CustomEditor(typeof(DoubleVariable))]
    public class DoubleEditor : VariableEditor<double> { }
}