using UnityEngine;
using UHFPS.Scriptable;
using UHFPS.Tools;
using ThunderWire.Attributes;

namespace UHFPS.Runtime.States
{
    [Summary("Trạng thái AI: Đuổi theo (Chase) và Tấn công (Attack) người chơi.")]
    public class ZombieChaseState : AIStateAsset
    {
        [Tooltip("Tốc độ chạy khi đuổi theo người chơi.")]
        public float RunSpeed = 3f;

        [Tooltip("Khoảng cách dừng lại (so với người chơi) khi đuổi theo.")]
        public float ChaseStoppingDistance = 1.5f;

        [Header("Chase")]
        [Tooltip("Thời gian (giây) chờ trước khi chuyển về trạng thái đi tuần (Patrol) sau khi mất dấu người chơi.")]
        public float LostPlayerPatrolTime = 5f;

        [Tooltip("Thời gian (giây) tiếp tục đi tới vị trí cuối cùng nhìn thấy người chơi sau khi mất dấu.")]
        public float LostPlayerPredictTime = 1f;

        [Tooltip("Khoảng cách siêu gần để AI tự động phát hiện người chơi (Dù không nằm trong tầm nhìn).")]
        public float VeryClosePlayerDetection = 1.5f;

        [Header("Attack")]
        [Tooltip("Góc nhìn cho phép AI thực hiện đòn tấn công.")]
        public float AttackFOV = 30f;

        [Tooltip("Khoảng cách tối đa để AI có thể tấn công người chơi.")]
        public float AttackDistance = 2f;

        public override FSMAIState InitState(NPCStateMachine machine, AIStatesGroup group)
        {
            return new ChaseState(machine, group, this);
        }

        public override string StateKey => "Chase";
        public override string Name => "Zombie/Chase";

        public class ChaseState : FSMAIState
        {
            private readonly ZombieStateGroup Group;
            private readonly ZombieChaseState State;

            private bool isChaseStarted;
            private bool isPatrolPending;
            private bool resetParameters;

            private float waitTime;
            private float predictTime;
            private bool playerDied;

            public ChaseState(NPCStateMachine machine, AIStatesGroup group, AIStateAsset state) : base(machine)
            {
                Group = (ZombieStateGroup)group;
                State = (ZombieChaseState)state;

                machine.CatchMessage("Attack", () => AttackPlayer());
            }

            public override Transition[] OnGetTransitions()
            {
                return new Transition[]
                {
                    Transition.To<ZombiePatrolState>(() => waitTime > State.LostPlayerPatrolTime || playerDied),
                    Transition.To<ZombiePlayerHideState>(() => playerMachine.IsCurrent(PlayerStateMachine.HIDING_STATE))
                };
            }

            public override void OnStateEnter()
            {
                Group.ResetAnimatorPrameters(animator);
                agent.speed = State.RunSpeed;
                agent.stoppingDistance = State.ChaseStoppingDistance;
                machine.RotateAgentManually = true;
                isChaseStarted = true;
            }

            public override void OnStateExit()
            {
                machine.RotateAgentManually = false;
                isChaseStarted = false;
                isPatrolPending = false;
                resetParameters = false;
                waitTime = 0f;
                predictTime = 0f;
            }

            public override void OnPlayerDeath()
            {
                animator.ResetTrigger(Group.AttackTrigger);
                playerDied = true;
            }

            public override void OnStateUpdate()
            {
                if (PlayerInSights())
                {
                    if (!resetParameters)
                    {
                        Group.ResetAnimatorPrameters(animator);
                        animator.SetBool(Group.RunParameter, true);
                        resetParameters = true;
                    }

                    Chasing();
                    SetDestination(PlayerPosition);
                    predictTime = State.LostPlayerPredictTime;

                    if (PathDistanceCompleted())
                    {
                        agent.isStopped = true;
                        agent.velocity = Vector3.zero;
                        animator.SetBool(Group.RunParameter, false);
                        animator.SetBool(Group.IdleParameter, true);
                    }
                    else
                    {
                        agent.isStopped = false;
                        animator.SetBool(Group.RunParameter, true);
                        animator.SetBool(Group.IdleParameter, false);
                        animator.ResetTrigger(Group.AttackTrigger);
                    }

                    isPatrolPending = false;
                    isChaseStarted = true;
                    waitTime = 0f;
                }
                else if(predictTime > 0f)
                {
                    SetDestination(PlayerPosition);
                    predictTime -= Time.deltaTime;
                }
                else
                {
                    if (!PathCompleted())
                        return;

                    if (!isPatrolPending)
                    {
                        Group.ResetAnimatorPrameters(animator);
                        animator.SetBool(Group.PatrolParameter, true);
                        agent.velocity = Vector3.zero;
                        agent.isStopped = true;

                        resetParameters = false;
                        isPatrolPending = true;
                        isChaseStarted = false;
                    }
                    else
                    {
                        waitTime += Time.deltaTime;
                    }
                }
            }

            private void Chasing()
            {
                bool isAttacking = IsAnimation(1, Group.AttackState);
                if(InPlayerDistance(State.AttackDistance) && IsObjectInSights(State.AttackFOV, PlayerPosition) && !isAttacking && !playerHealth.IsDead)
                {
                    animator.SetTrigger(Group.AttackTrigger);
                }
            }

            private bool PlayerInSights()
            {
                if (playerHealth.IsDead)
                    return false;

                if (!isChaseStarted || isPatrolPending)
                    return SeesPlayerOrClose(State.VeryClosePlayerDetection);

                return SeesObject(machine.SightsDistance, PlayerHead);
            }

            private void AttackPlayer()
            {
                if (!InPlayerDistance(State.AttackDistance))
                    return;

                int damage = Group.DamageRange.Random();
                playerHealth.OnApplyDamage(damage, machine.transform);
            }
        }
    }
}