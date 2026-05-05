using System;
using UnityEngine;
using ThunderWire.Attributes;

namespace UHFPS.Runtime
{
    [Serializable]
    [Summary("Trình đánh giá nội suy theo đường cong 3D (Curve3D) cho trục X, Y, Z.")]
    public sealed class Curve3D
    {
        [Tooltip("Đường cong cho trục X.")]
        public AnimationCurve CurveX = new(new Keyframe(0, 0), new Keyframe(1, 0));
        [Tooltip("Đường cong cho trục Y.")]
        public AnimationCurve CurveY = new(new Keyframe(0, 0), new Keyframe(1, 0));
        [Tooltip("Đường cong cho trục Z.")]
        public AnimationCurve CurveZ = new(new Keyframe(0, 0), new Keyframe(1, 0));
        [Range(-10f, 10f)] [Tooltip("Hệ số nhân áp dụng cho cả 3 đường cong.")] public float Multiplier = 1f;

        private float m_Duration;
        public float Duration
        {
            get
            {
                if (m_Duration > 0f)
                    return m_Duration;

                float durationX = CurveX[CurveX.length - 1].time;
                float durationY = CurveY[CurveY.length - 1].time;
                float durationZ = CurveZ[CurveZ.length - 1].time;
                return m_Duration = Mathf.Max(durationX, durationY, durationZ);
            }
        }

        public Vector3 Evaluate(float time)
        {
            return new Vector3()
            {
                x = CurveX.Evaluate(time) * Multiplier,
                y = CurveY.Evaluate(time) * Multiplier,
                z = CurveZ.Evaluate(time) * Multiplier
            };
        }
    }
}