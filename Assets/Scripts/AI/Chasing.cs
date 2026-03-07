using StateMechanics;

public class Chasing : State<AI>
{
	private static Chasing _instance;

	public static Chasing Instance
	{
		get
		{
			if (_instance == null)
			{
				new Chasing();
			}
			return _instance;
		}
	}

	private Chasing()
	{
		if (_instance == null)
		{
			_instance = this;
		}
	}

	public override void EnterState(AI master)
	{
		master.state = "chasing";
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
		master.agent.SetDestination(master.player.transform.position);
	}
}
