using UnityEngine;
using UHFPS.Input;
using UHFPS.Scriptable;

namespace UHFPS.Runtime.States
{
    [Summary("Cấu hình trạng thái trượt dốc (Sliding) cho người chơi.")]
    public class SlidingStateAsset : PlayerStateAsset
    {
        [Tooltip("Độ ma sát/tốc độ khi trượt (càng cao trượt càng nhanh).")]
        public float SlidingFriction = 2f;
        [Tooltip("Tốc độ thay đổi gia tốc trượt.")]
        public float SpeedChange = 2f;
        [Tooltip("Tốc độ chuyển đổi chuyển động từ đi/chạy sang trượt.")]
        public float MotionChange = 2f;
        [Tooltip("Khả năng điều hướng khi đang trượt dốc.")]
        public float SlideControlChange = 2f;
        [Tooltip("Cho phép điều khiển người chơi sang trái/phải khi trượt dốc.")]
        public bool SlideControl = true;

        public override FSMPlayerState InitState(PlayerStateMachine machine, PlayerStatesGroup group)
        {
            return new SlidingPlayerState(machine, this);
        }

        public override string StateKey => PlayerStateMachine.SLIDING_STATE;

        public override string Name => "Generic/Sliding";

        public class SlidingPlayerState : FSMPlayerState
        {
            protected readonly SlidingStateAsset State;

            private bool isSliding;
            private float slidingSpeed;

            private Vector3 enterMotion;
            private float motionToSlidingBlend;

            public SlidingPlayerState(PlayerStateMachine machine, PlayerStateAsset stateAsset) : base(machine) 
            {
                State = (SlidingStateAsset)stateAsset;
            }

            public override void OnStateEnter()
            {
                controllerState = machine.StandingState;
                slidingSpeed = machine.Motion.magnitude;
                enterMotion = machine.Motion;
                motionToSlidingBlend = 0f;
                InputManager.ResetToggledButtons();
            }

            public override void OnStateUpdate()
            {
                bool sliding = SlopeCast(out Vector3 normal, out float angle);
                isSliding = sliding && angle > machine.PlayerSliding.SlopeLimit;

                Vector3 slidingForward = Vector3.ProjectOnPlane(Vector3.down, normal);
                Vector3 slidingRight = Vector3.Cross(normal, slidingForward);

                Vector3 slidingDirection = slidingForward;
                if (State.SlideControl) slidingDirection += machine.Input.x * State.SlideControlChange * slidingRight;

                slidingSpeed = Mathf.MoveTowards(slidingSpeed, State.SlidingFriction, Time.deltaTime * State.SpeedChange);
                slidingDirection = slidingDirection.normalized * slidingSpeed;

                motionToSlidingBlend = Mathf.MoveTowards(motionToSlidingBlend, 1f, Time.deltaTime * State.MotionChange);
                Vector3 finalMotion = Vector3.Lerp(enterMotion, slidingDirection, motionToSlidingBlend);

                machine.Motion = finalMotion;
                PlayerHeightUpdate();
            }

            public override Transition[] OnGetTransitions()
            {
                return new Transition[]
                {
                    Transition.To(PlayerStateMachine.IDLE_STATE, () => !isSliding),
                    Transition.To(PlayerStateMachine.DEATH_STATE, () => IsDead)
                };
            }
        }
    }
}