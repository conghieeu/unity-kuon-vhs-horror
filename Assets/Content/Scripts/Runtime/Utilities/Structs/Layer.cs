using System;
using UnityEngine;
using ThunderWire.Attributes;

namespace UHFPS.Runtime
{
    /// <summary>
    /// Wrapper cho Layer của Unity, hỗ trợ các phép toán so sánh và chuyển đổi.
    /// </summary>
    [Serializable]
    public struct Layer
    {
        [Tooltip("Chỉ số Layer (0-31).")]
        public int index;

        public static implicit operator int(Layer layer)
        {
            return layer.index;
        }

        public static implicit operator Layer(int intVal)
        {
            Layer result = default;
            result.index = intVal;
            return result;
        }

        public bool CompareLayer(GameObject obj)
        {
            return obj.layer == this;
        }
    }
}