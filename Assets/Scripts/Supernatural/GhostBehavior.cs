using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using StarterAssets;

public class GhostBehavior : MonoBehaviour
{
	public FirstPersonController player;

	public float deathCounter = 10f;

	public Animator camAnim;

	public string enterAnimBool = "enter";

	public UnityEvent deathEvent;

	[Header("Behavior")]
	public float speed;

	private Transform playerCam;

	private Coroutine co;

	private void Start()
	{
		playerCam = Camera.main.transform;
	}

	private void Update()
	{
		base.transform.LookAt(playerCam.transform);
		base.transform.Translate(Vector3.forward * speed);
	}

	private void OnTriggerEnter(Collider other)
	{
		if (other.tag == "Player")
		{
			if (co != null)
			{
				StopCoroutine(co);
			}
			player.m_WalkSpeed = 1.5f;
			co = StartCoroutine(DeathCounter());
			camAnim.SetBool(enterAnimBool, value: true);
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (other.tag == "Player" && co != null)
		{
			player.m_WalkSpeed = 2f;
			camAnim.SetBool(enterAnimBool, value: false);
			StopCoroutine(co);
			co = null;
		}
	}

	private IEnumerator DeathCounter()
	{
		yield return new WaitForSeconds(deathCounter);
		deathEvent.Invoke();
	}
}
