using System;
using UnityEngine;
using ThunderWire.Attributes;

namespace UHFPS.Runtime
{
    [Serializable]
    [Summary("Module chuyển động sử dụng hệ thống vật lý lò xo (Spring3D) để tạo độ nảy và quán tính.")]
    public abstract class SpringMotionModule : MotionModule
    {
        private Spring3D PositionSpring = new();
        private Spring3D RotationSpring = new();

        [Tooltip("Cấu hình độ nhạy/quán tính lò xo cho vị trí.")]
        public SpringSettings PositionSpringSettings = new(10f, 100f, 1f, 1f);
        [Tooltip("Cấu hình độ nhạy/quán tính lò xo cho góc quay.")]
        public SpringSettings RotationSpringSettings = new(10f, 100f, 1f, 1f);

        public override void Initialize(MotionSettings motionSettings)
        {
            base.Initialize(motionSettings);
            PositionSpring = new(PositionSpringSettings);
            RotationSpring = new(RotationSpringSettings);
        }

        public override abstract void MotionUpdate(float deltaTime);

        public override Vector3 GetPosition(float deltaTime) => PositionSpring.Evaluate(deltaTime);
        public override Quaternion GetRotation(float deltaTime) => Quaternion.Euler(RotationSpring.Evaluate(deltaTime));

        protected override void SetTargetPosition(Vector3 target)
        {
            target *= Weight;
            PositionSpring.SetTarget(target);
        }

        protected override void SetTargetPosition(Vector3 target, float multiplier = 1)
        {
            target *= Weight * multiplier;
            PositionSpring.SetTarget(target);
        }

        protected override void SetTargetRotation(Vector3 target)
        {
            target *= Weight;
            RotationSpring.SetTarget(target);
        }

        protected override void SetTargetRotation(Vector3 target, float multiplier = 1)
        {
            target *= Weight * multiplier;
            RotationSpring.SetTarget(target);
        }

        public override void ResetSpring()
        {
            PositionSpring.Reset();
            RotationSpring.Reset();
        }
    }
}