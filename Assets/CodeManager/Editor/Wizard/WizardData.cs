using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AidenK.CodeManager
{
    public class WizardData : ScriptableObject
    {
        public int DropdownIndex;
        public string ClassType;

        public string SelectedAssetGUID;
        public WizardData() 
        {
            DropdownIndex = 1;
            ClassType = "Enter Class Here";
            SelectedAssetGUID = string.Empty;
        }
    }
}