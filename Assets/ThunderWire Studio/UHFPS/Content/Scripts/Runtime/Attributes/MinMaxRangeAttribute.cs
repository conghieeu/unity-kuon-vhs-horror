using System;
using UnityEngine;

namespace ThunderWire.Attributes
{
    /// <summary>
    /// Attribute which shows MinMax Range slider on Vector2.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public class MinMaxRangeAttribute : PropertyAttribute
    {
        public float MinValue;
        public float MaxValue;

        public MinMaxRangeAttribute(float min, float max)
        {
            MinValue = min;
            MaxValue = max;
        }
    }
}