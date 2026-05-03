using System;
using UnityEngine;

namespace UHFPS.Runtime
{
    [Serializable]
    public struct MinMax
    {
        public float min;
        public float max;

        public bool Flipped => max < min;

        public bool HasValue => min != 0 || max != 0;

        public float RealMin => Mathf.Min(min, max);
        public float RealMax => Mathf.Max(max, min);
        public Vector2 RealVector => this;
        public Vector2 Vector => new(min, max);

        public MinMax(float min, float max)
        {
            this.min = min;
            this.max = max;
        }

        public static implicit operator Vector2(MinMax minMax)
        {
            return new Vector2(minMax.RealMin, minMax.RealMax);
        }

        public static implicit operator MinMax(Vector2 vector)
        {
            MinMax result = default;
            result.min = vector.x;
            result.max = vector.y;
            return result;
        }

        public MinMax Flip() => new MinMax(max, min);

        public override string ToString() => $"({RealMin}, {RealMax})";
    }
}