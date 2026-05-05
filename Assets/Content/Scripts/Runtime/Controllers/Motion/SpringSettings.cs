using System;
using UnityEngine;
using ThunderWire.Attributes;

namespace UHFPS.Runtime
{
    [Serializable]
    [Summary("Cấu hình thông số vật lý của lò xo (Spring).")]
    public sealed class SpringSettings
    {
        [Range(0f, 100f)] [Tooltip("Độ giảm xóc (Damping), giá trị càng lớn thì dao động càng nhanh tắt.")] public float Damping;
        [Range(0f, 1000f)] [Tooltip("Độ cứng của lò xo (Stiffness).")] public float Stiffness;
        [Range(0f, 10f)] [Tooltip("Khối lượng ảnh hưởng đến lò xo (Mass).")] public float Mass;
        [Range(0f, 10f)] [Tooltip("Tốc độ vật lý của lò xo.")] public float Speed;

        public SpringSettings(float damping, float stiffness, float mass, float speed)
        {
            Damping = damping;
            Stiffness = stiffness;
            Mass = mass;
            Speed = speed;
        }

        public static SpringSettings Default => new(10f, 120f, 1f, 1f);
    }
}