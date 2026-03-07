using UnityEngine;

public class SparksExecutioner : MonoBehaviour
{
	public float Lifetime = 1f;

	private void Start()
	{
		Invoke("Kill", Lifetime);
	}

	private void Kill()
	{
		Object.Destroy(base.gameObject);
	}
}
