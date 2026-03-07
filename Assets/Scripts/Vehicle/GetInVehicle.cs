using UnityEngine;

public class GetInVehicle : StateMachineBehaviour
{
	private PhysicsCharacterController character;

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		character = animator.gameObject.GetComponent<PhysicsCharacterController>();
		character.transform.position = character.targetCar.entryPoints[character.entryPoint].entryPoint.position;
		character.transform.rotation = character.targetCar.entryPoints[character.entryPoint].entryPoint.rotation;
		animator.SetBool("GetIn", value: true);
	}

	public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		animator.SetBool("GetIn", value: true);
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
	}
}
