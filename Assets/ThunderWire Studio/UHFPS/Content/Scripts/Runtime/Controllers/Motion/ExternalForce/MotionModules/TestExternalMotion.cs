using System;
using UnityEngine;

namespace UHFPS.Runtime
{
    [Serializable]
    public class TestExternalMotion : ExternalMotionData
    {
        [Serializable]
        public struct TestSettings
        {
            public Vector3 Force;
            public float Duration;
        }

        public TestSettings PositionSettings;
        public TestSettings RotationSettings;

        public override string Name => "Test Motion";

        public override ExternalMotionModule GetPosition => new TestForce(PositionSettings);

        public override ExternalMotionModule GetRotation => new TestForce(RotationSettings);

        public sealed class TestForce : ExternalMotionModule
        {
            public override bool IsFinished => Time.time > endTime;

            private readonly Vector3 force;
            private readonly float endTime;

            public TestForce(TestSettings settings)
            {
                force = settings.Force;
                endTime = Time.time + settings.Duration;
            }

            public override Vector3 Evaluate()
            {
                return force;
            }
        }
    }
}