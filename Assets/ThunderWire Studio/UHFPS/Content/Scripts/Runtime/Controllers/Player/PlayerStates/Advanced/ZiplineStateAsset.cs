using UnityEngine;
using UHFPS.Input;
using UHFPS.Tools;
using UHFPS.Scriptable;

namespace UHFPS.Runtime.States
{
    public class ZiplineStateAsset : PlayerStateAsset
    {
        public ControlsContext ControlExit;

        public float EvaluationSpeed = 0.1f;
        public float ZiplineSpeed = 0.1f;
        public float ZiplineEndEval = 0.95f;

        [Header("Sounds")]
        public SoundClip ZiplineEnter;
        public SoundClip ZiplineExit;
        public SoundClip ZiplineSliding;

        public override FSMPlayerState InitState(PlayerStateMachine machine, PlayerStatesGroup group)
        {
            return new ZiplinePlayerState(machine, this);
        }

        public override string StateKey => PlayerStateMachine.ZIPLINE_STATE;

        public override string Name => "Generic/Zipline";

        public class ZiplinePlayerState : FSMPlayerState
        {
            private readonly ZiplineStateAsset State;
            private readonly AudioSource audioSource;
            private Collider interactCollider;

            private Vector3 ziplineStart;
            private Vector3 ziplineEnd;
            private Vector3 ziplineCurvatore;
            private Vector3 startPosition;

            private bool exitState = false;
            private bool enterZipline = false;
            private float bezierEval = 0;

            public ZiplinePlayerState(PlayerStateMachine machine, ZiplineStateAsset stateAsset) : base(machine) 
            {
                State = stateAsset;
                State.ControlExit.SubscribeGloc();
                audioSource = machine.GetComponent<AudioSource>();
            }

            public override Transition[] OnGetTransitions()
            {
                return new Transition[]
                {
                    Transition.To(PlayerStateMachine.IDLE_STATE, () =>
                    {
                        return exitState || InputManager.ReadButtonOnce("WalkState", Controls.JUMP);
                    }),
                    Transition.To(PlayerStateMachine.DEATH_STATE, () => IsDead)
                };
            }

            public override void OnStateEnter()
            {
                GameObject gameObject = (GameObject)StateData["object"];
                Vector3 center = (Vector3)StateData["center"];
                ziplineStart = center + (Vector3)StateData["start"];
                ziplineEnd = center + (Vector3)StateData["end"];
                ziplineCurvatore = center + (Vector3)StateData["curvatore"];

                if (gameObject.TryGetComponent(out interactCollider))
                    interactCollider.enabled = false;

                /*
                Vector3 projection = Vector3.Project(CenterPosition - ziplineStart, ziplineEnd - ziplineStart) + ziplineStart;
                bezierEval = Vector3.Distance(ziplineStart, projection) / Vector3.Distance(ziplineStart, ziplineEnd);
                */

                startPosition = ziplineStart;
                startPosition.y = CenterPosition.y;

                exitState = false;
                motionController.SetEnabled(false);
                InputManager.ResetToggledButtons();
                playerItems.DeactivateCurrentItem();

                audioSource.loop = true;
                playerItems.IsItemsUsable = false;

                gameManager.ShowControlsInfo(true, State.ControlExit);
            }

            public override void OnStateExit()
            {
                playerItems.IsItemsUsable = true;

                bezierEval = 0;
                enterZipline = false;

                if(!IsDead) motionController.SetEnabled(true);
                audioSource.PlayOneShotSoundClip(State.ZiplineExit);
                audioSource.loop = false;
                audioSource.clip = null;

                if (interactCollider != null)
                    interactCollider.enabled = true;

                gameManager.ShowControlsInfo(false, null);
            }

            public override void OnStateUpdate()
            {
                if(Vector3.Distance(CenterPosition, startPosition) > 0.1f && !enterZipline)
                {
                    Vector3 desiredPosition = (startPosition - CenterPosition).normalized;
                    machine.Motion = 2 * State.ZiplineSpeed * desiredPosition;
                }
                else if (!enterZipline)
                {
                    audioSource.PlayOneShotSoundClip(State.ZiplineEnter);
                    audioSource.SetSoundClip(State.ZiplineSliding);
                    enterZipline = true;
                }
                else
                {
                    if (!audioSource.isPlaying) audioSource.Play();

                    bezierEval += State.EvaluationSpeed * Time.deltaTime;
                    bezierEval = Mathf.Clamp01(bezierEval);

                    Vector3 desiredPosition = VectorE.QuadraticBezier(ziplineStart, ziplineEnd, ziplineCurvatore, bezierEval);
                    Vector3 motion = (desiredPosition - CenterPosition).normalized;
                    machine.Motion = Vector3.Distance(CenterPosition, desiredPosition) * State.ZiplineSpeed * motion;
                }

                // check if player is close to exit distance
                exitState = bezierEval >= State.ZiplineEndEval;

                controllerState = machine.StandingState;
                PlayerHeightUpdate();
            }
        }
    }
}