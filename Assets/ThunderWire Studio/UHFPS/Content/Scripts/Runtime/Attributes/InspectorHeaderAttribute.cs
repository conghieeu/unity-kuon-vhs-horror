using System;
using UnityEngine;

namespace ThunderWire.Attributes
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public class InspectorHeaderAttribute : PropertyAttribute
    {
        public string title;
        public string icon;
        public bool space;

        public InspectorHeaderAttribute(string title, string icon = "", bool space = true)
        {
            this.title = title;
            this.icon = icon;
            this.space = space;
        }
    }
}