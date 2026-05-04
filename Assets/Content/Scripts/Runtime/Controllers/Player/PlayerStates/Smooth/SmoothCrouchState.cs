using UnityEngine;
using UHFPS.Scriptable;
using UHFPS.Input;

namespace UHFPS.Runtime.States
{
    public class SmoothCrouchState : SmoothStateAsset
    {
        public override FSMPlayerState InitState(PlayerStateMachine machine, PlayerStatesGroup group)
        {
            return new CrouchPlayerState(machine, group);
        }

        public override string StateKey => PlayerStateMachine.CROUCH_STATE;

        public override string Name => "Smooth/Crouch";

        public class CrouchPlayerState : SmoothPlayerState
        {
            public CrouchPlayerState(PlayerStateMachine machine, PlayerStatesGroup group) : base(machine, group)
            {
            }

            public override void OnStateEnter()
            {
                movementSpeed = machine.PlayerBasicSettings.CrouchSpeed;
                controllerState = machine.CrouchingState;
            }

            public override Transition[] OnGetTransitions()
            {
                return new Transition[]
                {
                    Transition.To(PlayerStateMachine.IDLE_STATE, () =>
                    {
                        if(gameManager.IsInventoryShown)
                            return false;

                        if (machine.PlayerFeatures.CrouchToggle)
                        {
                            if(InputManager.ReadButtonOnce("Jump", Controls.JUMP))
                            {
                                InputManager.ResetToggledButtons();
                                return !CheckStandObstacle();
                            }

                            if(!InputManager.ReadButtonToggle("Crouch", Controls.CROUCH))
                                return !CheckStandObstacle();

                            return false;
                        }

                        if(!InputManager.ReadButton(Controls.CROUCH))
                        {
                            InputManager.ResetToggledButtons();
                            return !CheckStandObstacle();
                        }

                        return false;
                    }),
                    Transition.To(PlayerStateMachine.RUN_STATE, () =>
                    {
                        bool crouchPressed = InputManager.ReadButton(Controls.CROUCH);
                        if(crouchPressed || MovementInput.y < 0 || (MovementInput.y == 0 && MovementInput.x != 0))
                            return false;

                        if(InputMagnitude > 0)
                        {
                            if (machine.PlayerFeatures.RunToggle)
                            {
                                bool runToggle = InputManager.ReadButtonToggle("Run", Controls.SPRINT);
                                bool runFlag1 = runToggle && !CheckStandObstacle();
                                return runFlag1 && (!StaminaEnabled || machine.Stamina.Value > 0f);
                            }

                            bool runPressed = InputManager.ReadButton(Controls.SPRINT);
                            bool runFlag2 = runPressed && !CheckStandObstacle();
                            return runFlag2 && (!StaminaEnabled || machine.Stamina.Value > 0f);
                        }

                        return false;
                    }),
                    Transition.To(PlayerStateMachine.SLIDING_STATE, () =>
                    {
                        if(SlopeCast(out _, out float angle))
                            return angle > machine.PlayerSliding.SlopeLimit;

                        return false;
                    }),
                    Transition.To(PlayerStateMachine.DEATH_STATE, () => IsDead)
                };
            }

            private bool CheckStandObstacle()
            {
                float height = machine.StandingState.ControllerHeight + 0.1f;
                float radius = controller.radius;
                Vector3 origin = machine.ControllerFeet;
                Ray ray = new(origin, Vector3.up);

                return Physics.SphereCast(ray, radius, out _, height, machine.SurfaceMask);
            }
        }
    }
}