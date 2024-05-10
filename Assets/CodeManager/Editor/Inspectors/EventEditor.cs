using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace AidenK.CodeManager
{
    [CustomEditor(typeof(ScriptObjEventBase), true)]
    public class EventEditor: ScriptObjEditor
    {
        void TriggerInvoke(ClickEvent evt)
        {
            ScriptObjEventBase objAsBase = serializedObject.targetObject as ScriptObjEventBase;
            objAsBase.CallInvoke.Invoke();
        }

        protected override VisualElement ExtraContent()
        {
            Button button = new Button();
            button.text = "Fire Event (Play Mode Only)";
            button.RegisterCallback<ClickEvent>(TriggerInvoke);
            return button;
        }
    }
}
 