using System;
using UnityEngine;
using ThunderWire.Attributes;

namespace UHFPS.Runtime
{
    /// <summary>
    /// Tham chiếu đến một hành động Input cụ thể và chỉ số gán phím (Binding Index).
    /// </summary>
    [Serializable]
    public struct InputReference
    {
        [Tooltip("Tên hành động Input.")]
        public string ActionName;
        [Tooltip("Chỉ số gán phím.")]
        public int BindingIndex;
    }
}