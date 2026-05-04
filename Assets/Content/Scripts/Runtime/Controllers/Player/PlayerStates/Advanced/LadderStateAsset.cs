using UnityEngine;
using UHFPS.Scriptable;
using UHFPS.Input;
using UHFPS.Tools;

namespace UHFPS.Runtime.States
{
    public class LadderStateAsset : PlayerStateAsset
    {
        public ControlsContext ControlExit;

        [Header("Speed")]
        public float OnLadderSpeed = 1.5f;
        public float ToLadderSpeed = 3f;
        public float BezierLadderSpeed = 3f;
        public float BezierEvalSpeed = 1f;

        [Header("Distances")]
        public float OnLadderDistance = 0.1f;
        public float EndLadderDistance = 0.1f;

        [Header("Settings")]
        public float LadderFrontAngle = 10f;
        public float PlayerCenterOffset = 0.5f;
        public float GroundToLadderOffset = 0.1f;

        [Header("Sounds")]
        [Range(0f, 1f)] public float FootstepsVolume = 1f;
        public float LadderStepTime = 0.5f;
        public AudioClip[] LadderFootsteps;

        public override FSMPlayerState InitState(PlayerStateMachine machine, PlayerStatesGroup group)
        {
            return new LadderPlayerState(machine, this);
        }

        public override string StateKey => PlayerStateMachine.LADDER_STATE;

        public override string Name => "Generic/Ladder";

        public class LadderPlayerState : FSMPlayerState
        {
            protected readonly LadderStateAsset State;

            private readonly AudioSource audioSource;
            private Collider interactCollider;

            private Transform ladder;
            private Vector3 ladderStart;
            private Vector3 ladderEnd;
            private Vector3 ladderExit;
            private Vector3 ladderArc;

            private Vector3 startPosition;
            private Vector3 movePosition;

            private float bezierEval;
            private float stepTime;
            private int lastStep;

            private bool playerMoved;
            private bool exitLadder;
            private bool climbDown;
            private bool exitState;

            private Vector3 OffsetedCenter
            {
                get
                {
                    Vector3 center = CenterPosition;
                    center.y += State.PlayerCenterOffset;
                    return center;
                }
            }

            public LadderPlayerState(PlayerStateMachine machine, PlayerStateAsset stateAsset) : base(machine)
            {
                State = (LadderStateAsset)stateAsset;
                State.ControlExit.SubscribeGloc();
                audioSource = machine.GetComponent<AudioSource>();
            }

            public override Transition[] OnGetTransitions()
            {
                return new Transition[]
                {
                    Transition.To(PlayerStateMachine.IDLE_STATE, () =>
                    {
                        return exitState || InputManager.ReadButtonOnce("Jump", Controls.JUMP);
                    }),
                    Transition.To(PlayerStateMachine.DEATH_STATE, () => IsDead)
                };
            }

            public override void OnStateEnter()
            {
                ladder = (Transform)StateData["transform"];
                ladderStart = (Vector3)StateData["start"];
                ladderEnd = (Vector3)StateData["end"];
                ladderExit = (Vector3)StateData["exit"];
                ladderArc = (Vector3)StateData["arc"];

                if (ladder.TryGetComponent(out interactCollider))
                    interactCollider.enabled = false;

                bool useMouseLimits = (bool)StateData["useLimits"];
                MinMax verticalLimits = (MinMax)StateData["verticalLimits"];
                MinMax horizontalLimits = (MinMax)StateData["horizontalLimits"];

                playerMoved = false;
                exitLadder = false;
                exitState = false;
                climbDown = false;
                bezierEval = 0;

                // set look rotation and limits
                if (useMouseLimits)
                {
                    Vector3 ladderRotation = ladder.rotation.eulerAngles;
                    cameraLook.LerpClampRotation(ladderRotation, verticalLimits, horizontalLimits);
                }

                // set ladder climb direction
                if (climbDown = LadderDotUp() < 0)
                {
                    startPosition = CenterPosition;
                    movePosition = ladderEnd;
                    movePosition.y -= State.PlayerCenterOffset + 0.1f;

                    // player is in front of ladder
                    climbDown = LadderDotForward() < State.LadderFrontAngle;
                }
                else
                {
                    float ladderT = LadderEval(CenterPosition);
                    movePosition = Vector3.Lerp(ladderStart, ladderEnd, ladderT);
                    movePosition.y += State.GroundToLadderOffset;
                }

                playerItems.DeactivateCurrentItem();
                playerItems.IsItemsUsable = false;

                gameManager.ShowControlsInfo(true, State.ControlExit);
                controllerState = machine.StandingState;
            }

            public override void OnStateExit()
            {
                playerItems.IsItemsUsable = true;
                cameraLook.ResetLookLimits();
                if (interactCollider != null) 
                    interactCollider.enabled = true;

                gameManager.ShowControlsInfo(false, null);
            }

            public override void OnStateUpdate()
            {
                if (!playerMoved)
                {
                    if (climbDown || exitLadder)
                    {
                        bezierEval += State.BezierEvalSpeed * Time.deltaTime;
                        bezierEval = Mathf.Clamp01(bezierEval);

                        // use QuadraticBezier to move player to the desired position
                        Vector3 bezierPosition = VectorE.QuadraticBezier(startPosition, movePosition, ladderArc, bezierEval);
                        CenterPosition = Vector3.MoveTowards(CenterPosition, bezierPosition, Time.deltaTime * State.BezierLadderSpeed);
                        machine.Motion = Vector3.zero;
                        Physics.SyncTransforms();
                    }
                    else
                    {
                        Vector3 ladderMotion = (movePosition - CenterPosition).normalized;
                        machine.Motion = State.ToLadderSpeed * ladderMotion;
                    }

                    if (exitLadder)
                    {
                        exitState = Vector3.Distance(CenterPosition, movePosition) <= State.EndLadderDistance;
                    }
                    else
                    {
                        playerMoved = Vector3.Distance(CenterPosition, movePosition) <= State.OnLadderDistance;
                    }
                }
                else
                {
                    LadderClimbUpdate();
                }

                PlayerHeightUpdate();
            }

            private void LadderClimbUpdate()
            {
                Vector3 playerPos = new Vector3(CenterPosition.x, 0, CenterPosition.z);
                Vector3 ladderPos = new Vector3(ladderStart.x, 0, ladderStart.z);
                Vector3 direction = ladderPos - playerPos;
                Vector3 ladderMotion = direction.normalized;
                int magnitude = direction.magnitude > .05f ? 1 : 0;

                // assign ladder motion
                ladderMotion.x *= State.ToLadderSpeed * magnitude;
                ladderMotion.z *= State.ToLadderSpeed * magnitude;
                ladderMotion.y = machine.Input.y * State.OnLadderSpeed;

                // set ladder motion to machine
                machine.Motion = ladderMotion;

                // check if player should climb up and exit ladder
                if (LadderEval(OffsetedCenter) >= 1f)
                {
                    startPosition = CenterPosition;
                    movePosition = ladderExit;
                    playerMoved = false;
                    exitLadder = true;
                    bezierEval = 0;
                }

                // check if player touches ground to exit ladder
                exitState = machine.IsGrounded;

                // ladder sounds
                if(stepTime > 0) stepTime -= Time.deltaTime;
                else if (State.LadderFootsteps.Length > 0 && Mathf.Abs(machine.Input.y) > 0)
                {
                    lastStep = GameTools.RandomUnique(0, State.LadderFootsteps.Length, lastStep);
                    AudioClip footstep = State.LadderFootsteps[lastStep];
                    audioSource.PlayOneShot(footstep, State.FootstepsVolume);
                    stepTime = State.LadderStepTime;
                }
            }

            private float LadderEval(Vector3 playerCenter)
            {
                Vector3 projection = Vector3.Project(playerCenter - ladderStart, ladderEnd - ladderStart) + ladderStart;
                return Vector3.Distance(ladderStart, projection) / Vector3.Distance(ladderStart, ladderEnd);
            }

            private float LadderDotUp()
            {
                Vector3 ladderPos = ladderEnd;
                Vector3 playerPos = CenterPosition;
                Vector3 p1 = new Vector3(0, playerPos.y, 0);
                Vector3 p2 = new Vector3(0, ladderPos.y, 0);
                return Vector3.Dot((p2 - p1).normalized, Vector3.up);
            }

            private float LadderDotForward()
            {
                Vector3 ladderPos = ladder.position;
                Vector3 playerPos = CenterPosition;
                Vector3 p1 = new Vector3(playerPos.x, 0, playerPos.z);
                Vector3 p2 = new Vector3(ladderPos.x, 0, ladderPos.z);
                return Vector3.Dot((p2 - p1).normalized, ladder.forward) * 90;
            }
        }
    }
}