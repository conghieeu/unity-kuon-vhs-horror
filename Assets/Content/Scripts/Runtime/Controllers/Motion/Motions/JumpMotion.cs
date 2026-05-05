using System;
using UnityEngine;
using ThunderWire.Attributes;

namespace UHFPS.Runtime
{
    [Serializable]
    [Summary("Hiệu ứng chuyển động nảy lên khi nhân vật nhảy.")]
    public class JumpMotion : SpringMotionModule
    {
        public override string Name => "General/Jump Motion";

        [Header("General Settings")]
        [Tooltip("Cấu hình biên độ dịch chuyển (Offset) khi nhảy.")]
        public OffsetMotion.OffsetSettings jumpSettings;

        private float remainingResetDuration;
        private bool airborne;
        private bool jumped;

        public override void OnStateChange(string state)
        {
            jumped = state == PlayerStateMachine.JUMP_STATE;
        }

        public override void MotionUpdate(float deltaTime)
        {
            if (!IsUpdatable)
            {
                SetTargetPosition(Vector3.zero);
                SetTargetRotation(Vector3.zero);
                return;
            }

            if (remainingResetDuration > 0f)
                remainingResetDuration -= Time.deltaTime;

            if(jumped && !player.StateGrounded && !airborne)
            {
                remainingResetDuration = jumpSettings.duration;
                SetTargetPosition(jumpSettings.positionOffset);
                SetTargetRotation(jumpSettings.rotationOffset);
                airborne = true;
                jumped = false;
            }
            else if(player.StateGrounded)
            {
                airborne = false;
            }

            if (remainingResetDuration <= 0f)
            {
                SetTargetPosition(Vector3.zero);
                SetTargetRotation(Vector3.zero);
                remainingResetDuration = 0f;
            }
        }

        public override void Reset()
        {
            remainingResetDuration = 0f;
            airborne = false;
            jumped = false;
        }
    }
}