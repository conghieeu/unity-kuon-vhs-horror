using UnityEngine;
using ThunderWire.Attributes;

namespace UHFPS.Runtime
{
    /// <summary>
    /// Cấu trúc lưu trữ và tính toán tỉ lệ phần trăm (0-100).
    /// </summary>
    [System.Serializable]
    public struct Percentage
    {
        [Tooltip("Giá trị phần trăm (0-100).")]
        public ushort Value;

        public Percentage(ushort value)
        {
            Value = value;
        }

        public float Ratio() => (float)Value / 100;

        public float From(float value) => Ratio() * value;

        public static implicit operator ushort(Percentage percentage)
        {
            return percentage.Value;
        }

        public static implicit operator Percentage(ushort value)
        {
            return new Percentage(value);
        }
    }
}