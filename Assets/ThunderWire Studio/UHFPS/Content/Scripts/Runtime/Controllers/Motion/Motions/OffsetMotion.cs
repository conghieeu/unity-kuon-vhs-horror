using System;
using UnityEngine;

namespace UHFPS.Runtime
{
    [Serializable]
    public class OffsetMotion : SpringMotionModule
    {
        public override string Name => "General/Offset Motion";

        [Serializable]
        public struct OffsetSettings
        {
            public Vector3 positionOffset;
            public Vector3 rotationOffset;
            public float duration;
        }

        [Header("General Settings")]
        public OffsetSettings enterOffset;
        public OffsetSettings exitOffset;

        private bool hasEntered;
        private float remainingResetDuration;

        public override void MotionUpdate(float deltaTime)
        {
            if (remainingResetDuration > 0f) 
                remainingResetDuration -= Time.deltaTime;

            // Check if the object is updatable and has just entered
            if (IsUpdatable)
            {
                if (!hasEntered) remainingResetDuration = enterOffset.duration;

                SetTargetPosition(enterOffset.positionOffset);
                SetTargetRotation(enterOffset.rotationOffset);

                hasEntered = true;
            }
            // Check if the object is not updatable and has just exited
            else if (hasEntered)
            {
                if (hasEntered) remainingResetDuration = exitOffset.duration;

                SetTargetPosition(exitOffset.positionOffset);
                SetTargetRotation(exitOffset.rotationOffset);

                hasEntered = false;
            }

            // Reset position and rotation once the reset duration is over
            if (remainingResetDuration <= 0f)
            {
                SetTargetPosition(Vector3.zero);
                SetTargetRotation(Vector3.zero);
            }
        }

        public override void Reset()
        {
            hasEntered = false;
            remainingResetDuration = 0f;
        }
    }
}
