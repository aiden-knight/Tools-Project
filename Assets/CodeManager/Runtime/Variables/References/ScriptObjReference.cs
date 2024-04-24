using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace AidenK.CodeManager
{
    [Serializable]
    public abstract class ScriptObjReference<T>
    {
        public bool UseConstant;
        public T ConstantValue;
        public ScriptObjVariable<T> Variable;

        public T Value
        {
            get { return UseConstant ? ConstantValue :  Variable.Value; }
        }
    }
}