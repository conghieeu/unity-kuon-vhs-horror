using System.Collections;
using UnityEngine;

public class DamagePlayer : MonoBehaviour
{
	private bool entered;

	private HealthManager manager;

	private Coroutine delayCo;

	private void Start()
	{
		manager = HealthManager.Instance;
	}

	private void OnTriggerEnter(Collider other)
	{
		if (other.tag == "Player")
		{
			entered = true;
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (other.tag == "Player")
		{
			entered = false;
		}
	}

	private void Update()
	{
		if (entered && delayCo == null)
		{
			delayCo = StartCoroutine(AttackDelay());
		}
	}

	private IEnumerator AttackDelay()
	{
		manager.ApplyDamage(30f);
		yield return new WaitForSeconds(3f);
		delayCo = null;
	}
}
