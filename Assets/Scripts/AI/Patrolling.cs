using StateMechanics;
using UnityEngine;

public class Patrolling : State<AI>
{
	private static Patrolling _instance;

	public static Patrolling Instance
	{
		get
		{
			if (_instance == null)
			{
				new Patrolling();
			}
			return _instance;
		}
	}

	private Patrolling()
	{
		if (_instance == null)
		{
			_instance = this;
		}
	}

	public override void EnterState(AI master)
	{
		master.state = "patrolling";
		master.anim.SetBool("isWalking", value: true);
		master.anim.SetBool("isRunning", value: false);
		master.agent.speed = master.walkSpd;
	}

	public override void ExitState(AI master)
	{
	}

	public override void UpdateState(AI master)
	{
		if (Vector3.Distance(master.waypoints[master.currentWP].position, master.transform.position) < master.WPDist)
		{
			int currentWP = Random.Range(0, master.waypoints.Count);
			master.currentWP = currentWP;
			master.WaypointIdleDelay();
		}
		master.agent.SetDestination(master.waypoints[master.currentWP].position);
	}
}
