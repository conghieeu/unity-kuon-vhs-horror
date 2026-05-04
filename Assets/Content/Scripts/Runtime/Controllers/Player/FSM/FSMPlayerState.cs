using UnityEngine;

namespace UHFPS.Runtime
{
    public class FSMPlayerState : FSMState
    {
        public Transition[] Transitions { get; private set; }
        public StorableCollection StateData { get; set; }

        protected Transform cameraHolder;
        protected GameManager gameManager;
        protected PlayerStateMachine machine;
        protected CharacterController controller;
        protected PlayerItemsManager playerItems;
        protected MotionController motionController;
        protected LookController cameraLook;
        protected FootstepsSystem footstepsSystem;
        protected PlayerStateMachine.ControllerState controllerState;

        private Vector3 heightVelocity;

        /// <summary>
        /// Character controller current position.
        /// </summary>
        protected Vector3 Position
        {
            get => machine.transform.position;
            set => machine.transform.position = value;
        }

        /// <summary>
        /// Character controller center position.
        /// </summary>
        protected Vector3 CenterPosition
        {
            get => machine.ControllerCenter;
            set
            {
                Vector3 position = value;
                position -= controller.center;
                machine.transform.position = position;
            }
        }

        /// <summary>
        /// Character controller bottom position.
        /// </summary>
        protected Vector3 FeetPosition
        {
            get => machine.ControllerFeet;
            set
            {
                Vector3 position = value;
                position -= machine.ControllerFeet;
                machine.transform.position = position;
            }
        }

        /// <summary>
        /// The keyboard input movement value.
        /// </summary>
        protected Vector2 MovementInput => machine.Input;

        /// <summary>
        /// Check if you can transition to this state when the transition is disabled.
        /// </summary>
        public virtual bool CanTransitionWhenDisabled => false;

        /// <summary>
        /// The magnitude of the movement input.
        /// </summary>
        protected float InputMagnitude => machine.Input.magnitude;

        /// <summary>
        /// Check if the character controller is on the ground.
        /// </summary>
        protected bool IsGrounded => machine.IsGrounded;

        /// <summary>
        /// Check if the stamina feature is enabled in the player.
        /// </summary>
        protected bool StaminaEnabled => machine.PlayerFeatures.EnableStamina;

        /// <summary>
        /// Check if the player has died.
        /// </summary>
        protected bool IsDead => machine.IsPlayerDead;

        public FSMPlayerState(PlayerStateMachine machine)
        {
            this.machine = machine;
            gameManager = GameManager.Instance;
            controller = machine.Controller;
            playerItems = machine.PlayerManager.PlayerItems;
            cameraHolder = machine.PlayerManager.CameraHolder;
            motionController = machine.PlayerManager.MotionController;
            cameraLook = machine.LookController;
            footstepsSystem = machine.GetComponent<FootstepsSystem>();
            Transitions = OnGetTransitions();
        }

        /// <summary>
        /// Get player state transitions.
        /// </summary>
        public virtual Transition[] OnGetTransitions()
        {
            return new Transition[0];
        }

        /// <summary>
        /// Set custom controller state by index.
        /// </summary>
        public void SetCustomControllerState(int index)
        {
            if (index < 0 || index > machine.CustomStates.Count)
                throw new System.Exception("The controller state index exceeds the number of custom states.");

            controllerState = machine.CustomStates[index];
        }

        /// <summary>
        /// Change player controller height.
        /// </summary>
        public void PlayerHeightUpdate()
        {
            if (controllerState != null)
            {
                Vector3 cameraPosition = machine.SetControllerState(controllerState);
                float changeSpeed = machine.PlayerControllerSettings.StateChangeSmooth;

                Vector3 localPos = machine.PlayerManager.CameraHolder.localPosition;
                localPos = Vector3.SmoothDamp(localPos, cameraPosition, ref heightVelocity, changeSpeed);
                machine.PlayerManager.CameraHolder.localPosition = localPos;
            }
        }

        /// <summary>
        /// Get player gravity force with weight.
        /// </summary>
        public float GravityForce()
        {
            float gravity = machine.PlayerControllerSettings.BaseGravity;
            float weight = machine.PlayerControllerSettings.PlayerWeight / 10f;
            return gravity - weight;
        }

        /// <summary>
        /// Apply gravity force to motion.
        /// </summary>
        public void ApplyGravity(ref Vector3 motion)
        {
            float gravityForce = GravityForce();
            motion += gravityForce * Time.deltaTime * Vector3.up;
        }

        /// <summary>
        /// Check if the surface is a sliding surface.
        /// </summary>
        public bool SlopeCast(out Vector3 normal, out float angle)
        {
            LayerMask slidingMask = machine.PlayerSliding.SlidingMask;
            float slideRayLength = machine.PlayerSliding.SlideRayLength;

            if (Physics.SphereCast(CenterPosition, controller.radius, Vector3.down, out RaycastHit hit, slideRayLength, slidingMask))
            {
                normal = hit.normal;
                angle = Vector3.Angle(hit.normal, Vector3.up);
                return true;
            }

            normal = Vector3.zero;
            angle = 0f;
            return false;
        }

        /// <summary>
        /// Event when a player dies.
        /// </summary>
        public virtual void OnPlayerDeath() { }
    }
}