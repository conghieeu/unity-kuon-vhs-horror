using System;
using UnityEngine;

namespace ThunderWire.Attributes
{
    [AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
    public class BoxedAttribute : PropertyAttribute
    {
        public string icon;
        public string title;
        public bool rounded;
        public float headerHeight;
        public bool resourcesIcon;

        public BoxedAttribute(string icon = "", string title = "", float headerHeight = 22, bool rounded = true, bool resourcesIcon = false)
        {
            this.icon = icon;
            this.title = title;
            this.rounded = rounded;
            this.headerHeight = headerHeight;
            this.resourcesIcon = resourcesIcon;
        }
    }
}