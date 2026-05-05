using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using ThunderWire.Attributes;
using UHFPS.Tools;
using UHFPS.Scriptable;

namespace UHFPS.Runtime.States
{
    [Summary("Trạng thái AI: Đi tuần tra (Patrol) qua các điểm (Waypoint) được chỉ định.")]
    public class ZombiePatrolState : AIStateAsset
    {
        public enum WaypointPatrolEnum { InOrder, Random }
        public enum PatrolTypeEnum { None, WaitTime }

        [Tooltip("Chế độ đi tuần (Theo thứ tự - InOrder hoặc Ngẫu nhiên - Random).")]
        public WaypointPatrolEnum Patrol = WaypointPatrolEnum.InOrder;

        [Tooltip("Kiểu đi tuần (Không dừng lại - None hoặc Dừng lại chờ tại điểm - WaitTime).")]
        public PatrolTypeEnum PatrolType = PatrolTypeEnum.None;

        [Header("Settings")]
        [Tooltip("Thời gian chờ tại mỗi điểm tuần tra (Nếu PatrolType = WaitTime).")]
        public float PatrolTime = 3f;

        [Tooltip("Tốc độ đi bộ khi tuần tra.")]
        public float WalkSpeed = 0.5f;

        [Tooltip("Khoảng cách dừng lại (tới điểm Waypoint) khi tuần tra.")]
        public float PatrolStoppingDistance = 1f;

        [Tooltip("Khoảng cách siêu gần để AI tự động phát hiện người chơi.")]
        public float VeryClosePlayerDetection = 1f;

        public override FSMAIState InitState(NPCStateMachine machine, AIStatesGroup group)
        {
            return new PatrolState(machine, group, this);
        }

        public override string StateKey => "Patrol";
        public override string Name => "Zombie/Patrol";

        public class PatrolState : FSMAIState
        {
            private readonly ZombieStateGroup Group;
            private readonly ZombiePatrolState State;

            private AIWaypointsGroup waypointsGroup;
            private AIWaypoint currWaypoint;
            private AIWaypoint prevWaypoint;

            private float waitTime;
            private bool isWaypointSet;
            private bool isPatrolPending;

            public PatrolState(NPCStateMachine machine, AIStatesGroup group, AIStateAsset state) : base(machine) 
            {
                Group = (ZombieStateGroup)group;
                State = (ZombiePatrolState)state;
            }

            public override Transition[] OnGetTransitions()
            {
                return new Transition[]
                {
                    Transition.To<ZombieChaseState>(() => !playerMachine.IsCurrent(PlayerStateMachine.HIDING_STATE) 
                    && (SeesPlayer() || InDistance(State.VeryClosePlayerDetection, PlayerPosition)) && !IsPlayerDead)
                };
            }

            public override void OnStateEnter()
            {
                var closestWaypointsGroup = FindClosestWaypointsGroup();
                waypointsGroup = closestWaypointsGroup.Key;

                agent.speed = State.WalkSpeed;
                agent.stoppingDistance = State.PatrolStoppingDistance;
                Group.ResetAnimatorPrameters(animator);
            }

            public override void OnStateExit()
            {
                waitTime = 0f;
                isWaypointSet = false;
                isPatrolPending = false;

                if (currWaypoint != null)
                    currWaypoint.ReservedBy = null;
            }

            public override void OnStateUpdate()
            {
                if (waypointsGroup == null)
                    return;

                if (!isWaypointSet)
                {
                    SetNextWaypoint();
                    if(currWaypoint != null)
                    {
                        Vector3 waypointPos = currWaypoint.transform.position;
                        agent.isStopped = false;
                        agent.SetDestination(waypointPos);
                        animator.SetBool(Group.WalkParameter, true);
                        currWaypoint.ReservedBy = machine.gameObject;
                    }

                    isWaypointSet = true;
                }
                else
                {
                    if (!PathCompleted() && !isPatrolPending)
                        return;

                    if (State.PatrolType == PatrolTypeEnum.None)
                    {
                        isWaypointSet = false;
                        Group.ResetAnimatorPrameters(animator);
                    }
                    else if (State.PatrolType == PatrolTypeEnum.WaitTime)
                    {
                        if (!isPatrolPending)
                        {
                            Group.ResetAnimatorPrameters(animator);
                            animator.SetBool(Group.PatrolParameter, true);
                            agent.velocity = Vector3.zero;
                            agent.isStopped = true;
                            isPatrolPending = true;
                        }
                        else
                        {
                            waitTime += Time.deltaTime;

                            if (waitTime > State.PatrolTime)
                            {
                                waitTime = 0f;
                                isPatrolPending = false;
                                isWaypointSet = false;
                                Group.ResetAnimatorPrameters(animator);
                            }
                        }
                    }
                }
            }

            private void SetNextWaypoint()
            {
                prevWaypoint = currWaypoint;
                if(prevWaypoint != null) 
                    prevWaypoint.ReservedBy = null;

                var freeWaypoints = GetFreeWaypoints(waypointsGroup);
                if(State.Patrol == WaypointPatrolEnum.InOrder)
                {
                    if (currWaypoint == null) currWaypoint = freeWaypoints[0];
                    else
                    {
                        int currIndex = Array.IndexOf(freeWaypoints, currWaypoint);
                        int nextIndex = currIndex + 1 >= freeWaypoints.Length ? 0 : currIndex + 1;
                        currWaypoint = freeWaypoints[nextIndex];
                    }
                }
                else if(State.Patrol == WaypointPatrolEnum.Random)
                {
                    freeWaypoints = freeWaypoints.Except(new[] { prevWaypoint }).ToArray();
                    currWaypoint = freeWaypoints.Random();
                }
            }
        }
    }
}