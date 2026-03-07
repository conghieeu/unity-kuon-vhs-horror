using System.Collections;
using UnityEngine;

public class CameraFaceDetection : MonoBehaviour
{
	public Collider worldGO;

	public RectTransform pointer;

	public Vector3 cameraSpacePos;

	public AudioSource detection;

	public float appearDist = 10f;

	public LayerMask blockMask;

	private float distToPlayer;

	[HideInInspector]
	public Camera playerCam;

	private Coroutine co;

	public static CameraFaceDetection Instance;

	private Vector3 lastPointerPos;

	private Vector3 CenterPos;

	private void Awake()
	{
		Instance = this;
	}

	private void Start()
	{
		playerCam = Camera.main;
	}

	private void Update()
	{
		if (worldGO != null)
		{
			distToPlayer = Vector3.Distance(worldGO.transform.position, playerCam.transform.position);
			CenterPos = worldGO.bounds.center;
		}
		if (co == null)
		{
			co = StartCoroutine(FaceInterval());
		}
	}

	private IEnumerator FaceInterval()
	{
		if (lastPointerPos != playerCam.WorldToScreenPoint(CenterPos))
		{
			pointer.position = playerCam.WorldToScreenPoint(CenterPos);
			lastPointerPos = pointer.position;
			if (CheckIfInView())
			{
				detection.Play();
				pointer.gameObject.SetActive(value: true);
			}
			else
			{
				pointer.gameObject.SetActive(value: false);
			}
			yield return new WaitForSeconds(3f);
			co = null;
		}
	}

	private bool CheckIfInView()
	{
		if (OnScreen() && distToPlayer < appearDist)
		{
			return true;
		}
		return false;
	}

	private bool OnScreen()
	{
		if (worldGO != null)
		{
			Vector3 vector = playerCam.WorldToViewportPoint(worldGO.transform.position);
			if (vector.z > 0f && vector.x > 0f && vector.x < 1f && vector.y > 0f)
			{
				return vector.y < 1f;
			}
			return false;
		}
		return false;
	}
}
