using UnityEngine;
using UnityStandardAssets.Characters.FirstPerson;

public class LockPlayerWhenMenuOpen : MonoBehaviour
{
	private FirstPersonController fpsController;

	private void Start()
	{
		fpsController = Object.FindObjectOfType<FirstPersonController>();
	}

	private void Update()
	{
		if (!fpsController.enabled)
		{
			Cursor.lockState = CursorLockMode.None;
			Cursor.visible = true;
		}
	}
}
