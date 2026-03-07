using UnityEngine;
using StarterAssets;

public class LockPlayerWhenMenuOpen : MonoBehaviour
{
	private FirstPersonController fpsController;

	private void Start()
	{
		fpsController = FindFirstObjectByType<FirstPersonController>();
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
