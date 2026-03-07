using UnityEngine;
using UnityEngine.AI;

public class SecretChase_teleport : MonoBehaviour
{
	public NavMeshAgent agent;

	private void OnEnable()
	{
		agent.Warp(base.transform.position);
	}
}
