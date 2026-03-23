using System;
using UnityEngine;

namespace UHFPS.Runtime
{
    [Serializable]
    public sealed class ShakeForceMotion : ExternalMotionData
    {
        [Serializable]
        public struct ShakeForceSettings
        {
            public float XAmplitude;
            public float YAmplitude;
            public float ZAmplitude;

            [Header("Settings")]
            public float Duration;
            public float Speed;
        }

        public override string Name => "Shake Force";

        public ShakeForceSettings PositionShake;
        public ShakeForceSettings RotationShake;

        public override ExternalMotionModule GetPosition => new ShakeForce(PositionShake);
        public override ExternalMotionModule GetRotation => new ShakeForce(RotationShake);

        public sealed class ShakeForce : ExternalMotionModule
        {
            public override bool IsFinished => Time.time > endTime;

            private readonly float xAmplitude;
            private readonly float yAmplitude;
            private readonly float zAmplitude;
            private readonly float speed;
            private readonly float duration;
            private readonly float endTime;

            private static readonly AnimationCurve decayCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);

            public ShakeForce(ShakeForceSettings settings)
            {
                xAmplitude = (UnityEngine.Random.value > 0.5f ? 1f : -1f) * settings.XAmplitude;
                yAmplitude = (UnityEngine.Random.value > 0.5f ? 1f : -1f) * settings.YAmplitude;
                zAmplitude = (UnityEngine.Random.value > 0.5f ? 1f : -1f) * settings.ZAmplitude;

                duration = settings.Duration;
                speed = settings.Speed;
                endTime = Time.time + duration;
            }

            public override Vector3 Evaluate()
            {
                float remainingTime = endTime - Time.time;
                float timer = remainingTime * speed;
                float decay = decayCurve.Evaluate(1f - remainingTime / duration);

                return new Vector3(
                    Mathf.Sin(timer) * xAmplitude * decay,
                    Mathf.Cos(timer) * yAmplitude * decay,
                    Mathf.Sin(timer) * zAmplitude * decay
                );
            }
        }
    }
}