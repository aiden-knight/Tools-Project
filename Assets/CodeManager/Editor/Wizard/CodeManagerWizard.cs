using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace AidenK.CodeManager
{
    public class CodeManagerWizard : EditorWindow
    {
        [SerializeField]
        private VisualTreeAsset m_VisualTreeAsset = default;

        // Window creation and showing
        static CodeManagerWizard Instance;
        [MenuItem("Window/Code Manager/Wizard", priority = -5)]
        public static void ShowExample()
        {
            Instance = GetWindow<CodeManagerWizard>(false, "Code Manager", true);
        }

        VisualElement ScrollingContainerContent;
        public void CreateGUI()
        {
            // Each editor window contains a root VisualElement object
            VisualElement root = rootVisualElement;

            // Instantiate UXML
            VisualElement labelFromUXML = m_VisualTreeAsset.Instantiate();
            root.Add(labelFromUXML);

            var scrollV = root.Q<ScrollView>("ScriptableObjects");
            ScrollingContainerContent = scrollV.Q("unity-content-container");
            
            string[] guids = AssetDatabase.FindAssets("t:ScriptableObject", null);
            foreach (string guid in guids)
            {
                Button button = new Button();
                button.text = AssetDatabase.GUIDToAssetPath(guid);
                ScrollingContainerContent.Add(button);
            }
        }

        public void OnGUI()
        {
            var children = ScrollingContainerContent.Children().Where(elem => elem.GetType() == typeof(Button));
            Debug.Log(children.Count());
        }
    }
}