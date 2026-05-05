using UnityEngine;
using UHFPS.Scriptable;
using UHFPS.Tools;

namespace UHFPS.Runtime.States
{
    [Summary("Cấu hình trạng thái khi người chơi chết (Death).")]
    public class DeathStateAsset : PlayerStateAsset
    {
        [Tooltip("Vị trí của Camera khi nhân vật ngã gục.")]
        public Vector3 DeathCameraPosition;
        [Tooltip("Góc quay của Camera khi nhân vật ngã gục.")]
        public Vector3 DeathCameraRotation;
        [Tooltip("Ngưỡng bắt đầu xoay góc nhìn (0 - 1) trong quá trình ngã.")]
        public float RotationChangeStart = 0.7f;
        [Tooltip("Thời gian chuyển đổi góc nhìn khi chết.")]
        public float DeathChangeTime = 0.3f;

        public override FSMPlayerState InitState(PlayerStateMachine machine, PlayerStatesGroup group)
        {
            return new DeathPlayerState(machine, this);
        }

        public override string StateKey => PlayerStateMachine.DEATH_STATE;

        public override string Name => "Generic/Death";

        public class DeathPlayerState : FSMPlayerState
        {
            protected readonly DeathStateAsset State;

            public override bool CanTransitionWhenDisabled => true;

            private float t;
            private float velocity;
            private bool isGrounded;

            private Vector3 positionStart;
            private Vector3 rotationStart;

            public DeathPlayerState(PlayerStateMachine machine, PlayerStateAsset stateAsset) : base(machine) 
            {
                State = (DeathStateAsset)stateAsset;
            }

            public override void OnStateEnter()
            {
                positionStart = machine.PlayerManager.CameraHolder.localPosition;
                rotationStart = machine.PlayerManager.CameraHolder.localEulerAngles;
                cameraLook.enabled = false;
            }

            public override void OnStateUpdate()
            {
                if (IsGrounded || isGrounded)
                {
                    t = Mathf.SmoothDamp(t, 1f, ref velocity, State.DeathChangeTime);
                    float rotationBlend = GameTools.Remap(State.RotationChangeStart, 1f, 0f, 1f, t);

                    Vector3 localPos = Vector3.Lerp(positionStart, State.DeathCameraPosition, t);
                    Vector3 localRot = Vector3.Lerp(rotationStart, rotationStart + State.DeathCameraRotation, rotationBlend);

                    machine.PlayerManager.CameraHolder.localPosition = localPos;
                    machine.PlayerManager.CameraHolder.localEulerAngles = localRot;

                    if (!isGrounded)
                    {
                        machine.PlayerCollider.enabled = false;
                        machine.Motion = Vector3.zero;
                        isGrounded = true;
                    }
                }
                else
                {
                    ApplyGravity(ref machine.Motion);
                }
            }
        }
    }
}