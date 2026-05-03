using System;
using UnityEngine;

namespace ThunderWire.Attributes
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public class DocsAttribute : PropertyAttribute
    {
        public string docsLink;

        public DocsAttribute(string link)
        {
            docsLink = link;
        }
    }
}