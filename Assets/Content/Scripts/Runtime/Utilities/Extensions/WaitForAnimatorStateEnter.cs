using UnityEngine;

namespace UHFPS.Runtime
{
    public class WaitForAnimatorStateEnter : CustomYieldInstruction
    {
        private readonly Animator animator;
        private readonly string state;

        public WaitForAnimatorStateEnter(Animator animator, string state)
        {
            this.animator = animator;
            this.state = state;
        }

        public override bool keepWaiting
        {
            get
            {
                AnimatorStateInfo info = animator.GetCurrentAnimatorStateInfo(0);
                return !info.IsName(state);
            }
        }
    }
}