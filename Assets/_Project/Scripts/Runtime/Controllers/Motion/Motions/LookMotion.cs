using UnityEngine;

namespace UHFPS.Runtime
{
    public class LookMotion : SpringMotionModule
    {
        public override string Name => "General/Look Motion";

        private const float PositionMod = 0.02f;

        [Header("General Settings")]
        public float MaxSwayLength = 10f;

        [Header("Position Sway")]
        public Vector3 PositionSway;
        public float PositionMultiplier = 1f;

        [Header("Rotation Sway")]
        public Vector3 RotationSway;
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