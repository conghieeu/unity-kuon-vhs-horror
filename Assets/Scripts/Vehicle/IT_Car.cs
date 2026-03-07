using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

public class IT_Car : MonoBehaviour
{
	public bool dog;

	public Animator dogAnim;

	public string IdleAnim = "Idle";

	public string WalkAnim = "Walk";

	public AudioSource footstepAS;

	public List<AudioClip> sound;

	[Space]
	public float tireSpd = 0.5f;

	public Transform tire1;

	public Transform tire2;

	public Transform tire3;

	public Transform tire4;

	private NavMeshAgent agent;

	public float wpDist = 0.5f;

	public List<IT_Car_Waypoints> waypoints;

	public UnityEvent FinishEvent;

	public UnityEvent KillMode;

	[Space]
	public bool moving = true;

	private bool reachedEnd;

	private Coroutine stopCo;

	private Coroutine WalkCo;

	private Coroutine killCo;

	private int currentWPNum;

	private void Start()
	{
		agent = GetComponent<NavMeshAgent>();
	}

	public void StopThenKill()
	{
		StartCoroutine(Kill());
	}

	private IEnumerator Kill()
	{
		if (stopCo != null)
		{
			StopCoroutine(stopCo);
			stopCo = null;
		}
		moving = false;
		yield return new WaitForSeconds(3f);
		KillMode.Invoke();
		agent.speed = 7f;
		moving = true;
	}

	private IEnumerator Footstep()
	{
		footstepAS.clip = sound[Random.Range(0, sound.Count - 1)];
		footstepAS.pitch = Random.Range(0.7f, 1f);
		footstepAS.Play();
		yield return new WaitForSeconds(0.5f);
		WalkCo = null;
	}

	private void Update()
	{
		if (dog)
		{
			if (moving)
			{
				if (WalkCo == null)
				{
					WalkCo = StartCoroutine(Footstep());
				}
			}
			else if (WalkCo != null)
			{
				StopCoroutine(WalkCo);
				WalkCo = null;
			}
		}
		if (!reachedEnd && Vector3.Distance(base.transform.position, waypoints[currentWPNum].waypoint.position) < wpDist)
		{
			if (currentWPNum == waypoints.Count - 1)
			{
				ReachedEnd();
			}
			else
			{
				stopCo = StartCoroutine(CarStopTime());
				currentWPNum++;
			}
		}
		if (moving)
		{
			if (tire1 != null)
			{
				tire1.Rotate(Vector3.right * tireSpd, Space.Self);
				tire2.Rotate(Vector3.right * tireSpd, Space.Self);
				tire3.Rotate(Vector3.right * tireSpd, Space.Self);
				tire4.Rotate(Vector3.right * tireSpd, Space.Self);
			}
			agent.SetDestination(waypoints[currentWPNum].waypoint.position);
		}
		else
		{
			agent.ResetPath();
		}
	}

	private IEnumerator CarStopTime()
	{
		if (dog)
		{
			dogAnim.Play(IdleAnim);
		}
		moving = false;
		yield return new WaitForSeconds(waypoints[currentWPNum].stopTime);
		if (dog)
		{
			dogAnim.Play(WalkAnim);
		}
		moving = true;
		stopCo = null;
	}

	private void ReachedEnd()
	{
		reachedEnd = true;
		moving = false;
		FinishEvent.Invoke();
	}

	public void StartMoving()
	{
		moving = true;
	}

	public void StopMoving()
	{
		moving = false;
	}
}
