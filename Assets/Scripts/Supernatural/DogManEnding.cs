using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

public class DogManEnding : MonoBehaviour
{
	public Transform player;

	public UnityEvent EndingEvent;

	private NavMeshAgent agent;

	private Animator anim;

	private float dist;

	private bool complete;

	private void Start()
	{
		agent = GetComponent<NavMeshAgent>();
		anim = GetComponent<Animator>();
	}

	private void Update()
	{
		dist = Vector3.Distance(player.position, base.transform.position);
		agent.SetDestination(player.position);
		if (!complete)
		{
			if (dist < 2f)
			{
				StartCoroutine(delay());
				complete = true;
			}
			else
			{
				anim.SetBool("isRunning", value: true);
			}
		}
	}

	private IEnumerator delay()
	{
		anim.SetBool("isAttacking", value: true);
		anim.SetBool("isRunning", value: false);
		yield return new WaitForSeconds(0.2f);
		EndingEvent.Invoke();
		base.gameObject.SetActive(value: false);
	}
}
