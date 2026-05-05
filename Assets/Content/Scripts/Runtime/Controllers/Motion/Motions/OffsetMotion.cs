using System;
using UnityEngine;
using ThunderWire.Attributes;

namespace UHFPS.Runtime
{
    [Serializable]
    [Summary("Cấu hình biên độ dịch chuyển (Offset) cố định được kích hoạt khi đi vào/ra khỏi một trạng thái.")]
    public class OffsetMotion : SpringMotionModule
    {
        public override string Name => "General/Offset Motion";

        [Serializable]
        public struct OffsetSettings
        {
            [Tooltip("Vị trí bù trừ (Offset).")]
            public Vector3 positionOffset;
            [Tooltip("Góc quay bù trừ (Offset).")]
            public Vector3 rotationOffset;
            [Tooltip("Thời gian áp dụng.")]
            public float duration;
        }

        [Header("General Settings")]
        [Tooltip("Cấu hình áp dụng khi vừa bước vào (Enter) hiệu ứng.")]
        public OffsetSettings enterOffset;
        [Tooltip("Cấu hình áp dụng khi kết thúc/thoát (Exit) hiệu ứng.")]
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
