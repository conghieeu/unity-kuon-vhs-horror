using UnityEngine;

public class GhostFollowPlayer : MonoBehaviour
{
	private Transform player;

	public float speed;

	private void Start()
	{
		player = Camera.main.transform;
	}

	private void Update()
	{
		base.transform.LookAt(player.transform);
		base.transform.Translate(Vector3.forward * speed);
	}
}
