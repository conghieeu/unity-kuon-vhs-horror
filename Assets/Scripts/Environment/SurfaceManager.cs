using System.Collections.Generic;
using UnityEngine;
using StarterAssets;

public class SurfaceManager : MonoBehaviour
{
	public List<SurfaceManager_Sets> surfaces;

	private FirstPersonController fpsController;

	private bool finding;

	private void Start()
	{
		fpsController = GetComponent<FirstPersonController>();
	}

	private void OnControllerColliderHit(ControllerColliderHit hit)
	{
		finding = true;
		for (int i = 0; i < surfaces.Count; i++)
		{
			if (hit.gameObject.CompareTag(surfaces[i].tag))
			{
				fpsController.m_FootstepSounds = surfaces[i].footsteps;
				fpsController.echoFootstep = surfaces[i].echo;
				finding = false;
			}
		}
		if (finding)
		{
			fpsController.m_FootstepSounds = surfaces[0].footsteps;
		}
	}
}
