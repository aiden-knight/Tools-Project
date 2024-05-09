using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace AidenK.CodeManager
{
    public abstract class ScriptObjEventBase : ScriptableObject
    {
        public Action CallInvoke;
    }
}