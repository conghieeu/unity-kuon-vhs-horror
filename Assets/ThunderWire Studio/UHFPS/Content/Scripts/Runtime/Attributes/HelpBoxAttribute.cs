using System;
using UnityEngine;

namespace ThunderWire.Attributes
{
    public enum HelpType { None, Info, Warning, Error }

    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public class HelpBoxAttribute : PropertyAttribute
    {
        public string message;
        public HelpType messageType;

        public HelpBoxAttribute(string message, HelpType type = HelpType.Info)
        {
            this.message = message;
            messageType = type;
        }
    }
}