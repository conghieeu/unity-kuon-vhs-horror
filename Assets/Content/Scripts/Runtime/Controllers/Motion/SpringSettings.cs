using System;
using UnityEngine;

namespace UHFPS.Runtime
{
    [Serializable]
    public sealed class SpringSettings
    {
        [Range(0f, 100f)] public float Damping;
        [Range(0f, 1000f)] public float Stiffness;
        [Range(0f, 10f)] public float Mass;
        [Range(0f, 10f)] public float Speed;

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