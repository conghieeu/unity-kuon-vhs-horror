using UnityEngine;

public class StoolMovePlayer : MonoBehaviour
{
	public Transform moveToPoint;

	public Transform player;

	private void Update()
	{
		if (InteractManager.Instance.InteractingObject == base.gameObject && Input.GetKeyDown(InteractManager.Instance.interactKey))
		{
			player.position = moveToPoint.position;
		}
	}
}
