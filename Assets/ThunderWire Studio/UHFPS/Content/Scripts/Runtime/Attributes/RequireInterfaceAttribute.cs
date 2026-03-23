using System;
using UnityEngine;

namespace ThunderWire.Attributes
{
    [AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
    public class RequireInterfaceAttribute : PropertyAttribute
    {
        public Type InterfaceType;

        public RequireInterfaceAttribute(Type interfaceType)
        {
            InterfaceType = interfaceType;
        }
    }
}