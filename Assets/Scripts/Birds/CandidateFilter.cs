using System.Collections.Generic;
using UnityEngine;
using ch.sycoforge.Decal;
using ch.sycoforge.Decal.Projectors;

[RequireComponent(typeof(EasyDecal))]
public class CandidateFilter : MonoBehaviour
{
	public GameObject ExclusiveReceiver;

	private EasyDecal decal;

	private void Start()
	{
		decal = GetComponent<EasyDecal>();
		ch.sycoforge.Decal.Projectors.Projector projector = decal.Projector;
		if (projector != null && projector is BoxProjector)
		{
			(projector as BoxProjector).OnCandidatesProcessed += bp_OnCandidatesProcessed;
		}
	}

	private void bp_OnCandidatesProcessed(List<Collider> colliders)
	{
		List<Collider> list = new List<Collider>();
		foreach (Collider collider in colliders)
		{
			if (!collider.gameObject.Equals(ExclusiveReceiver))
			{
				list.Add(collider);
			}
		}
		foreach (Collider item in list)
		{
			colliders.Remove(item);
		}
	}
}
