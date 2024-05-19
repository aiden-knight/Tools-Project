using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace AidenK.CodeManager
{
    public enum ClassType
    {
        Variable,
        VariableAndReference,
        EventAndListener,
        Collection
    }

    public static class ClassGenerator
    {
        //private static string[] s_types = { "bool", "char", "double", "float", "int", "long", "string", "Vector2", "Vector3" };
        private static string s_directory = Application.dataPath + "/AidenK.CodeManager/Generated/";

        public static void Generate(ClassType classType, string type)
        {
            switch (classType)
            {
                case ClassType.Variable:
                    GenerateVariable(type);
                    break;
                case ClassType.VariableAndReference:
                    GenerateVariable(type);
                    GenerateReference(type);
                    break;
                case ClassType.EventAndListener:
                    GenerateEventAndListener(type);
                    break;
                case ClassType.Collection:
                    GenerateCollection(type);
                    break;
                default:
                    return;
            }


            AssetDatabase.Refresh();
        }

        private static string NameFixer(string type)
        {
            // iterate backwards over type string
            for( int i = type.Length - 1; i >= 0; i-- ) 
            {
                // if it is not a letter or digit it's invalid as a name of a class
                if (!char.IsLetterOrDigit(type, i))
                {
                    // if at end of string just remove
                    if(i + 1 == type.Length)
                    {
                        type = type.Remove(i, 1);
                    }
                    else // if not at end of string make name slightly nicer
                    {
                        char upper = char.ToUpper(type[i+1]);
                        type = type.Remove(i, 2).Insert(i, upper.ToString());
                    }
                }
            }

            // make first character upper case as convention
            char firstChar = char.ToUpper(type[0]);
            type = type.Remove(0, 1).Insert(0, firstChar.ToString());

            return type;
        }

        private static void GenerateVariable(string type)
        {
            Directory.CreateDirectory(s_directory + "/VariablesAndReferences/");

            string outFile = "TNameVariable.cs";
            string classTxt = "using UnityEngine;\r\n\r\nnamespace AidenK.CodeManager\r\n{\r\n    [CreateAssetMenu(menuName = \"Code Manager/Generated/Variables/TName\", order = 0)]\r\n    public class TNameVariable : ScriptObjVariable<Type> { }\r\n}";

            string typeAsName = NameFixer(type);

            outFile = s_directory + "/VariablesAndReferences/" + outFile.Replace("TName", typeAsName);
            classTxt = classTxt.Replace("TName", typeAsName);
            classTxt = classTxt.Replace("Type", type);
            using (StreamWriter writer = new StreamWriter(outFile))
            {
                writer.Write(classTxt);
            }
        }

        private static void GenerateReference(string type)
        {
            Directory.CreateDirectory(s_directory + "/VariablesAndReferences/");

            string outFile = "TNameReference.cs";
            string classTxt = "using System;\r\n\r\nnamespace AidenK.CodeManager\r\n{\r\n    [Serializable]\r\n    public class TNameReference : ScriptObjReference<Type> { }\r\n}";

            string typeAsName = NameFixer(type);

            outFile = s_directory + "/VariablesAndReferences/" + outFile.Replace("TName", typeAsName);
            classTxt = classTxt.Replace("TName", typeAsName);
            classTxt = classTxt.Replace("Type", type);
            using (StreamWriter writer = new StreamWriter(outFile))
            {
                writer.Write(classTxt);
            }
        }

        private static void GenerateEventAndListener(string type)
        {
            Directory.CreateDirectory(s_directory + "/EventsAndListeners/");

            string eventOutFile = "TNameEvent.cs";
            string eventClass = "using UnityEngine;\r\n\r\nnamespace AidenK.CodeManager\r\n{\r\n    [CreateAssetMenu(menuName = \"Code Manager/Generated/Events/TName Event\", order = 0)]\r\n    public class TNameEvent : ScriptObjEventOneParam<Type>\r\n    {\r\n\r\n    }\r\n}";

            string listenerOutFile = "TNameListener.cs";
            string listenerClass = "namespace AidenK.CodeManager\r\n{\r\n    public class TNameListener : ScriptObjListenerOneParam<Type> {}\r\n}";

            string typeAsName = NameFixer(type);

            eventOutFile = s_directory + "/EventsAndListeners/" + eventOutFile.Replace("TName", typeAsName);
            eventClass = eventClass.Replace("TName", typeAsName);
            eventClass = eventClass.Replace("Type", type);
            using (StreamWriter writer = new StreamWriter(eventOutFile))
            {
                writer.Write(eventClass);
            }

            listenerOutFile = s_directory + "/EventsAndListeners/" + listenerOutFile.Replace("TName", typeAsName);
            listenerClass = listenerClass.Replace("TName", typeAsName);
            listenerClass = listenerClass.Replace("Type", type);
            using (StreamWriter writer = new StreamWriter(listenerOutFile))
            {
                writer.Write(listenerClass);
            }
        }

        private static void GenerateCollection(string type)
        {
            Directory.CreateDirectory(s_directory + "/Collections/");

            string outFile = "TNameCollection.cs";
            string classTxt = "using UnityEngine;\r\n\r\nnamespace AidenK.CodeManager\r\n{\r\n    [CreateAssetMenu(menuName = \"Code Manager/Generated/Collections/TName\", order = 0)]\r\n    public class TNameCollection : ScriptObjCollection<Type> { }\r\n}";

            string typeAsName = NameFixer(type);

            outFile = s_directory + "/Collections/" + outFile.Replace("TName", typeAsName);
            classTxt = classTxt.Replace("TName", typeAsName);
            classTxt = classTxt.Replace("Type", type);
            using (StreamWriter writer = new StreamWriter(outFile))
            {
                writer.Write(classTxt);
            }
        }
    }
}