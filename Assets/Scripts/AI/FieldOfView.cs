using System;
using System.Collections;
using Sirenix.OdinInspector;
using UnityEngine;

public class FieldOfView : MonoBehaviour
{
	public float viewRadius;

	[Range(0f, 360f)]
	public float viewAngle;

	public float viewDistance = 5f;

	public float offsetHeight = 0.5f;

	public LayerMask targetMask;

	public LayerMask obstacleMask;

	public LayerMask lowerObstacleMask;

	[Header("Debug")]
	[DisplayAsString]
	public bool canSeePlayer;

	[DisplayAsString]
	public Transform visibleTarget;

	private Coroutine co;

	private void Update()
	{
		if (co == null)
		{
			co = StartCoroutine(FindTargetsWithDelay(0.2f));
		}
	}

	private IEnumerator FindTargetsWithDelay(float Delay)
	{
		yield return new WaitForSeconds(Delay);
		FindVisibleTargets();
		co = null;
	}

	private void FindVisibleTargets()
	{
		canSeePlayer = false;
		visibleTarget = null;
		Collider[] array = Physics.OverlapSphere(base.transform.position, viewRadius, targetMask);
		for (int i = 0; i < array.Length; i++)
		{
			Transform transform = array[i].transform;
			Vector3 normalized = (transform.position - base.transform.position).normalized;
			if (!(Vector3.Angle(base.transform.forward, normalized) < viewAngle / 2f))
			{
				continue;
			}
			float num = Vector3.Distance(base.transform.position, transform.position);
			if (!Physics.Raycast(base.transform.position + base.transform.up * offsetHeight, normalized, num, obstacleMask))
			{
				visibleTarget = transform;
				if (transform.tag == "Player" && num < viewDistance)
				{
					canSeePlayer = true;
				}
			}
		}
	}

	public Vector3 DirFromAngle(float angleInDegrees, bool angleIsGlobal)
	{
		if (!angleIsGlobal)
		{
			angleInDegrees += base.transform.eulerAngles.y;
		}
		return new Vector3(Mathf.Sin(angleInDegrees * ((float)Math.PI / 180f)), 0f, Mathf.Cos(angleInDegrees * ((float)Math.PI / 180f)));
	}
}
