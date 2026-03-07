using UnityEngine;

public class CameraFaceDetection_Child : MonoBehaviour
{
	public float detectDistance = 10f;

	private CameraFaceDetection master;

	private float dist;

	private CameraFaceDetection_Child comparison;

	private void Start()
	{
		master = CameraFaceDetection.Instance;
	}

	private void OnDisable()
	{
		master.worldGO = null;
	}

	private void Update()
	{
		dist = Vector3.Distance(base.transform.position, master.playerCam.transform.position);
		if (!(dist < detectDistance))
		{
			return;
		}
		if (master.worldGO != null && master.worldGO != base.transform)
		{
			if (comparison == null)
			{
				comparison = master.worldGO.GetComponent<CameraFaceDetection_Child>();
			}
		}
		else
		{
			comparison = null;
		}
		if (comparison != null)
		{
			if (comparison.dist < dist)
			{
				master.worldGO = GetComponent<Collider>();
			}
		}
		else
		{
			master.worldGO = GetComponent<Collider>();
		}
	}
}
