using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

public class SecretChase : MonoBehaviour
{
	private NavMeshAgent agent;

	public Transform player;

	public Transform GirlModel;

	public Animator CamAnim;

	public string camAnimName;

	public Vector2 teleportInterval = new Vector2(2f, 3f);

	public AudioSource GlitchAS;

	public List<AudioClip> glitchSound;

	[Space]
	public UnityEvent InEvent;

	public UnityEvent OutEvent;

	public UnityEvent KillEvent;

	private Coroutine teleportCo;

	private Coroutine dmgCo;

	private void Start()
	{
		agent = GetComponent<NavMeshAgent>();
	}

	private void Update()
	{
		agent.SetDestination(player.position);
		if (Vector3.Distance(GirlModel.position, player.position) < 0.5f && dmgCo == null)
		{
			dmgCo = StartCoroutine(DamagePlayer());
		}
		if (teleportCo == null)
		{
			teleportCo = StartCoroutine(Teleport());
		}
	}

	private IEnumerator DamagePlayer()
	{
		InEvent.Invoke();
		yield return new WaitForSeconds(3f);
		if (Vector3.Distance(GirlModel.position, player.position) < 0.5f)
		{
			base.gameObject.SetActive(value: false);
			KillEvent.Invoke();
		}
		else
		{
			OutEvent.Invoke();
		}
		dmgCo = null;
	}

	private IEnumerator Teleport()
	{
		yield return new WaitForSeconds(Random.Range(teleportInterval.x, teleportInterval.y));
		GlitchAS.clip = glitchSound[Random.Range(0, glitchSound.Count - 1)];
		GlitchAS.pitch = Random.Range(0.7f, 1f);
		GlitchAS.Play();
		CamAnim.Play(camAnimName);
		GirlModel.position = base.transform.position - Vector3.up;
		GirlModel.rotation = base.transform.rotation;
		teleportCo = null;
	}
}
