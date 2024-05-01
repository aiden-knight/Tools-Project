using AidenK.CodeManager;
using UnityEditor;
using UnityEngine;

namespace AidenK.CodeManager
{
    [CustomEditor(typeof(Vector3Variable))]
    public class Vector3Editor : VariableEditor<Vector3> { }
}