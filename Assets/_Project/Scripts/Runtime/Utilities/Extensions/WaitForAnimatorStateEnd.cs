using UnityEngine;

namespace UHFPS.Runtime
{
    public class WaitForAnimatorStateEnd : CustomYieldInstruction
    {
        private readonly Animator animator;
        private readonly string stateName;
        private readonly float timeOffset;
        private readonly int layer;

        public override bool keepWaiting
        {
            get
            {
                if (!animator) return false;

                AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(layer);
                return !stateInfo.IsName(stateName) || stateInfo.normalizedTime < (1.0f - timeOffset);
            }
        }

        public WaitForAnimatorStateEnd(Animator animator, string stateName, float timeOffset = 0f, int layer = 0)
        {
            this.animator = animator;
            this.stateName = stateName;
            this.timeOffset = timeOffset;
            this.layer = layer;
        }
    }
}