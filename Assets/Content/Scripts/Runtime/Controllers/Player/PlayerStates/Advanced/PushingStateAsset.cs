using UnityEngine;
using UHFPS.Scriptable;
using UHFPS.Input;
using UHFPS.Tools;
using static UHFPS.Runtime.MovableObject;

namespace UHFPS.Runtime.States
{
    public class PushingStateAsset : PlayerStateAsset
    {
        public ControlsContext ControlExit;

        public float ToMovableTime;
        public float PushingSpeed;

        public override FSMPlayerState InitState(PlayerStateMachine machine, PlayerStatesGroup group)
        {
            return new PushingPlayerState(machine, this);
        }

        public override string StateKey => PlayerStateMachine.PUSHING_STATE;

        public override string Name => "Generic/Pushing";

        public class PushingPlayerState : FSMPlayerState
        {
            private readonly PushingStateAsset State;

            private BoxCollider collider;
            private Collider interactCollider;
            private LayerMask collisionMask;

            private MovableObject movableObject;
            private AudioSource audioSource;
            private Transform movable;

            private Axis forwardAxis;
            private MoveDirectionEnum direction;
            private Vector3 holdOffset;
            private bool allowRotation;

            private bool useLimits;
            private MinMax verticalLimits;

            private float slidingVolume;
            private float volumeFadeSpeed;

            private float holdDistance;
            private float oldSensitivity;

            private Vector3 startingPosition;
            private Vector3 targetPosition;
            private Vector2 targetLook;
            private bool isMoved;

            private float movementSpeed;
            private float prevRotationX;
            private float elapsedTime;

            private Vector3 CameraForward => cameraLook.RotationX * Vector3.forward;

            public PushingPlayerState(PlayerStateMachine machine, PushingStateAsset stateAsset) : base(machine)
            {
                State = stateAsset;
                State.ControlExit.SubscribeGloc();
            }

            public override void OnStateEnter()
            {
                movableObject = (MovableObject)StateData["reference"];
                audioSource = movableObject.AudioSource;
                movable = movableObject.RootMovable;
                collider = movable.GetComponent<BoxCollider>();

                if (movableObject.TryGetComponent(out interactCollider))
                    interactCollider.enabled = false;

                forwardAxis = movableObject.ForwardAxis;
                collisionMask = movableObject.CollisionMask;
                direction = movableObject.MoveDirection;
                holdOffset = movableObject.HoldOffset;
                allowRotation = movableObject.AllowRotation;

                useLimits = movableObject.UseMouseLimits;
                verticalLimits = movableObject.MouseVerticalLimits;

                slidingVolume = movableObject.SlideVolume;
                volumeFadeSpeed = movableObject.VolumeFadeSpeed;
                holdDistance = movableObject.HoldDistance;

                float weight = movableObject.ObjectWeight;
                float walkMultiplier = movableObject.WalkMultiplier;
                float lookMultiplier = movableObject.LookMultiplier;

                oldSensitivity = cameraLook.SensitivityX;

                float walkSpeed = machine.PlayerBasicSettings.WalkSpeed;
                float walkMul = Mathf.Min(1f, walkSpeed * 10f / weight);
                float lookMul = Mathf.Min(1f, oldSensitivity * 10f / weight);

                movementSpeed = walkSpeed * walkMul * walkMultiplier;
                if (allowRotation) cameraLook.SensitivityX = oldSensitivity * lookMul * lookMultiplier;
                else cameraLook.SensitivityX = 0f;

                Vector3 forwardGlobal = forwardAxis.Convert();
                Vector3 forwardLocal = movable.Direction(forwardAxis);
                Vector3 movablePos = movable.position;

                movablePos.y = Position.y;
                targetPosition = movable.TransformPoint((-forwardGlobal * holdDistance) + holdOffset);
                targetPosition.y = Position.y;

                holdOffset = Quaternion.LookRotation(forwardGlobal) * holdOffset;
                float angleOffset = Vector3.Angle(movable.forward, forwardLocal);
                float lookX = movable.eulerAngles.y - angleOffset;

                targetLook = new Vector2(lookX, 0f);
                motionController.SetEnabled(false);
                InputManager.ResetToggledButtons();

                startingPosition = Position;
                machine.Motion = Vector3.zero;
                elapsedTime = 0f;

                audioSource.volume = 0f;
                audioSource.Play();

                playerItems.DeactivateCurrentItem();
                playerItems.IsItemsUsable = false;

                gameManager.ShowControlsInfo(true, State.ControlExit);
                footstepsSystem.enabled = false;
            }

            public override void OnStateExit()
            {
                playerItems.IsItemsUsable = true;
                if (!IsDead) motionController.SetEnabled(true);
                movableObject.FadeSoundOut();
                cameraLook.ResetCustomLerp();
                cameraLook.ResetLookLimits();
                cameraLook.SensitivityX = oldSensitivity;
                targetPosition = Vector3.zero;
                isMoved = false;

                if (interactCollider != null)
                    interactCollider.enabled = true;

                gameManager.ShowControlsInfo(false, null);
                footstepsSystem.enabled = true;
            }

            public override void OnStateUpdate()
            {
                elapsedTime += Time.deltaTime;
                float t = GameTools.SmootherStep(0f, 1f, elapsedTime / State.ToMovableTime);

                if (t < 1f && !isMoved)
                {
                    Position = Vector3.Lerp(startingPosition, targetPosition, t);
                    cameraLook.CustomLerp(targetLook, t);
                }
                else if(!isMoved)
                {
                    Position = targetPosition;
                    cameraLook.ResetCustomLerp();
                    if (useLimits) cameraLook.SetVerticalLimits(verticalLimits);
                    isMoved = true;
                }
                else
                {
                    MovementUpdate();
                    SoundUpdate();
                }

                controllerState = machine.StandingState;
                PlayerHeightUpdate();
            }

            private void SoundUpdate()
            {
                Vector3 motion = controller.velocity;
                motion.y = 0f;

                float magnitude = motion.magnitude > 0 ? 1f : 0f;
                float targetVolume = slidingVolume * magnitude;
                audioSource.volume = Mathf.MoveTowards(audioSource.volume, targetVolume, Time.deltaTime * volumeFadeSpeed);
            }

            private void MovementUpdate()
            {
                Vector3 wishDir = direction switch
                {
                    MoveDirectionEnum.ForwardBackward   => new Vector3(0, 0, machine.Input.y),
                    MoveDirectionEnum.LeftRight         => new Vector3(machine.Input.x, 0, 0),
                    MoveDirectionEnum.AllDirections     => new Vector3(machine.Input.x, 0, machine.Input.y),
                    _ => Vector3.zero
                };

                wishDir = cameraLook.RotationX * wishDir;

                if (machine.IsGrounded)
                {
                    if (CanMove(wishDir)) machine.Motion = wishDir * movementSpeed;
                    else machine.Motion = Vector3.zero;

                    machine.Motion.y = -machine.PlayerControllerSettings.AntiBumpFactor;
                }

                ApplyGravity(ref machine.Motion);

                Vector3 movablePosition = Position;
                Vector3 offset = cameraLook.RotationX * holdOffset;

                movablePosition += CameraForward * holdDistance;
                movablePosition += offset;
                movablePosition.y = movable.position.y;
                movable.position = movablePosition;

                if (allowRotation)
                {
                    Vector3 movableForward = forwardAxis.Convert();
                    Quaternion rotation = Quaternion.LookRotation(movableForward, Vector3.up);
                    Quaternion result = Quaternion.LookRotation(rotation * CameraForward);

                    if (CanRotate(result)) prevRotationX = cameraLook.LookRotation.x;
                    else cameraLook.LookRotation.x = prevRotationX;

                    movable.rotation = result;
                }
            }

            private bool CanMove(Vector3 direction)
            {
                Vector3 newPosition = movable.position + movementSpeed * Time.deltaTime * direction;
                newPosition.y += 0.01f;

                return !Physics.CheckBox(newPosition, collider.size / 2, movable.rotation, collisionMask);
            }

            private bool CanRotate(Quaternion rotation)
            {
                Quaternion newRotation = rotation * movable.rotation;
                Vector3 position = movable.position + Vector3.up * 0.01f;

                return !Physics.CheckBox(position, collider.size / 2, newRotation, collisionMask);
            }

            public override Transition[] OnGetTransitions()
            {
                return new Transition[]
                {
                    Transition.To(PlayerStateMachine.WALK_STATE, () => InputManager.ReadButtonOnce("Jump", Controls.JUMP)),
                    Transition.To(PlayerStateMachine.DEATH_STATE, () => IsDead)
                };
            }
        }
    }
}