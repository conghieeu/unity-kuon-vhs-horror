using UnityEngine;

public class InsideVehicle : StateMachineBehaviour
{
	private PhysicsCharacterController character;

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		character = animator.gameObject.GetComponent<PhysicsCharacterController>();
		character.insideVehicle = true;
	}

	public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
	}
}
