using UnityEngine;

public class DisableMovement : MonoBehaviour
{
	private void Start()
	{
	}

	public void DisablePlayerMovement()
	{
		GetComponent<CharacterController>().enabled = false;
	}

	private void Update()
	{
	}
}
