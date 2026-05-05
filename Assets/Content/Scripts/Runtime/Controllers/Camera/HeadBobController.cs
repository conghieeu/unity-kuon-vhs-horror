using System;
using System.Collections;
using UnityEngine;
using UHFPS.Input;
using UHFPS.Tools;
using ThunderWire.Attributes;

namespace UHFPS.Runtime
{
    [InspectorHeader("HeadBob Controller")]
    [Summary("Điều khiển hiệu ứng rung lắc (HeadBob), nhịp thở (Breath) và nghiêng người (Lean) của Camera.")]
    public class HeadBobController : PlayerComponent
    {
        #region Structures
        [Serializable]
        public struct HeadBob
        {
            [Header("Vertical HeadBob")]
            [Tooltip("Tốc độ rung lắc theo chiều dọc.")]
            public float verticalBobSpeed;
            [Tooltip("Biên độ rung lắc theo chiều dọc.")]
            public float verticalBobAmount;
            [Tooltip("Biên độ nghiêng (Tilt) theo chiều dọc.")]
            public float verticalTiltAmount;

            [Header("Horizontal HeadBob")]
            [Tooltip("Tốc độ rung lắc theo chiều ngang.")]
            public float horizontalBobSpeed;
            [Tooltip("Biên độ rung lắc theo chiều ngang.")]
            public float horizontalBobAmount;
            [Tooltip("Biên độ nghiêng (Tilt) theo chiều ngang.")]
            public float horizontalTiltAmount;
        }

        public struct HeadBobWave
        {
            public float BobTime;
            public float Wave => Mathf.Sin(BobTime);

            public void Update(float multiplier = 1)
            {
                BobTime += Time.deltaTime * multiplier;
            }

            public void Reset() 
            {
                BobTime = 0;
            }
        }
        #endregion

        [Header("References")]
        [Tooltip("Transform điều khiển hiệu ứng HeadBob của Camera.")]
        public Transform CameraHeadBob;
        [Tooltip("Transform điều khiển hiệu ứng Lean (nghiêng người) của Camera.")]
        public Transform CameraLean;

        [Header("HeadBob States"), Space(1)]
        [Boxed] [Tooltip("Cấu hình HeadBob khi đang đi bộ.")] public HeadBob WalkingHeadBob = new();
        [Boxed] [Tooltip("Cấu hình HeadBob khi đang chạy.")] public HeadBob RunningHeadBob = new();
        [Boxed] [Tooltip("Cấu hình HeadBob khi đang ngồi xổm (Crouch).")] public HeadBob CrouchingHeadBob = new();
        [Boxed] [Tooltip("Cấu hình HeadBob khi đang ngắm bắn (Aim).")] public HeadBob AimingHeadBob = new();

        [Header("Breath Settings")]
        [Tooltip("Đường cong mô phỏng nhịp thở.")]
        public AnimationCurve BreathCurve = new(new(0, 1), new (1, 1));
        [Tooltip("Tốc độ nhịp thở.")]
        public float BreathSpeed;
        [Tooltip("Biên độ nhịp thở (độ nhấp nhô của Camera).")]
        public float BreathAmount;

        [Header("Jump Settings")]
        [Tooltip("Thời gian tối thiểu ở trên không để kích hoạt hiệu ứng khi chạm đất.")]
        public float MinAirTime;
        [Tooltip("Biên độ nảy (Kickback) cơ bản khi rơi xuống.")]
        public float FallKickbackAmount;
        [Tooltip("Biên độ nảy tối đa khi rơi xuống từ độ cao lớn.")]
        public float MaxFallKickbackAmount;
        [Tooltip("Biên độ nảy sang hai bên tối đa khi chạm đất.")]
        public float MaxSidewayKickbackAmount;
        [Tooltip("Ngưỡng tính toán độ nảy dựa trên khoảng cách rơi.")]
        public float FallKickbackTreshold;
        [Tooltip("Thời gian thực hiện hiệu ứng Kickback.")]
        public float KickbackTime;

        [Header("Lean Settings")]
        [Tooltip("LayerMask để kiểm tra va chạm khi nghiêng người (tránh xuyên tường).")]
        public LayerMask LeanMask;
        [Tooltip("Khoảng cách tối đa khi nghiêng người.")]
        public float LeanPosition;
        [Tooltip("Góc nghiêng tối đa của Camera.")]
        public float LeanTiltAmount;
        [Tooltip("Bán kính của SphereCast dùng để kiểm tra va chạm khi nghiêng.")]
        public float LeanColliderRadius;

        [Header("Speed Settings")]
        [Tooltip("Tốc độ chuyển đổi vị trí HeadBob.")]
        public float HeadBobSpeed;
        [Tooltip("Tốc độ chuyển đổi góc nghiêng (Tilt) HeadBob.")]
        public float HeadBobTiltSpeed;
        [Tooltip("Tốc độ di chuyển Camera khi nghiêng người.")]
        public float LeanSpeed;
        [Tooltip("Tốc độ nghiêng góc Camera khi Lean.")]
        public float LeanTiltSpeed;

        [Header("Blend Settings")]
        [Tooltip("Tốc độ pha trộn (Blend) giữa các hiệu ứng.")]
        public float BobBlendSpeed;
        [Tooltip("Vận tốc tối thiểu của người chơi để bắt đầu hiệu ứng HeadBob.")]
        public float BobStartVelocity;

        public Vector2 Wave => new Vector2(horizontalBob.Wave, verticalBob.Wave);
        public Vector3 BreathBobBlended { get; private set; }
        public float BreathBobBlend { get; private set; }
        public float Breath { get; private set; }

        // private
        private HeadBobWave verticalBob = new();
        private HeadBobWave horizontalBob = new();

        private float magnitude;
        private float breathTime;
        private float bobJumpBlend;
        private float jumpAirTime;
        private float lastYPos;

        private Vector3 defaultPos;
        private Vector3 defaultRot;

        private void Awake()
        {
            defaultPos = CameraHeadBob.localPosition;
            defaultRot = CameraHeadBob.localEulerAngles;
        }

        private void Update()
        {
            if (isEnabled)
            {
                UpdateHeadEffects();
            }
            else
            {
                CameraHeadBob.localPosition = Vector3.Lerp(CameraHeadBob.localPosition, defaultPos, Time.deltaTime * HeadBobSpeed);
                CameraHeadBob.localRotation = Quaternion.Slerp(CameraHeadBob.localRotation, Quaternion.Euler(defaultRot), Time.deltaTime * HeadBobTiltSpeed);
            }
        }

        private void UpdateHeadEffects()
        {
            // get camera effects
            var (headBob, headBobTilt) = EvaluateHeadBob();
            var (leanDir, leanPos) = EvaluateLean();
            Vector3 breath = EvaluateBreath();
            EvaluateJump();

            // calculate whether to play a breathing or head bobbing animation
            magnitude = PlayerCollider.velocity.magnitude > BobStartVelocity ? 1 : 0;
            BreathBobBlend = Mathf.MoveTowards(BreathBobBlend, magnitude, Time.deltaTime * BobBlendSpeed);
            BreathBobBlended = Vector3.Lerp(breath, headBob, BreathBobBlend);

            // calculate whether to play a jump or head bobbing/breath animation
            bool isGrounded = PlayerStateMachine.IsGrounded;
            bobJumpBlend = Mathf.MoveTowards(bobJumpBlend, isGrounded ? 0 : 1, Time.deltaTime * BobBlendSpeed);

            // select bob or jump effect
            Vector3 bobJumpPosBlended = Vector3.Lerp(BreathBobBlended, Vector3.zero, bobJumpBlend);
            Vector3 bobJumpRotBlended = Vector3.Lerp(headBobTilt, Vector3.zero, bobJumpBlend);

            // apply head bob position and tilt
            CameraHeadBob.localPosition = Vector3.Lerp(CameraHeadBob.localPosition, bobJumpPosBlended, Time.deltaTime * HeadBobSpeed);
            CameraHeadBob.localRotation = Quaternion.Slerp(CameraHeadBob.localRotation, Quaternion.Euler(bobJumpRotBlended), Time.deltaTime * HeadBobTiltSpeed);

            // calculate the lean tilt value
            float leanBlend = VectorE.InverseLerp(Vector3.zero, leanPos, CameraLean.localPosition);
            Vector3 leanTilt = -1 * leanDir * LeanTiltAmount * leanBlend * Vector3.forward;

            // calculate the head position offset value
            Vector3 leanDirection = transform.right * leanDir;
            Ray leanRay = new Ray(transform.position, leanDirection);

            // convert the max lean distance to a multiplier and multiply it with the leanPos value
            if (Physics.SphereCast(leanRay, LeanColliderRadius, out RaycastHit hit, LeanPosition, LeanMask))
                leanPos *= GameTools.Remap(0f, LeanPosition, 0f, 1f, hit.distance);

            // apply lean position and tilt
            CameraLean.localPosition = Vector3.Lerp(CameraLean.localPosition, leanPos, Time.deltaTime * LeanSpeed);
            CameraLean.localRotation = Quaternion.Slerp(CameraLean.localRotation, Quaternion.Euler(leanTilt), Time.deltaTime * LeanTiltSpeed);
        }

        private (Vector3 headBob, Vector3 headTilt) EvaluateHeadBob()
        {
            bool idle = PlayerStateMachine.IsCurrent(PlayerStateMachine.IDLE_STATE) || magnitude <= 0;
            bool running = PlayerStateMachine.IsCurrent(PlayerStateMachine.RUN_STATE);
            bool crouching = PlayerStateMachine.IsCurrent(PlayerStateMachine.CROUCH_STATE);

            HeadBob headBobState = WalkingHeadBob;
            if (running && !crouching) headBobState = RunningHeadBob;
            else if (crouching) headBobState = CrouchingHeadBob;

            if (!idle)
            {
                verticalBob.Update(headBobState.verticalBobSpeed);
                horizontalBob.Update(headBobState.horizontalBobSpeed);
            }
            else
            {
                verticalBob.Reset();
                horizontalBob.Reset();
            }

            Vector3 headBobPos = defaultPos;
            headBobPos.y += verticalBob.Wave * headBobState.verticalBobAmount;
            headBobPos.x += horizontalBob.Wave * headBobState.horizontalBobAmount;

            Vector3 headBobRot = defaultRot;
            headBobRot.x += verticalBob.Wave * headBobState.verticalTiltAmount;
            headBobRot.z += horizontalBob.Wave * headBobState.horizontalTiltAmount;

            return (headBobPos, headBobRot);
        }

        private Vector3 EvaluateBreath()
        {
            if (breathTime > BreathCurve[BreathCurve.length - 1].time)
                breathTime = 0f;

            breathTime += Time.deltaTime * BreathSpeed;
            float breathEval = BreathCurve.Evaluate(breathTime) * BreathAmount;
            Breath = breathEval;

            Vector3 breathPos = defaultPos;
            breathPos.y = breathEval;
            return breathPos;
        }

        private void EvaluateJump()
        {
            if (!PlayerStateMachine.IsGrounded)
            {
                jumpAirTime += Time.deltaTime;
            }
            else if (jumpAirTime > MinAirTime)
            {
                float currentYPos = transform.root.position.y;
                float additionalKickback = Mathf.Clamp(lastYPos - currentYPos, 0f, MaxFallKickbackAmount) * FallKickbackTreshold;
                float kickback = FallKickbackAmount + additionalKickback;
                StartCoroutine(DoHeadBobKickback(new Vector3(kickback, UnityEngine.Random.Range(-MaxSidewayKickbackAmount, MaxSidewayKickbackAmount), 0f), KickbackTime));
                jumpAirTime = 0f;
            }
            else
            {
                lastYPos = transform.root.position.y;
            }
        }

        private (float leanDir, Vector3 leanPos) EvaluateLean()
        {
            float leanDir = InputManager.ReadInput<float>(Controls.LEAN);
            Vector3 leanPos = new Vector3(leanDir * LeanPosition, 0f, 0f);
            return (leanDir, leanPos);
        }

        IEnumerator DoHeadBobKickback(Vector3 offset, float time)
        {
            Quaternion s = CameraHeadBob.localRotation;
            Quaternion e = CameraHeadBob.localRotation * Quaternion.Euler(offset);

            float r = 1.0f / time;
            float t = 0.0f;

            while (t < 1.0f)
            {
                t += Time.deltaTime * r;
                CameraHeadBob.localRotation = Quaternion.Slerp(s, e, t);
                yield return null;
            }
        }
    }
}