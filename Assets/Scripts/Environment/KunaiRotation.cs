using UnityEngine;

public class KunaiRotation : MonoBehaviour
{
	public float xSpeed;

	public float ySpeed;

	public float zSpeed;

	private void Start()
	{
	}

	private void Update()
	{
		base.transform.Rotate(xSpeed * Time.deltaTime, ySpeed * Time.deltaTime, zSpeed * Time.deltaTime);
	}
}
