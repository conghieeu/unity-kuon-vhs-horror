using UnityEngine;
using UHFPS.Scriptable;
using static UHFPS.Runtime.States.HidingStateAsset;

namespace UHFPS.Runtime.States
{
    public class ZombiePlayerHideState : AIStateAsset
    {
        public int SeePlayerDamage = 10;
        public float HidingCloseDistance = 2f;
        public float PatrolTime = 3f;

        public override FSMAIState InitState(NPCStateMachine machine, AIStatesGroup group)
        {
            return new PlayerHideState(machine, group, this);
        }

        public override string StateKey => "PlayerHide";
        public override string Name => "Zombie/PlayerHide";

        public class PlayerHideState : FSMAIState
        {
            private readonly ZombieStateGroup Group;
            private readonly ZombiePlayerHideState State;

            private HidingPlayerState PlayerHide;
            private HideInteract HidingPlace;

            private bool PlayerFullyHidden => PlayerHide.IsFullyHidden;
            private bool IsPlayerHiding => playerMachine.IsCurrent(PlayerStateMachine.HIDING_STATE);

            private bool hidingPlaceSeen;
            private bool attackBeforeHide;
            private bool chasePlayer;
            private bool unhidePlayer;

            private float patrolTime;

            public PlayerHideState(NPCStateMachine machine, AIStatesGroup group, AIStateAsset state) : base(machine)
            {
                Group = (ZombieStateGroup)group;
                State = (ZombiePlayerHideState)state;

                machine.CatchMessage("Attack", () => AttackPlayer());
            }

            public override Transition[] OnGetTransitions()
            {
                return new Transition[]
                {
                    Transition.To<ZombiePatrolState>(() => patrolTime > State.PatrolTime),
                    Transition.To<ZombieChaseState>(() => chasePlayer && !IsPlayerHiding)
                };
            }

            public override void OnStateEnter()
            {
                PlayerHide ??= (HidingPlayerState)playerMachine.GetState<HidingStateAsset>();
                if (PlayerHide != null) HidingPlace = PlayerHide.HidingPlace;

                if (SeesPlayer())
                {
                    SetDestination(PlayerPosition);
                    hidingPlaceSeen = true;
                    chasePlayer = true;
                }
            }

            public override void OnStateExit()
            {
                HidingPlace = null;
                hidingPlaceSeen = false;
                attackBeforeHide = false;
                chasePlayer = false;
                unhidePlayer = false;
                patrolTime = 0f;
            }

            public override void OnStateUpdate()
            {
                if(chasePlayer || hidingPlaceSeen)
                {
                    // zombie seen the hiding place

                    if (!InPlayerDistance(State.HidingCloseDistance))
                    {
                        SetDestination(PlayerPosition);
                    }
                    else if(!PlayerFullyHidden && PathDistanceCompleted())
                    {
                        if (!attackBeforeHide && playerHealth.EntityHealth > Group.DamageRange.RealMax)
                        {
                            animator.SetTrigger(Group.AttackTrigger);
                            attackBeforeHide = true;
                        }
                    }
                    else if(PlayerFullyHidden && !unhidePlayer)
                    {
                        HidingPlace.Unhide(true);
                        chasePlayer = true;
                        unhidePlayer = true;
                    }
                }
                else if(SeesPlayer() && !PlayerFullyHidden)
                {
                    // zombie didn't seen the hiding place, but player was not fully hidden

                    hidingPlaceSeen = true;
                    chasePlayer = true;
                }
                else if (PlayerFullyHidden && !hidingPlaceSeen)
                {
                    // zombie didn't seen the hiding place and player is fully hidden

                    patrolTime += Time.deltaTime;
                }

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
            }

            private void AttackPlayer()
            {
                playerHealth.OnApplyDamage(State.SeePlayerDamage, machine.transform);
            }
        }
    }
}