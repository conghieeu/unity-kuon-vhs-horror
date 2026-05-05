using System;
using UnityEngine;
using ThunderWire.Attributes;

namespace UHFPS.Runtime
{
    [Serializable]
    [Summary("Hiệu ứng nhấp nhô (Bobbing) khi di chuyển, áp dụng cho vị trí và góc quay.")]
    public class BobMotion : SpringMotionModule
    {
        public override string Name => "General/Bob Motion";

        [Header("General Settings")]
        [Tooltip("Tốc độ nhấp nhô.")]
        public float bobbingSpeed = 1f;
        [Tooltip("Tốc độ phục hồi về vị trí ban đầu khi dừng lại.")]
        public float resetSpeed = 10f;
        [Tooltip("Tốc độ tối đa được xem là dừng lại.")]
        public float playerStopSpeed = 0.5f;

        [Header("Amplitude Settings")]
        [Tooltip("Biên độ nhấp nhô của vị trí.")]
        public Vector3 positionAmplitude = Vector3.zero;
        [Tooltip("Biên độ nhấp nhô của góc quay.")]
        public Vector3 rotationAmplitude = Vector3.zero;

        private float currentBobTime;
        private Vector3 currentPositionBob;
        private Vector3 currentRotationBob;

        public override void MotionUpdate(float deltaTime)
        {
            float playerSpeed = component.PlayerCollider.velocity.magnitude;
            bool isIdle = playerSpeed <= playerStopSpeed || !IsUpdatable;

            if (!isIdle)
            {
                currentBobTime = Time.time * bobbingSpeed;
                float bobY = Mathf.Cos(currentBobTime * 2);

                Vector3 posAmplitude = positionAmplitude;
                currentPositionBob = new Vector3
                {
                    x = Mathf.Cos(currentBobTime) * posAmplitude.x,
                    y = bobY * posAmplitude.y,
                    z = Mathf.Cos(currentBobTime) * posAmplitude.z
                };

                Vector3 rotAmplitude = rotationAmplitude;
                currentRotationBob = new Vector3
                {
                    x = Mathf.Cos(currentBobTime * 2) * rotAmplitude.x,
                    y = Mathf.Cos(currentBobTime) * rotAmplitude.y,
                    z = Mathf.Cos(currentBobTime) * rotAmplitude.z
                };

                Parameters["waveY"] = bobY;
            }
            else
            {
                float resetBobSpeed = deltaTime * resetSpeed * 10f;
                currentBobTime = Mathf.MoveTowards(currentBobTime, 0f, resetBobSpeed);

                if (Mathf.Abs(currentPositionBob.x + currentPositionBob.y + currentPositionBob.y) > 0.001f)
                    currentPositionBob = Vector3.MoveTowards(currentPositionBob, Vector3.zero, resetBobSpeed);
                else
                    currentPositionBob = Vector3.zero;

                if (Mathf.Abs(currentRotationBob.x + currentRotationBob.y + currentRotationBob.y) > 0.001f)
                    currentRotationBob = Vector3.MoveTowards(currentRotationBob, Vector3.zero, resetBobSpeed);
                else
                    currentRotationBob = Vector3.zero;
            }

            SetTargetPosition(currentPositionBob);
            SetTargetRotation(currentRotationBob);
        }

        public override void Reset()
        {
            currentBobTime = 0f;
            currentPositionBob = Vector3.zero;
            currentRotationBob = Vector3.zero;
        }
    }
}
