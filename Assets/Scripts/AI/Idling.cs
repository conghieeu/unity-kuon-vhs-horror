using StateMechanics;
using UnityEngine;

public class Idling : State<AI>
{
	private static Idling _instance;

	public float timer;

	public static Idling Instance
	{
		get
		{
			if (_instance == null)
			{
				new Idling();
			}
			return _instance;
		}
	}

	private Idling()
	{
		if (_instance == null)
		{
			_instance = this;
		}
	}

	public override void EnterState(AI master)
	{
		timer = master.idleTime / 2f;
		master.state = "idling";
		master.anim.SetBool("isWalking", value: false);
		master.anim.SetBool("isRunning", value: false);
		master.agent.isStopped = true;
		master.rotateToTarget = false;
	}

	public override void ExitState(AI master)
	{
		master.agent.isStopped = false;
		master.rotateToTarget = false;
	}

	public override void UpdateState(AI master)
	{
		if (master.rotateToTarget)
		{
			Vector3 forward = master.waypoints[master.currentWP].position - master.transform.position;
			master.transform.rotation = Quaternion.Slerp(master.transform.rotation, Quaternion.LookRotation(forward), 2f * Time.deltaTime);
		}
	}
}
