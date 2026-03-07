using UnityEngine;

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
		if (Input.GetKeyDown(KeyCode.F))
		{
			lite.enabled = !lite.enabled;
		}
	}
}
