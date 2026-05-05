using UnityEngine;
using ThunderWire.Attributes;

namespace UHFPS.Runtime
{
    [Summary("Hiệu ứng lắc lư nhẹ theo hướng nhìn của chuột (Sway).")]
    public class LookMotion : SpringMotionModule
    {
        public override string Name => "General/Look Motion";

        private const float PositionMod = 0.02f;

        [Header("General Settings")]
        [Tooltip("Độ dài Sway (lắc lư) tối đa.")]
        public float MaxSwayLength = 10f;

        [Header("Position Sway")]
        [Tooltip("Biên độ thay đổi vị trí khi xoay chuột.")]
        public Vector3 PositionSway;
        [Tooltip("Hệ số nhân cho sự thay đổi vị trí.")]
        public float PositionMultiplier = 1f;

        [Header("Rotation Sway")]
        [Tooltip("Biên độ thay đổi góc quay khi xoay chuột.")]
        public Vector3 RotationSway;
        [Tooltip("Hệ số nhân cho sự thay đổi góc quay.")]
        public float RotationMultiplier = 1f;

        public override void MotionUpdate(float deltaTime)
        {
            if (!IsUpdatable)
                return;

            Vector2 lookDelta = look.DeltaInput;
            lookDelta = Vector2.ClampMagnitude(lookDelta, MaxSwayLength);

            Vector3 posSway = new(
                lookDelta.x * PositionSway.x * PositionMod * PositionMultiplier,
                lookDelta.y * PositionSway.y * PositionMod * PositionMultiplier);

            Vector3 rotSway = new(
                lookDelta.y * RotationSway.x * PositionMultiplier,
                lookDelta.x * RotationSway.y * PositionMultiplier,
                lookDelta.x * RotationSway.z * PositionMultiplier);

            SetTargetPosition(posSway);
            SetTargetRotation(rotSway);
        }
    }
}