using System;
using UnityEngine;

namespace ThunderWire.Attributes
{
    [AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
    public class PlayerStatePickerAttribute : PropertyAttribute
    {
        public bool includeDefault;

        public PlayerStatePickerAttribute(bool includeDefault = false)
        {
            this.includeDefault = includeDefault;
        }
    }
}