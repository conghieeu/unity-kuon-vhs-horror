using UHFPS.Scriptable;
using UHFPS.Input;
using UnityEngine;

namespace UHFPS.Runtime.States
{
    public class HidingStateAsset : PlayerStateAsset
    {
        public override FSMPlayerState InitState(PlayerStateMachine machine, PlayerStatesGroup group)
        {
            return new HidingPlayerState(machine);
        }

        public override string StateKey => PlayerStateMachine.HIDING_STATE;

        public override string Name => "Generic/Hiding";

        public class HidingPlayerState : FSMPlayerState
        {
            public HidingPlayerState(PlayerStateMachine machine) : base(machine) { }

            public bool IsFullyHidden { get; set; }
            public HideInteract HidingPlace { get; private set; }

            private bool unhidePressed = false;

            public override void OnStateEnter()
            {
                HidingPlace = (HideInteract)StateData["hideTrigger"];

                unhidePressed = false;
                IsFullyHidden = false;
                machine.Motion = Vector3.zero;
            }

            public override void OnStateExit()
            {
                HidingPlace = null;
                unhidePressed = false;
                IsFullyHidden = false;
                machine.Motion = Vector3.zero;
            }

            public override void OnStateUpdate()
            {
                HideInteract hideTrigger = (HideInteract)StateData["hideTrigger"];
                bool isHidden = hideTrigger.IsHidden;

                if(isHidden && !unhidePressed && InputManager.ReadButtonOnce("Hiding", Controls.USE))
                {
                    hideTrigger.Unhide();
                    unhidePressed = true;
                }
            }

            public override Transition[] OnGetTransitions()
            {
                return new Transition[0];
            }
        }
    }
}