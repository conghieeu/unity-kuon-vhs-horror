using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using StateMechanics;
using UnityEngine;
using UnityEngine.AI;

public class AI : MonoBehaviour
{
	public GameObject player;

	[ShowInInspector]
	[DisplayAsString]
	public string state = "patrolling";

	[Header("Waypoint system")]
	public float WPDist = 0.5f;

	public List<Transform> waypoints;

	[Header("Enemy Properties")]
	public float idleTime = 1f;

	public float runSpd = 3f;

	public float walkSpd = 1f;

	public float chaseTimer = 7f;

	public float attackRange = 0.5f;

	public float attackDur = 1f;

	[Header("Audio")]
	public AudioSource chase;

	public AudioSource detection;

	public AudioSource footstep;

	public AudioSource attack;

	public float walkInterval;

	public float runInterval;

	public List<AudioClip> footsteps;

	public List<AudioClip> attacks;

	private Coroutine footstepCo;

	[HideInInspector]
	public NavMeshAgent agent;

	[HideInInspector]
	public HealthManager healthManager;

	[HideInInspector]
	public FieldOfView fov;

	[HideInInspector]
	public Animator anim;

	[HideInInspector]
	public int currentWP;

	private Coroutine idleCo;

	private Coroutine chaseCo;

	private Coroutine attackCo;

	[DisplayAsString]
	public bool rotateToTarget;

	[DisplayAsString]
	public bool teleport;

	[HideInInspector]
	public Vector3 alertedPos;

	public StateMachine<AI> stateMachine { get; set; }

	private void Awake()
	{
		agent = GetComponent<NavMeshAgent>();
		fov = GetComponent<FieldOfView>();
		anim = GetComponent<Animator>();
		healthManager = player.GetComponent<HealthManager>();
	}

	private void Start()
	{
		stateMachine = new StateMachine<AI>(this);
		stateMachine.ChangeState(Patrolling.Instance);
	}

	private void Update()
	{
		stateMachine.Update();
		DisplayCurrentState();
		ChaseState();
		AttackState();
		Footsteps();
	}

	private void Footsteps()
	{
		if (state == "chasing" || state == "attacking")
		{
			chase.volume = 1f;
		}
		else
		{
			chase.volume = 0f;
		}
		if (state == "chasing" && footstepCo == null)
		{
			footstepCo = StartCoroutine(FootStepInterval(runInterval));
		}
		if (state == "patrolling" && footstepCo == null)
		{
			footstepCo = StartCoroutine(FootStepInterval(walkInterval));
		}
	}

	private IEnumerator FootStepInterval(float interval)
	{
		int index = Random.Range(0, footsteps.Count - 1);
		footstep.clip = footsteps[index];
		footstep.Play();
		yield return new WaitForSeconds(interval);
		footstepCo = null;
	}

	private void AttackState()
	{
		if (DistanceToPlayer() < attackRange && state != "attacking" && attackCo == null)
		{
			attackCo = StartCoroutine(AttackDuration());
		}
	}

	private IEnumerator AttackDuration()
	{
		stateMachine.ChangeState(Attacking.Instance);
		yield return new WaitForSeconds(attackDur);
		if (state != "chasing")
		{
			stateMachine.ChangeState(Chasing.Instance);
		}
		attackCo = null;
	}

	private void ChaseState()
	{
		if (CanSeePlayer() && attackCo == null && state != "chasing")
		{
			stateMachine.ChangeState(Chasing.Instance);
		}
		if (!(state == "chasing"))
		{
			return;
		}
		if (DistanceToPlayer() > 5f)
		{
			if (chaseCo == null)
			{
				chaseCo = StartCoroutine(LoseTimer());
			}
		}
		else if (chaseCo != null)
		{
			StopCoroutine(chaseCo);
			chaseCo = null;
		}
	}

	private IEnumerator LoseTimer()
	{
		yield return new WaitForSeconds(chaseTimer);
		idleCo = StartCoroutine(IdleDelay());
		teleport = true;
		chaseCo = null;
	}

	public void WaypointIdleDelay()
	{
		idleCo = StartCoroutine(IdleDelay());
	}

	private IEnumerator IdleDelay()
	{
		stateMachine.ChangeState(Idling.Instance);
		yield return new WaitForSeconds(idleTime);
		idleCo = StartCoroutine(RotateBeforePatrolling());
	}

	private IEnumerator RotateBeforePatrolling()
	{
		rotateToTarget = true;
		yield return new WaitForSeconds(1.5f);
		stateMachine.ChangeState(Patrolling.Instance);
	}

	private void DisplayCurrentState()
	{
		base.gameObject.name = state;
	}

	private float DistanceToPlayer()
	{
		return Vector3.Distance(base.transform.position, player.transform.position);
	}

	private bool CanSeePlayer()
	{
		return fov.canSeePlayer;
	}
}
