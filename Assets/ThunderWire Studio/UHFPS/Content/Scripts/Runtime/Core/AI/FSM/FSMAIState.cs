using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using UHFPS.Tools;
using static UnityEngine.Object;
using static UHFPS.Runtime.NPCStateMachine;

namespace UHFPS.Runtime
{
    public class FSMAIState : FSMState
    {
        public Transition[] Transitions { get; private set; }
        public StorableCollection StateData { get; set; }

        public Vector3 PlayerPosition => playerMachine.transform.position;
        public Vector3 PlayerHead => playerManager.CameraHolder.transform.position;

        protected NPCStateMachine machine;
        protected PlayerStateMachine playerMachine;
        protected PlayerHealth playerHealth;
        protected PlayerManager playerManager;
        protected Animator animator;
        protected NavMeshAgent agent;

        /// <summary>
        /// Check if the player has died.
        /// </summary>
        protected bool IsPlayerDead => machine.IsPlayerDead;

        private bool reachedDistance;
        private Vector3 lastPossibleDestination;

        public FSMAIState(NPCStateMachine machine)
        {
            this.machine = machine;
            playerMachine = machine.Player;
            playerHealth = machine.PlayerHealth;
            playerManager = machine.PlayerManager;
            animator = machine.Animator;
            agent = machine.Agent;
            Transitions = OnGetTransitions();
        }

        /// <summary>
        /// Get AI state transitions.
        /// </summary>
        public virtual Transition[] OnGetTransitions()
        {
            return new Transition[0];
        }

        /// <summary>
        /// Set destination of the agent.
        /// </summary>
        public bool SetDestination(Vector3 destination)
        {
            if (agent.SetDestination(destination))
            {
                if (agent.pathStatus != NavMeshPathStatus.PathPartial || agent.pathStatus != NavMeshPathStatus.PathInvalid)
                {
                    lastPossibleDestination = destination;
                    return true;
                }
            }

            if (lastPossibleDestination != Vector3.zero)
                agent.SetDestination(lastPossibleDestination);

            return false;
        }

        /// <summary>
        /// Is the agent's path completed?
        /// </summary>
        public bool PathCompleted()
        {
            return agent.remainingDistance <= agent.stoppingDistance && agent.velocity.sqrMagnitude <= 0.1f && !agent.pathPending;
        }

        /// <summary>
        /// Is the agent's remaining distance less than the stopping distance?
        /// </summary>
        public bool PathDistanceCompleted()
        {
            if (agent.remainingDistance <= agent.stoppingDistance && !reachedDistance) 
            {
                reachedDistance = true;
                return true;
            }
            else if (reachedDistance && agent.remainingDistance < (agent.stoppingDistance + 0.5f))
            {
                return true;
            }

            reachedDistance = false;
            return false;
        }

        /// <summary>
        /// Can AI reach the destination?
        /// </summary>
        public bool IsPathPossible(Vector3 destination)
        {
            NavMeshPath path = new();
            agent.CalculatePath(destination, path);
            return path.status != NavMeshPathStatus.PathPartial && path.status != NavMeshPathStatus.PathInvalid;
        }

        /// <summary>
        /// Does the AI see the object from the head position?
        /// </summary>
        public bool SeesObject(float distance, Vector3 position)
        {
            if (Vector3.Distance(machine.transform.position, position) <= distance)
            {
                Vector3 headPos = machine.HeadBone.position;
                return !Physics.Linecast(headPos, position, machine.SightsMask, QueryTriggerInteraction.Collide);
            }

            return false;
        }

        /// <summary>
        /// Is the object in the AI field of view?
        /// </summary>
        public bool IsObjectInSights(float FOV, Vector3 position)
        {
            Vector3 dir = position - machine.transform.position;
            return Vector3.Angle(machine.transform.forward, dir) <= FOV * 0.5;
        }

        /// <summary>
        /// Is the object in the distance?
        /// </summary>
        public bool InDistance(float distance, Vector3 position)
        {
            return DistanceOf(position) <= distance;
        }

        /// <summary>
        /// Is the player in the distance?
        /// </summary>
        public bool InPlayerDistance(float distance)
        {
            return InDistance(distance, PlayerPosition);
        }

        /// <summary>
        /// Distance from AI to target.
        /// </summary>
        public float DistanceOf(Vector3 target)
        {
            return Vector3.Distance(machine.transform.position, target);
        }

        /// <summary>
        /// Does the AI see the player from the head position using all the sights?
        /// </summary>
        public bool SeesPlayer()
        {
            bool isInvisible = (machine.NPCType == NPCTypeEnum.Enemy && playerHealth.IsInvisibleToEnemies)
                || (machine.NPCType == NPCTypeEnum.Ally && playerHealth.IsInvisibleToAllies);

            if (playerHealth.IsDead || isInvisible)
                return false;

            bool seesPlayer = SeesObject(machine.SightsDistance, PlayerHead);
            bool isPlayerInSights = IsObjectInSights(machine.SightsFOV, PlayerPosition);
            return seesPlayer && isPlayerInSights;
        }

        /// <summary>
        /// Does the AI see the player or sense the player's proximity?
        /// </summary>
        public bool SeesPlayerOrClose(float closeDistance)
        {
            return SeesPlayer() || InPlayerDistance(closeDistance);
        }

        /// <summary>
        /// Event when a player dies.
        /// </summary>
        public virtual void OnPlayerDeath() { }

        /// <summary>
        /// Check if the animation state is playing.
        /// </summary>
        public bool IsAnimation(int layerIndex, string stateName)
        {
            AnimatorStateInfo info = animator.GetCurrentAnimatorStateInfo(layerIndex);
            return info.IsName(stateName);
        }

        /// <summary>
        /// Find closest waypoints group and waypoint.
        /// </summary>
        public Pair<AIWaypointsGroup, AIWaypoint> FindClosestWaypointsGroup()
        {
            AIWaypointsGroup[] allGroups = FindObjectsByType<AIWaypointsGroup>(FindObjectsSortMode.None);
            AIWaypointsGroup closestGroup = null;
            AIWaypoint closestWaypoint = null;
            float distance = Mathf.Infinity;

            foreach (var group in allGroups)
            {
                foreach (var waypoint in group.Waypoints)
                {
                    if(waypoint == null) 
                        continue;

                    Vector3 pointPos = waypoint.transform.position;
                    float waypointDistance = DistanceOf(pointPos);

                    if(waypointDistance < distance)
                    {
                        closestGroup = group;
                        closestWaypoint = waypoint;
                    }
                }
            }

            return new(closestGroup, closestWaypoint);
        }

        /// <summary>
        /// Retrieve unreserved waypoints from a group of waypoints.
        /// </summary>
        public AIWaypoint[] GetFreeWaypoints(AIWaypointsGroup group)
        {
            if (group == null || group.Waypoints.Count == 0)
                return null;

            return group.Waypoints.Where(x => x.ReservedBy == null).ToArray();
        }
    }
}