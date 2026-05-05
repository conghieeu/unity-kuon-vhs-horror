using System;
using UnityEngine;
using ThunderWire.Attributes;

namespace UHFPS.Runtime
{
    /// <summary>
    /// Cấu trúc lưu trữ một cặp giá trị bao gồm Tên (GString) và Giá trị (Struct).
    /// </summary>
    [Serializable]
    public struct NameValue<TValue> where TValue : struct
    {
        [Tooltip("Tên của giá trị.")]
        public GString Name;
        [Tooltip("Giá trị.")]
        public TValue Value;

        public NameValue(GString name, TValue value)
        {
            Name = new(name, "");
            Value = value;
        }
    }
}