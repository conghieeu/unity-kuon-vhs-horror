using System;
using UnityEngine;

namespace UHFPS.Runtime
{
    [Serializable]
    public sealed class SimpleForceMotion : ExternalMotionData
    {
        [Serializable]
        public struct SimpleForceSettings
        {
            public Vector3 Force;
            public float Duration;
            public float Delay;
        }

        public override string Name => "Simple Force";

        public SimpleForceSettings PositionForce;
        public SimpleForceSettings RotationForce;

        public override ExternalMotionModule GetPosition => new SimpleForce(PositionForce);
        public override ExternalMotionModule GetRotation => new SimpleForce(RotationForce);

        public sealed class SimpleForce : ExternalMotionModule
        {
            public override bool IsFinished => Time.time > endTime;

            private readonly Vector3 force;
            private readonly float endTime;
            private readonly float delay;

            public SimpleForce(SimpleForceSettings settings)
            {
                force = settings.Force;

                float _duration = Mathf.Max(0f, settings.Duration);
                float _delay = Mathf.Max(0f, settings.Delay);

                endTime = Time.time + _duration + _delay;
                delay = Time.time + _delay;
            }

            public override Vector3 Evaluate()
            {
                if (Time.time > delay && Time.time < endTime)
                    return force;

                return Vector3.zero;
            }
        }
    }
}