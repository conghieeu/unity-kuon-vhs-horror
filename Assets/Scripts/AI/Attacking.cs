using StateMechanics;
using UnityEngine;

public class Attacking : State<AI>
{
	private static Attacking _instance;

	public static Attacking Instance
	{
		get
		{
			if (_instance == null)
			{
				new Attacking();
			}
			return _instance;
		}
	}

	private Attacking()
	{
		if (_instance == null)
		{
			_instance = this;
		}
	}

	public override void EnterState(AI master)
	{
		master.state = "attacking";
		master.anim.SetBool("isWalking", value: true);
		master.anim.SetBool("isRunning", value: false);
		master.anim.SetBool("isAttacking", value: true);
		master.agent.speed = master.walkSpd;
		master.attack.clip = master.attacks[Random.Range(0, master.attacks.Count - 1)];
		master.attack.Play();
		master.healthManager.ApplyDamage(30f);
	}

	public override void ExitState(AI master)
	{
		master.anim.SetBool("isAttacking", value: false);
	}

	public override void UpdateState(AI master)
	{
	}
}
