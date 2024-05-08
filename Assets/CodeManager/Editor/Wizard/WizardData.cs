using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AidenK.CodeManager
{
    public class WizardData : ScriptableObject
    {
        public int dropdownIndex;
        public string classType;

        public WizardData() 
        {
            dropdownIndex = 1;
            classType = "Enter Class Here";
        }
    }
}