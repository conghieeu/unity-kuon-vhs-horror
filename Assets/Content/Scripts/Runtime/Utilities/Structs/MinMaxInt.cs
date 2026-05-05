using System;
using UnityEngine;
using ThunderWire.Attributes;

namespace UHFPS.Runtime
{
    /// <summary>
    /// Cấu trúc lưu trữ giá trị tối thiểu và tối đa (kiểu int).
    /// </summary>
    [Serializable]
    public struct MinMaxInt
    {
        [Tooltip("Giá trị tối thiểu.")]
        public int min;
        [Tooltip("Giá trị tối đa.")]
        public int max;

        public bool Flipped => max < min;

        public int RealMin => Flipped ? max : min;
        public int RealMax => Flipped ? min : max;
        public Vector2Int RealVector => this;
        public Vector2Int Vector => new Vector2Int(min, max);

        public MinMaxInt(int min, int max)
        {
            this.min = min;
            this.max = max;
        }

        public static implicit operator Vector2Int(MinMaxInt minMax)
        {
            return new Vector2Int(minMax.RealMin, minMax.RealMax);
        }

        public static implicit operator MinMaxInt(Vector2Int vector)
        {
            MinMaxInt result = default;
            result.min = vector.x;
            result.max = vector.y;
            return result;
        }

        public MinMaxInt Flip() => new MinMaxInt(max, min);
    }
}