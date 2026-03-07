using UnityEngine;

public class CarCamera : MonoBehaviour
{
	public float cameraHeight;

	public float cameraDistance;

	public Transform cameraTarget;

	public float targetOffset;

	private void Start()
	{
	}

	private void LateUpdate()
	{
		Vector3 position = cameraTarget.position - cameraTarget.forward * cameraDistance;
		position.y = cameraTarget.position.y;
		position.y += cameraHeight;
		base.transform.position = position;
		base.transform.LookAt(cameraTarget.position + cameraTarget.forward * targetOffset);
	}
}
