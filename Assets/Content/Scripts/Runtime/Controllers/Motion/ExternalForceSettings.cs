using System;
using UnityEngine;
using ThunderWire.Attributes;

namespace UHFPS.Runtime
{
    [Serializable]
    [Summary("Cấu hình lực tác động từ bên ngoài (External Force) lên camera/chuyển động.")]
    public struct ExternalForceSettings
    {
        [Tooltip("Hướng và độ lớn của lực tác động.")]
        public Vector3 Force;
        [Tooltip("Thời gian duy trì lực.")]
        public float Duration;
        [Tooltip("Độ trễ trước khi áp dụng lực.")]
        public float Delay;

        public ExternalForceSettings(Vector3 force, float duration, float delay)
        {
            Force = force;
            Duration = Mathf.Max(0f, duration);
            Delay = Mathf.Max(0f, delay);
        }
    }
}