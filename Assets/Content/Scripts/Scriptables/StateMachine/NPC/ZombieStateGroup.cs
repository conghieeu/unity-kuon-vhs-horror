using UnityEngine;
using UHFPS.Runtime;

namespace UHFPS.Scriptable
{
    [CreateAssetMenu(fileName = "ZombieStateGroup", menuName = "UHFPS/AI/Zombie State Group")]
    public class ZombieStateGroup : AIStatesGroup
    {
        public string IdleParameter = "Idle";
        public string WalkParameter = "Walk";
        public string RunParameter = "Run";
        public string PatrolParameter = "Patrol";
        public string AttackTrigger = "Attack";

        [Header("States")]
        public string AttackState = "Attack 01";

        [Header("Player Damage")]
        public MinMaxInt DamageRange;

        public void ResetAnimatorPrameters(Animator animator)
        {
            animator.SetBool(IdleParameter, false);
            animator.SetBool(WalkParameter, false);
            animator.SetBool(RunParameter, false);
            animator.SetBool(PatrolParameter, false);
        }
    }
}
