using UnityEngine;
using UHFPS.Scriptable;
using UHFPS.Input;

namespace UHFPS.Runtime.States
{
    public class IdleStateAsset : StrafeStateAsset
    {
        public override FSMPlayerState InitState(PlayerStateMachine machine, PlayerStatesGroup group)
        {
            return new IdlePlayerState(machine, group);
        }

        public override string StateKey => PlayerStateMachine.IDLE_STATE;

        public override string Name => "Strafe/Idle";

        public class IdlePlayerState : StrafePlayerState
        {
            public override bool CanTransitionWhenDisabled => true;

            public IdlePlayerState(PlayerStateMachine machine, PlayerStatesGroup group) : base(machine, group)
            {
            }

            public override void OnStateEnter()
            {
                movementSpeed = 0f;
                controllerState = machine.StandingState;
                InputManager.ResetToggledButton("Run", Controls.SPRINT);
            }

            public override Transition[] OnGetTransitions()
            {
                return new Transition[]
                {
                    Transition.To(PlayerStateMachine.WALK_STATE, () =>
                    {
                        bool runPressed = InputManager.ReadButton(Controls.SPRINT);
                        return !runPressed && InputMagnitude > 0;
                    }),
                    Transition.To(PlayerStateMachine.RUN_STATE, () =>
                    {
                        if(InputMagnitude > 0)
                        {
                            if (machine.PlayerFeatures.RunToggle)
                            {
                                bool runToggle = InputManager.ReadButtonToggle("Run", Controls.SPRINT);
                                return runToggle && (!StaminaEnabled || machine.Stamina.Value > 0f);
                            }

                            bool runPressed = InputManager.ReadButton(Controls.SPRINT);
                            return runPressed && (!StaminaEnabled || machine.Stamina.Value > 0f);
                        }

                        return false;
                    }),
                    Transition.To(PlayerStateMachine.CROUCH_STATE, () =>
                    {
                        if (machine.PlayerFeatures.CrouchToggle)
                        {
                            return InputManager.ReadButtonToggle("Crouch", Controls.CROUCH);
                        }

                        return InputManager.ReadButton(Controls.CROUCH);
                    }),
                    Transition.To(PlayerStateMachine.JUMP_STATE, () =>
                    {
                        bool jumpPressed = InputManager.ReadButtonOnce("Jump", Controls.JUMP);
                        return jumpPressed && (!StaminaEnabled || machine.Stamina.Value > 0f);
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
        }
    }
}