using UnityEngine;
using UnityEngine.Events;

public class KillPlayer : MonoBehaviour
{
	public UnityEvent killEvent;

	public bool trigger = true;

	private void OnTriggerEnter(Collider other)
	{
		if (other.tag == "Player" && trigger)
		{
			killEvent.Invoke();
		}
	}

	public void Kill()
	{
		killEvent.Invoke();
	}
}
