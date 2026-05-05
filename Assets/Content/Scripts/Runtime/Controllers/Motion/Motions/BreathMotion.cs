using System;
using UnityEngine;
using ThunderWire.Attributes;

namespace UHFPS.Runtime
{
    [Serializable]
    [Summary("Hiệu ứng nhịp thở (Breathing), làm chuyển động nhẹ nhàng theo chu kỳ.")]
    public class BreathMotion : SimpleMotionModule
    {
        public override string Name => "General/Breath Motion";

        [Header("General Settings")]
        [Tooltip("Đường cong Animation xác định mẫu nhịp thở.")]
        public AnimationCurve breathingPattern = new(new(0, 1), new(1, 1));
        [Tooltip("Tốc độ nhịp thở.")]
        public float breathingRate;
        [Tooltip("Cường độ nhịp thở.")]
        public float breathingIntensity;

        // Current time in the breathing cycle
        private float breathingCycleTime;

        public override void MotionUpdate(float deltaTime)
        {
            // If not updatable, reset to initial conditions
            if (!IsUpdatable)
            {
                SetTargetPosition(Vector3.zero);
                breathingCycleTime = 0f;
                return;
            }

            // Check if we've completed the breathing cycle, if so, reset the cycle
            if (breathingCycleTime > breathingPattern[breathingPattern.length - 1].time)
                breathingCycleTime = 0f;

            // Advance the breathing cycle
            breathingCycleTime += Time.deltaTime * breathingRate;
            float breathingValue = breathingPattern.Evaluate(breathingCycleTime) * breathingIntensity;

            // Create the breathing motion vector
            Vector3 breathingMotion = new(0, breathingValue, 0);
            SetTargetPosition(breathingMotion);
        }

        public override void Reset()
        {
            breathingCycleTime = 0f;
        }
    }
}