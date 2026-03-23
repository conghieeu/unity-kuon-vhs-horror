using System;
using UnityEngine;

namespace UHFPS.Runtime
{
    [Serializable]
    public struct ExternalForceSettings
    {
        public Vector3 Force;
        public float Duration;
        public float Delay;

        public ExternalForceSettings(Vector3 force, float duration, float delay)
        {
            Force = force;
            Duration = Mathf.Max(0f, duration);
            Delay = Mathf.Max(0f, delay);
        }
    }
}