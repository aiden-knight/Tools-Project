using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace AidenK.CodeManager
{
    [CustomEditor(typeof(ScriptObjListenerBase), true)]
    public class ListenerEditor: Editor
    {
        [SerializeField]
        VisualTreeAsset _listenerTreeAsset = null;
        public override VisualElement CreateInspectorGUI()
        {
            VisualElement root = new VisualElement();
            root.Add(new IMGUIContainer(OnInspectorGUI));

            VisualElement uxmlElement = _listenerTreeAsset.Instantiate();
            root.Add(uxmlElement);

            return root;
        }

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
        }
    }
}
