using System;
using UnityEngine;

namespace UHFPS.Runtime
{
    public abstract class SimpleMotionModule : MotionModule
    {
        [NonSerialized] private Vector3 positionTarget;
        [NonSerialized] private Vector3 rotationTarget;

        public override abstract void MotionUpdate(float deltaTime);

        public override Vector3 GetPosition(float deltaTime) => positionTarget;
        public override Quaternion GetRotation(float deltaTime) => Quaternion.Euler(rotationTarget);

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

        public override void ResetSpring()
        {
            positionTarget = Vector3.zero;
            rotationTarget = Vector3.zero;
        }
    }
}