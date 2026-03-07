using UnityEngine;

public class ClockPuzzle_Hands : MonoBehaviour
{
	public GameObject hand;

	public bool isIn;

	private void OnTriggerEnter(Collider other)
	{
		if (other.gameObject == hand)
		{
			isIn = true;
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (other.gameObject == hand)
		{
			isIn = false;
		}
	}
}
