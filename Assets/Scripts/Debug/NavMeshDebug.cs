using UnityEngine;
using UnityEngine.AI;

public class NavMeshDebug : MonoBehaviour
{
	public NavMeshAgent agent;

	private void Update()
	{
		if (agent.enabled && agent.gameObject.activeSelf)
		{
			base.transform.position = agent.destination;
		}
	}
}
