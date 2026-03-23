using UnityEngine;

namespace UHFPS.Runtime
{
    public class WaitForAnimatorClip : CustomYieldInstruction
    {
        const string BaseLayer = "Base Layer";

        private readonly Animator animator;
        private readonly float timeOffset;
        private readonly int stateHash;

        private bool isStateEntered;
        private float stateWaitTime;
        private float timeWaited;

        public WaitForAnimatorClip(Animator animator, string state, float timeOffset = 0, bool normalized = false)
        {
            this.animator = animator;
            this.timeOffset = timeOffset;
            stateHash = Animator.StringToHash(BaseLayer + "." + state);
        }

        public override bool keepWaiting
        {
            get
            {
                AnimatorStateInfo info = animator.GetCurrentAnimatorStateInfo(0);

                if (info.fullPathHash == stateHash && !isStateEntered)
                {
                    float stateLength = info.length;
                    stateWaitTime = stateLength - timeOffset;
                    isStateEntered = true;
                }
                else if (isStateEntered)
                {
                    if (timeWaited < stateWaitTime) 
                        timeWaited += Time.deltaTime;
                    else return false;
                }

                return true;
            }
        }
    }
}