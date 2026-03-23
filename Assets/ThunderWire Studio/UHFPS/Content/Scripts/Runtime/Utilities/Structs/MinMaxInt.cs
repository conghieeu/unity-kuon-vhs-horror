using System;
using UnityEngine;

namespace UHFPS.Runtime
{
    [Serializable]
    public struct MinMaxInt
    {
        public int min;
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