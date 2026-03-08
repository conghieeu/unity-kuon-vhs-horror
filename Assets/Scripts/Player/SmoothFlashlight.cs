using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class SmoothFlashlight : MonoBehaviour
{
	public float speed = 3f;

	private Vector3 vectOffset;

	private GameObject goFollow;

	private Light lite;

	private void Start()
	{
		goFollow = Camera.main.gameObject;
		base.transform.parent.position = goFollow.transform.position;
		vectOffset = base.transform.position - goFollow.transform.position;
		lite = GetComponent<Light>();
	}

	private void Update()
	{
		base.transform.position = goFollow.transform.position + vectOffset;
		base.transform.rotation = Quaternion.Slerp(base.transform.rotation, goFollow.transform.rotation, speed * Time.deltaTime);

		bool flashlightToggle = false;
#if ENABLE_INPUT_SYSTEM
		if (Keyboard.current != null)
			flashlightToggle = Keyboard.current.fKey.wasPressedThisFrame;
#else
		flashlightToggle = Input.GetKeyDown(KeyCode.F);
#endif

		if (flashlightToggle)
		{
			lite.enabled = !lite.enabled;
		}
	}
}
