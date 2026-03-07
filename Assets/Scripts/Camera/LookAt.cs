using UnityEngine;

public class LookAt : MonoBehaviour
{
	public Transform target;

	private void Start()
	{
	}

	private void Update()
	{
		base.transform.LookAt(target);
	}
}
