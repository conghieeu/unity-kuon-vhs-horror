using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class HealthManager : MonoBehaviour
{
	public float health = 100f;

	public UnityEvent onDeathEvent;

	public Animator playerCamAnim;

	public List<string> hitAnims;

	public AudioSource hitAS;

	public List<AudioClip> hitSfx;

	private bool dead;

	public static HealthManager Instance;

	private void Awake()
	{
		Instance = this;
	}

	private void Start()
	{
	}

	private void Update()
	{
		if (health <= 0f && !dead)
		{
			dead = true;
			onDeathEvent.Invoke();
		}
	}

	public void ApplyDamage(float amount)
	{
		health -= amount;
		playerCamAnim.Play(hitAnims[Random.Range(0, hitAnims.Count - 1)]);
		hitAS.clip = hitSfx[Random.Range(0, hitSfx.Count - 1)];
		hitAS.Play();
	}
}
