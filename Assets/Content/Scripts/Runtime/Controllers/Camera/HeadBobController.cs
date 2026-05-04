using System;
using System.Collections;
using UnityEngine;
using UHFPS.Input;
using UHFPS.Tools;
using ThunderWire.Attributes;

namespace UHFPS.Runtime
{
    [InspectorHeader("HeadBob Controller")]
    public class HeadBobController : PlayerComponent
    {
        #region Structures
        [Serializable]
        public struct HeadBob
        {
            [Header("Vertical HeadBob")]
            public float verticalBobSpeed;
            public float verticalBobAmount;
            public float verticalTiltAmount;

            [Header("Horizontal HeadBob")]
            public float horizontalBobSpeed;
            public float horizontalBobAmount;
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
        public Transform CameraHeadBob;
        public Transform CameraLean;

        [Header("HeadBob States"), Space(1)]
        [Boxed] public HeadBob WalkingHeadBob = new();
        [Boxed] public HeadBob RunningHeadBob = new();
        [Boxed] public HeadBob CrouchingHeadBob = new();
        [Boxed] public HeadBob AimingHeadBob = new();

        [Header("Breath Settings")]
        public AnimationCurve BreathCurve = new(new(0, 1), new (1, 1));
        public float BreathSpeed;
        public float BreathAmount;

        [Header("Jump Settings")]
        public float MinAirTime;
        public float FallKickbackAmount;
        public float MaxFallKickbackAmount;
        public float MaxSidewayKickbackAmount;
        public float FallKickbackTreshold;
        public float KickbackTime;

        [Header("Lean Settings")]
        public LayerMask LeanMask;
        public float LeanPosition;
        public float LeanTiltAmount;
        public float LeanColliderRadius;

        [Header("Speed Settings")]
        public float HeadBobSpeed;
        public float HeadBobTiltSpeed;
        public float LeanSpeed;
        public float LeanTiltSpeed;

        [Header("Blend Settings")]
        public float BobBlendSpeed;
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