using UnityEngine;

public class GetOutVehicle : StateMachineBehaviour
{
	private PhysicsCharacterController character;

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		character = animator.gameObject.GetComponent<PhysicsCharacterController>();
		character.targetCar.entryPoints[character.entryPoint].startTime = Time.time;
		character.targetCar.entryPoints[character.entryPoint].exit = true;
	}

	public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		character.physicsController.enabled = true;
		character.insideVehicle = false;
		character.transform.parent = null;
		character.targetCar.carMotor.hasDriver = false;
		character.targetCar.carMotor.parked = true;
	}
}
