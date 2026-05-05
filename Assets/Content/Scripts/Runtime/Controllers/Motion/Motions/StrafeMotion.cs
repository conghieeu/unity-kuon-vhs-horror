using UnityEngine;
using ThunderWire.Attributes;

namespace UHFPS.Runtime
{
    [Summary("Hiệu ứng lắc lư khi nhân vật di chuyển ngang (Strafe).")]
    public class StrafeMotion : SpringMotionModule
    {
        public override string Name => "General/Strafe Motion";

        private const float PositionMod = 0.01f;

        [Header("General Settings")]
        [Tooltip("Giới hạn độ dài lắc lư tối đa.")]
        public float MaxSwayLength = 10f;

        [Header("Position Strafe")]
        [Tooltip("Độ thay đổi vị trí theo chuyển động ngang.")]
        public Vector3 PositionSway;

        [Header("Rotation Strafe")]
        [Tooltip("Độ thay đổi góc quay theo chuyển động ngang.")]
        public Vector3 RotationSway;

        public override void MotionUpdate(float deltaTime)
        {
            Vector3 movementMotion = player.Motion;
            if (!IsUpdatable || state == PlayerStateMachine.IDLE_STATE || movementMotion == Vector3.zero)
                return;

            movementMotion = transform.InverseTransformVector(movementMotion);
            movementMotion = Vector3.ClampMagnitude(movementMotion, MaxSwayLength);

            Vector3 posSway = new()
            {
                x = movementMotion.x * PositionSway.x * PositionMod,
                y = -Mathf.Abs(movementMotion.x * PositionSway.y) * PositionMod,
                z = -movementMotion.z * PositionSway.z * PositionMod
            };

            Vector3 rotSway = new()
            {
                x = movementMotion.z * RotationSway.x,
                y = -movementMotion.x * RotationSway.y,
                z = movementMotion.x * RotationSway.z
            };

            SetTargetPosition(posSway);
            SetTargetRotation(rotSway);
        }
    }
}