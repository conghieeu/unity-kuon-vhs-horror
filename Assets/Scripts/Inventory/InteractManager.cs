using UnityEngine;

public class InteractManager : MonoBehaviour
{
	public KeyCode interactKey = KeyCode.Mouse0;

	[Header("Raycast")]
	public float RaycastRange = 3f;

	public LayerMask cullLayers;

	public string InteractLayer;

	[HideInInspector]
	public bool isHeld;

	[HideInInspector]
	public bool inUse;

	public GameObject InteractingObject;

	public static InteractManager Instance;

	private Camera playerCam;

	private bool isPressed;

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
		if (Physics.Raycast(playerCam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f)), out var hitInfo, RaycastRange, cullLayers))
		{
			InteractingObject = hitInfo.collider.gameObject;
		}
		else
		{
			InteractingObject = null;
		}
	}
}
