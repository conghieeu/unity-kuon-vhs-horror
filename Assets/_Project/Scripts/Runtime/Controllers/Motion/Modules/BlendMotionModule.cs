using System;
using UnityEngine;

namespace UHFPS.Runtime
{
    public abstract class BlendMotionModule : MotionModule
    {
        public float BlendDamping = 1f;

        [NonSerialized] private Vector3 positionTarget;
        [NonSerialized] private Vector3 rotationTarget;

        [NonSerialized] private float motionWeight = 1f;
        [NonSerialized] private float targetWeight = 1f;
        [NonSerialized] private float velocity;

        public bool IsUpdating => motionWeight > 0f;

        public override Vector3 GetPosition(float deltaTime) => positionTarget * motionWeight;
        public override Quaternion GetRotation(float deltaTime) => Quaternion.Euler(rotationTarget * motionWeight);

        protected override void SetTargetPosition(Vector3 target)
        {
            target *= Weight;
            positionTarget = target;
        }

        protected override void SetTargetPosition(Vector3 target, float multiplier = 1)
        {
            target *= Weight * multiplier;
            positionTarget = target;
        }

        protected override void SetTargetRotation(Vector3 target)
        {
            target *= Weight;
            rotationTarget = target;
        }

        protected override void SetTargetRotation(Vector3 target, float multiplier = 1)
        {
            target *= Weight * multiplier;
            rotationTarget = target;
        }

        public void SetTargetWeight(float weight)
        {
            targetWeight = Mathf.Clamp01(weight);
        }

        public void SetMotionWeight(float weight)
        {
            weight = Mathf.Clamp01(weight);
            targetWeight = weight;
            motionWeight = weight;
        }

        public override void MotionUpdate(float deltaTime)
        {
            motionWeight = Mathf.SmoothDamp(motionWeight, targetWeight, ref velocity, deltaTime * BlendDamping * 10);
        }

        public override void ResetSpring()
        {
            positionTarget = Vector3.zero;
            rotationTarget = Vector3.zero;
            targetWeight = 1f;
            motionWeight = 1f;
        }
    }
}