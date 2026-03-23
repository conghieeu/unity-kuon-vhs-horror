using UnityEngine;
using UHFPS.Scriptable;
using UHFPS.Input;

namespace UHFPS.Runtime.States
{
    public class SmoothJumpState : SmoothStateAsset
    {
        public override FSMPlayerState InitState(PlayerStateMachine machine, PlayerStatesGroup group)
        {
            return new JumpPlayerState(machine, group);
        }

        public override string StateKey => PlayerStateMachine.JUMP_STATE;

        public override string Name => "Smooth/Jump";

        public class JumpPlayerState : SmoothPlayerState
        {
            public JumpPlayerState(PlayerStateMachine machine, PlayerStatesGroup group) : base(machine, group)
            {
            }

            public override void OnStateEnter()
            {
                movementSpeed = machine.Motion.magnitude;
                machine.Motion.y = Mathf.Sqrt(machine.PlayerBasicSettings.JumpHeight * -2f * GravityForce());

                if (machine.PlayerFeatures.EnableStamina)
                {
                    float stamina = machine.Stamina.Value;
                    stamina -= machine.PlayerStamina.JumpExhaustion * 0.01f;
                    machine.Stamina.OnNext(stamina);
                }
            }

            public override Transition[] OnGetTransitions()
            {
                return new Transition[]
                {
                    Transition.To(PlayerStateMachine.IDLE_STATE, () => IsGrounded),
                    Transition.To(PlayerStateMachine.CROUCH_STATE, () =>
                    {
                        if(IsGrounded)
                        {
                            if (machine.PlayerFeatures.CrouchToggle)
                            {
                                return InputManager.ReadButtonToggle("Crouch", Controls.CROUCH);
                            }

                            return InputManager.ReadButton(Controls.CROUCH);
                        }

                        return false;
                    }),
                    Transition.To(PlayerStateMachine.DEATH_STATE, () => IsDead)
                };
            }
        }
    }
}