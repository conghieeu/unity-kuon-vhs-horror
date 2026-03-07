using UnityEngine;

public class PlayerTracker : MonoBehaviour
{
	public CamTarget camTarget;

	public PhysicsCharacterController playerController;

	private void Start()
	{
	}

	private void Update()
	{
		if (playerController.insideVehicle)
		{
			camTarget.target = playerController.targetCar.transform;
		}
		else
		{
			camTarget.target = playerController.transform;
		}
	}
}
