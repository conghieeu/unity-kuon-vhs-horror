using System;
using System.Linq;
using System.Reactive.Subjects;
using System.Collections.Generic;
using UnityEngine;
using UHFPS.Input;
using UHFPS.Tools;
using UHFPS.Scriptable;
using ThunderWire.Attributes;

namespace UHFPS.Runtime
{
    [RequireComponent(typeof(CharacterController))]
    [Docs("https://docs.twgamesdev.com/uhfps/guides/state-machines/adding-player-states")]
    public class PlayerStateMachine : PlayerComponent
    {
        #region Getters / Setters
        private CharacterController m_controller;
        public CharacterController Controller
        {
            get
            {
                if (m_controller == null)
                    m_controller = GetComponent<CharacterController>();

                return m_controller;
            }
        }

        public Vector3 FeetOffset
        {
            get
            {
                float height = Controller.height;
                float skinWidth = Controller.skinWidth;
                float center = height / 2;

                return ControllerOffset switch
                {
                    PositionOffset.Ground => new Vector3(0, skinWidth, 0),
                    PositionOffset.Feet => new Vector3(0, 0, 0),
                    PositionOffset.Center => new Vector3(0, -center, 0),
                    PositionOffset.Head => new Vector3(0, -center * 2, 0),
                    _ => Controller.center
                };
            }
        }

        public Vector3 ControllerFeet
        {
            get
            {
                Vector3 position = transform.position;
                return position + FeetOffset;
            }
        }

        public Vector3 ControllerCenter
        {
            get
            {
                Vector3 position = transform.position;
                return position += Controller.center;
            }
        }

        public State CurrentState => currentState;

        public State PreviousState => previousState;

        public string CurrentStateKey => CurrentState?.stateData.stateAsset.StateKey;
        #endregion

        #region Structures
        public const string PREVIOUS_STATE = "_Previous";

        public const string IDLE_STATE = "Idle";
        public const string WALK_STATE = "Walk";
        public const string RUN_STATE = "Run";
        public const string CROUCH_STATE = "Crouch";
        public const string JUMP_STATE = "Jump";

        public const string LADDER_STATE = "Ladder";
        public const string ZIPLINE_STATE = "Zipline";
        public const string SLIDING_STATE = "Sliding";
        public const string PUSHING_STATE = "Pushing";
        public const string DEATH_STATE = "Death";
        public const string HIDING_STATE = "Hiding";

        [Serializable]
        public sealed class BasicSettings
        {
            public float WalkSpeed = 3;
            public float RunSpeed = 7;
            public float CrouchSpeed = 2;
            public float JumpHeight = 1;
        }

        [Serializable]
        public sealed class ControllerFeatures
        {
            public bool EnableStamina = false;
            public bool RunToggle = false;
            public bool CrouchToggle = false;
            public bool NormalizeMovement = false;
        }

        [Serializable]
        public sealed class SlidingSettings
        {
            public LayerMask SlidingMask;
            public float SlideRayLength = 1f;
            public float SlopeLimit = 45f;
        }

        [Serializable]
        public sealed class StaminaSettings
        {
            public float JumpExhaustion = 1f;
            public float RunExhaustionSpeed = 1f;
            public float StaminaRegenSpeed = 1f;
            public float RegenerateAfter = 2f;
        }

        [Serializable]
        public sealed class ControllerSettings
        {
            public float BaseGravity = -9.81f;
            public float PlayerWeight = 70f;
            public float SkinWidthOffset = 0.05f;
            public float FeetRadius = 0.1f;
            public float AntiBumpFactor = 4.5f;
            public float WallRicochet = 0.1f;
            public float StateChangeSmooth = 1.35f;
        }

        [Serializable]
        public sealed class ControllerState
        {
            public float ControllerHeight;
            public Vector3 CameraOffset;
        }

        public sealed class State
        {
            public PlayerStateData stateData;
            public FSMPlayerState fsmState;
        }
        #endregion

        public enum PositionOffset { Ground, Feet, Center, Head }

        public PlayerStatesGroup StatesAsset;
        public PlayerStatesGroup StatesAssetRuntime;

        public LayerMask SurfaceMask;
        public PositionOffset ControllerOffset;

        public BasicSettings PlayerBasicSettings;
        public ControllerFeatures PlayerFeatures;
        public SlidingSettings PlayerSliding;
        public StaminaSettings PlayerStamina;
        public ControllerSettings PlayerControllerSettings;

        public ControllerState StandingState;
        public ControllerState CrouchingState;
        public List<ControllerState> CustomStates = new();

        public bool DrawPlayerGizmos = true;
        public bool DrawPlayerWireframe = true;
        public float ScaleOffset = 0f;
        public Color GizmosColor = Color.white;

        public Vector2 Input;
        public Vector3 Motion;

        public ControllerColliderHit ControllerHit { get; private set; }
        public BehaviorSubject<float> Stamina { get; set; } = new(1f);
        public bool IsGrounded { get; private set; }
        public bool IsPlayerDead { get; private set; }

        /// <summary>
        /// Check that the player is not airborne by checking the ground status and the current status of the player.
        /// </summary>
        public bool StateGrounded
        {
            get => IsGrounded
                || IsCurrent(SLIDING_STATE) 
                || IsCurrent(LADDER_STATE) 
                || IsCurrent(PUSHING_STATE);
        }

        /// <summary>
        /// Check whether the character controller is enabled.
        /// </summary>
        public bool ControllerEnabled
        {
            get => Controller.enabled;
            set => Controller.enabled = value;
        }

        /// <summary>
        /// The name of the current active state.
        /// </summary>
        public string StateName
        {
            get
            {
                if (currentState != null)
                    return currentState?.stateData.stateAsset.StateKey;

                return "None";
            }
        }

        /// <summary>
        /// The name of the current active state as observable.
        /// </summary>
        public BehaviorSubject<string> ObservableState = new("None");

        private readonly List<ICharacterControllerHit> currentSurfaces = new();
        private MultiKeyDictionary<string, Type, State> playerStates;

        private State currentState;
        private State previousState;

        private bool stateEntered;
        private float staminaRegenTime;

        private Vector3 externalForce;
        private Mesh playerGizmos;

        private void Awake()
        {
            playerStates = new MultiKeyDictionary<string, Type, State>();
            StatesAssetRuntime = Instantiate(StatesAsset);

            if (StatesAsset != null)
            {
                // initialize all states
                foreach (var playerState in StatesAssetRuntime.GetStates(this))
                {
                    Type stateType = playerState.stateData.stateAsset.GetType();
                    string stateKey = playerState.stateData.stateAsset.StateKey;
                    playerStates.Add(stateKey, stateType, playerState);
                }

                // select initial player state
                if (playerStates.Count > 0)
                {
                    stateEntered = false;
                    ChangeState(playerStates.subDictionary.Keys.First());
                }
            }
        }

        private void Update()
        {
            if (isEnabled) GetInput();
            else Input = Vector2.zero;

            // player death event
            if (currentState != null && !IsPlayerDead && PlayerManager.PlayerHealth.IsDead)
            {
                currentState?.fsmState.OnPlayerDeath();
                IsPlayerDead = true;
            }

            if (!stateEntered)
            {
                // enter state
                currentState?.fsmState.OnStateEnter();
                string stateName = currentState?.stateData.stateAsset.StateKey;
                ObservableState.OnNext(stateName);
                stateEntered = true;
            }
            else if (currentState != null)
            {
                // update state
                currentState?.fsmState.OnStateUpdate();

                // check state transitions
                if (currentState.fsmState.Transitions != null)
                {
                    foreach (var transition in currentState.fsmState.Transitions)
                    {
                        string nextStateKey = transition.NextStateKey;
                        if (playerStates.TryGetValue(nextStateKey, out State state))
                        {
                            if (state.stateData.isEnabled && StateName != nextStateKey && transition.Value)
                            {
                                if (playerStates.ContainsKey(nextStateKey))
                                {
                                    if (nextStateKey == PREVIOUS_STATE)
                                        ChangeToPreviousState();
                                    else ChangeState(state);
                                    break;
                                }
                            }
                        }
                    }
                }
            }

            // regenerate player stamina
            if (PlayerFeatures.EnableStamina)
            {
                bool runHold = InputManager.ReadButton(Controls.SPRINT);
                if (IsCurrent(RUN_STATE) || IsCurrent(JUMP_STATE) || runHold)
                {
                    staminaRegenTime = PlayerStamina.RegenerateAfter;
                }
                else if(staminaRegenTime > 0f)
                {
                    staminaRegenTime -= Time.deltaTime;
                }
                else if(Stamina.Value < 1f)
                {
                    float stamina = Stamina.Value;
                    stamina = Mathf.MoveTowards(stamina, 1f, Time.deltaTime * PlayerStamina.StaminaRegenSpeed);
                    Stamina.OnNext(stamina);
                    staminaRegenTime = 0f;
                }
            }

            float feetRadius = PlayerControllerSettings.FeetRadius;
            float maxDistance = Controller.skinWidth + PlayerControllerSettings.SkinWidthOffset + feetRadius;
            Vector3 rayOrigin = ControllerFeet + Vector3.up * (feetRadius + Controller.skinWidth);
            Ray groundRay = new(rayOrigin, Vector3.down);

            // raycast for character controller enter/exit event
            if (Physics.SphereCast(groundRay, feetRadius, out RaycastHit hit, maxDistance, SurfaceMask, QueryTriggerInteraction.Collide))
            {
                if (hit.collider.gameObject.TryGetComponent(out ICharacterControllerHit newSurface))
                {
                    if (!currentSurfaces.Contains(newSurface))
                    {
                        newSurface.OnCharacterControllerEnter(Controller);

                        currentSurfaces.ForEach(x => x.OnCharacterControllerExit());
                        currentSurfaces.Clear();
                        currentSurfaces.Add(newSurface);
                    }
                }
                else
                {
                    currentSurfaces.ForEach(x => x.OnCharacterControllerExit());
                    currentSurfaces.Clear();
                }
            }
            else
            {
                currentSurfaces.ForEach(x => x.OnCharacterControllerExit());
                currentSurfaces.Clear();
            }

            // apply external force
            if (externalForce != Vector3.zero)
            {
                Motion += externalForce;
                externalForce = Vector3.zero;
            }

            // apply movement direction
            if (Controller.enabled) IsGrounded = (Controller.Move(Motion * Time.deltaTime) & CollisionFlags.Below) != 0;
        }

        private void FixedUpdate()
        {
            if (currentState != null)
            {
                // update state
                currentState?.fsmState.OnStateFixedUpdate();
            }
        }

        private void OnControllerColliderHit(ControllerColliderHit hit)
        {
            if (!IsGrounded)
            {
                Vector3 normal = hit.normal;
                if (normal.y > 0) return;

                Vector3 ricochet = Vector3.Reflect(Motion, normal);
                ricochet.y = Motion.y;

                float ricochetDot = Mathf.Clamp01(Vector3.Dot(ricochet.normalized, Motion.normalized));
                float wallDot = Mathf.Clamp01(Vector3.Dot(Motion.normalized, -normal));

                float ricochetMul = Mathf.Lerp(1f, PlayerControllerSettings.WallRicochet, wallDot);
                ricochet *= ricochetMul;

                Vector3 newMotion = Vector3.Lerp(ricochet, Motion, ricochetDot);
                newMotion.y = Motion.y;

                Motion = newMotion;
            }

            ControllerHit = hit;
        }

        /// <summary>
        /// Calculate movement input vector.
        /// </summary>
        private void GetInput()
        {
            Input = Vector2.zero;
            if (InputManager.ReadInput(Controls.MOVEMENT, out Vector2 _rawInput))
            {
                if (PlayerFeatures.NormalizeMovement)
                {
                    _rawInput.y = _rawInput.y > 0.1f ? 1 : _rawInput.y < -0.1f ? -1 : 0;
                    _rawInput.x = _rawInput.x > 0.1f ? 1 : _rawInput.x < -0.1f ? -1 : 0;
                }
                Input = _rawInput;
            }
        }

        /// <summary>
        /// Set state enabled value. (Can transition to the state condition.)
        /// </summary>
        public void SetStateEnabled(string stateKey, bool enabled)
        {
            if (playerStates.TryGetValue(stateKey, out State state))
            {
                state.stateData.isEnabled = enabled;
            }
        }

        /// <summary>
        /// Add external force to the player motion.
        /// </summary>
        public void AddForce(Vector3 force, ForceMode mode)
        {
            float mass = PlayerControllerSettings.PlayerWeight;

            externalForce += mode switch
            {
                ForceMode.Force => force * (1f / mass),
                ForceMode.Acceleration => force * Time.deltaTime,
                ForceMode.Impulse => force * (1f / mass),
                ForceMode.VelocityChange => force,
                _ => throw new ArgumentException(nameof(mode)),
            };
        }

        /// <summary>
        /// Set player controller state.
        /// </summary>
        public Vector3 SetControllerState(ControllerState state)
        {
            float height = state.ControllerHeight;
            float skinWidth = Controller.skinWidth;
            float center = height / 2;

            Vector3 controllerCenter = ControllerOffset switch
            {
                PositionOffset.Ground => new Vector3(0, center + skinWidth, 0),
                PositionOffset.Feet => new Vector3(0, center, 0),
                PositionOffset.Center => new Vector3(0, 0, 0),
                PositionOffset.Head => new Vector3(0, -center, 0),
                _ => Controller.center
            };

            Controller.height = height;
            Controller.center = controllerCenter;

            Vector3 cameraTop = state.CameraOffset;
            cameraTop.y += center + controllerCenter.y;

            return cameraTop;
        }

        /// <summary>
        /// Change player pose to standing.
        /// </summary>
        public void ToStandingPose()
        {
            Vector3 cameraPos = SetControllerState(StandingState);
            PlayerManager.CameraHolder.localPosition = cameraPos;
        }

        /// <summary>
        /// Get player FSM state.
        /// </summary>
        public FSMPlayerState GetState<TState>() where TState : PlayerStateAsset
        {
            if (playerStates.TryGetValue(typeof(TState), out State state))
            {
                return state.fsmState;
            }

            return null;
        }

        /// <summary>
        /// Get player FSM state by type.
        /// </summary>
        public FSMPlayerState GetState(Type stateType)
        {
            if (playerStates.TryGetValue(stateType, out State state))
            {
                return state.fsmState;
            }

            return null;
        }

        /// <summary>
        /// Change player FSM state.
        /// </summary>
        public void ChangeState<TState>() where TState : PlayerStateAsset
        {
            if (playerStates.TryGetValue(typeof(TState), out State state))
            {
                if (!isEnabled && !state.fsmState.CanTransitionWhenDisabled)
                    return;

                if ((currentState == null || !currentState.Equals(state)) && state.stateData.isEnabled)
                {
                    currentState?.fsmState.OnStateExit();
                    if (currentState != null) previousState = currentState;
                    currentState = state;
                    stateEntered = false;
                }
                return;
            }

            throw new MissingReferenceException($"Could not find a state with type '{typeof(TState).Name}'");
        }

        /// <summary>
        /// Change player FSM state.
        /// </summary>
        public void ChangeState(Type nextState)
        {
            if (playerStates.TryGetValue(nextState, out State state))
            {
                if (!isEnabled && !state.fsmState.CanTransitionWhenDisabled)
                    return;

                if ((currentState == null || !currentState.Equals(state)) && state.stateData.isEnabled)
                {
                    currentState?.fsmState.OnStateExit();
                    if (currentState != null) previousState = currentState;
                    currentState = state;
                    stateEntered = false;
                }
                return;
            }

            throw new MissingReferenceException($"Could not find a state with type '{nextState.Name}'");
        }

        /// <summary>
        /// Change player FSM state.
        /// </summary>
        public void ChangeState(State state)
        {
            if (!isEnabled && !state.fsmState.CanTransitionWhenDisabled)
                return;

            if ((currentState == null || !currentState.Equals(state)))
            {
                currentState?.fsmState.OnStateExit();
                if (currentState != null) previousState = currentState;
                currentState = state;
                stateEntered = false;
            }
        }

        /// <summary>
        /// Change player FSM state.
        /// </summary>
        public void ChangeState(string nextState)
        {
            if (playerStates.TryGetValue(nextState, out State state))
            {
                if (!isEnabled && !state.fsmState.CanTransitionWhenDisabled)
                    return;

                if ((currentState == null || !currentState.Equals(state)) && state.stateData.isEnabled)
                {
                    currentState?.fsmState.OnStateExit();
                    if (currentState != null) previousState = currentState;
                    currentState = state;
                    stateEntered = false;
                }
                return;
            }

            throw new MissingReferenceException($"Could not find a state with key '{nextState}'");
        }

        /// <summary>
        /// Change player FSM state and set the state data.
        /// </summary>
        public void ChangeState(string nextState, StorableCollection stateData)
        {
            if (playerStates.TryGetValue(nextState, out State state))
            {
                if (!isEnabled && !state.fsmState.CanTransitionWhenDisabled)
                    return;

                if ((currentState == null || !currentState.Equals(state)) && state.stateData.isEnabled)
                {
                    currentState?.fsmState.OnStateExit();
                    if (currentState != null) previousState = currentState;
                    state.fsmState.StateData = stateData;
                    currentState = state;
                    stateEntered = false;
                }
                return;
            }

            throw new MissingReferenceException($"Could not find a state with key '{nextState}'");
        }

        /// <summary>
        /// Change player FSM state to previous state.
        /// </summary>
        public void ChangeToPreviousState()
        {
            if (previousState != null && !currentState.Equals(previousState) && previousState.stateData.isEnabled)
            {
                if (!isEnabled && !previousState.fsmState.CanTransitionWhenDisabled)
                    return;

                currentState?.fsmState.OnStateExit();
                State temp = currentState;
                currentState = previousState;
                previousState = temp;
                stateEntered = false;
            }
        }

        /// <summary>
        /// Change player FSM state to Idle
        /// </summary>
        public void ChangeToIdle()
        {
            ChangeState(IDLE_STATE);
        }

        /// <summary>
        /// Check if current state is of the specified type.
        /// </summary>
        public bool IsCurrent(Type stateType)
        {
            return currentState?.stateData.stateAsset.GetType() == stateType;
        }

        /// <summary>
        /// Check if current state matches the specified state key.
        /// </summary>
        public bool IsCurrent(string stateKey)
        {
            return currentState?.stateData.stateAsset.StateKey == stateKey;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(ControllerFeet, 0.02f);

            Gizmos.color = Color.cyan;
            Gizmos.DrawSphere(ControllerCenter, 0.05f);

            if (currentState != null)
                currentState?.fsmState.OnDrawGizmos();

            if (PlayerManager.MainVirtualCamera != null)
            {
                Vector3 camForward = PlayerManager.MainVirtualCamera.transform.forward;
                Vector3 lookForward = LookController.RotationX * Vector3.forward;
                Vector3 lookRotation = Application.isPlaying ? lookForward : camForward;
                
                Gizmos.color = Color.red;
                GizmosE.DrawGizmosArrow(ControllerFeet, lookRotation * 0.5f);
            }

            Gizmos.color = Color.white;
            Gizmos.DrawRay(ControllerFeet, Vector3.down * (Controller.skinWidth + PlayerControllerSettings.SkinWidthOffset));
        }

        private void OnDrawGizmos()
        {
            if (DrawPlayerGizmos)
            {
                if (playerGizmos == null)
                {
                    playerGizmos = Resources.Load<Mesh>("Gizmos/Player");
                }
                else
                {
                    float height = Controller.height;
                    Vector3 scale = (0.73f + ScaleOffset) * height * Vector3.one;
                    Quaternion lookRotation = Application.isPlaying ? LookController.RotationX : transform.rotation;
                    Quaternion rotation = lookRotation * Quaternion.Euler(-90, 0, 0);

                    Gizmos.color = GizmosColor.Alpha(0.1f);
                    if(DrawPlayerWireframe) Gizmos.DrawWireMesh(playerGizmos, transform.position, rotation, scale);
                    else Gizmos.DrawMesh(playerGizmos, transform.position, rotation, scale);
                }
            }
        }
    }
}