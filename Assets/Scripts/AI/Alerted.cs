using StateMechanics;
using UnityEngine;

public class Alerted : State<AI>
{
	private static Alerted _instance;

	public static Alerted Instance
	{
		get
		{
			if (_instance == null)
			{
				new Alerted();
			}
			return _instance;
		}
	}

	private Alerted()
	{
		if (_instance == null)
		{
			_instance = this;
		}
	}

	public override void EnterState(AI master)
	{
		master.state = "alerted";
		master.anim.SetBool("isWalking", value: false);
		master.anim.SetBool("isRunning", value: true);
		master.agent.speed = master.runSpd;
		master.detection.Play();
	}

	public override void ExitState(AI master)
	{
	}

	public override void UpdateState(AI master)
	{
		master.agent.SetDestination(master.alertedPos);
		if (Vector3.Distance(master.transform.position, master.alertedPos) < 1f)
		{
			master.WaypointIdleDelay();
		}
	}
}
