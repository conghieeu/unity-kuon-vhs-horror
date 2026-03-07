using UnityEngine;

public class CarCamera2 : MonoBehaviour
{
	public Transform target;

	public float height = 1f;

	public float positionDamping = 3f;

	public float velocityDamping = 3f;

	public float distance = 4f;

	public LayerMask ignoreLayers = -1;

	private RaycastHit hit;

	private Vector3 prevVelocity = Vector3.zero;

	private LayerMask raycastLayers = -1;

	private Vector3 currentVelocity = Vector3.zero;

	private void Start()
	{
		raycastLayers = ~(int)ignoreLayers;
	}

	private void FixedUpdate()
	{
		currentVelocity = Vector3.Lerp(prevVelocity, target.root.GetComponent<Rigidbody>().linearVelocity, velocityDamping * Time.deltaTime);
		currentVelocity.y = 0f;
		prevVelocity = currentVelocity;
	}

	private void LateUpdate()
	{
		float t = Mathf.Clamp01(target.root.GetComponent<Rigidbody>().linearVelocity.magnitude / 70f);
		GetComponent<Camera>().fieldOfView = Mathf.Lerp(55f, 72f, t);
		float num = Mathf.Lerp(distance, distance * 0.7f, t);
		currentVelocity = currentVelocity.normalized;
		Vector3 vector = target.position + Vector3.up * height;
		Vector3 vector2 = vector - currentVelocity * num;
		vector2.y = vector.y;
		Vector3 direction = vector2 - vector;
		if (Physics.Raycast(vector, direction, out hit, num, raycastLayers))
		{
			vector2 = hit.point;
		}
		base.transform.position = vector2;
		base.transform.LookAt(vector);
	}
}
