using UnityEngine;

public class RotateAround : MonoBehaviour
{
	public Transform rot_center;

	private void Start()
	{
	}

	private void Update()
	{
		base.transform.RotateAround(rot_center.position, Vector3.up, 0.25f);
	}
}
