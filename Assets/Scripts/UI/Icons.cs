using Sirenix.OdinInspector;
using UnityEngine;

public class Icons : MonoBehaviour
{
	public enum ClearType
	{
		None = 0,
		OnMouseDown = 1,
		OnMouseDownRepeat = 2,
		OnMouseOverRepeat = 3,
		OnMouseExitRepeat = 4
	}

	public Vector3 posOffset = new Vector3(0f, -0.1f, 0f);

	public Vector2 size = new Vector2(0.12f, 0.12f);

	public float appearDist = 3f;

	public bool dontFollowCam;

	[ShowIf("dontFollowCam", true)]
	public Vector3 rotOffset;

	public bool hideOnPickUp = true;

	public ClearType clearType;

	public bool seeThroughWalls = true;

	[HideInInspector]
	public GameObject plane;

	private Transform cam;

	private Transform playerCam;

	private Renderer planeRend;

	private InteractManager intScript;

	private Collider col;

	private float dist;

	private bool clear;

	private GameObject iconParent;

	private SP_Inventory inventory;

	private void Start()
	{
		playerCam = Camera.main.transform;
		cam = Camera.main.transform;
		intScript = InteractManager.Instance;
		col = GetComponent<Collider>();
		inventory = SP_Inventory.Instance;
	}

	private void Update()
	{
		if (iconParent != null)
		{
			iconParent.transform.position = col.bounds.center + posOffset;
		}
		dist = Vector3.Distance(col.bounds.center, playerCam.position);
		Clear();
		if (!clear)
		{
			if (dist < appearDist)
			{
				if (seeThroughWalls)
				{
					CreateIcon();
				}
				else if (CanSeeObject())
				{
					CreateIcon();
				}
			}
			else
			{
				DestroyIcon();
			}
			if (plane != null && !dontFollowCam)
			{
				iconParent.transform.LookAt(cam.position);
			}
		}
		if (hideOnPickUp && base.gameObject.layer == 10)
		{
			DestroyIcon();
		}
	}

	private void CreateIcon()
	{
		if (iconParent == null)
		{
			iconParent = new GameObject("IconParent");
			iconParent.transform.position = base.transform.position;
			iconParent.transform.position = col.bounds.center + posOffset;
			plane = IconCreator.CreatePlane(size.x, size.y, collider: false, inventory.iconMat);
			plane.transform.parent = iconParent.transform;
			plane.transform.localPosition = Vector3.zero;
			if (dontFollowCam)
			{
				plane.transform.localEulerAngles = rotOffset;
			}
			planeRend = plane.GetComponent<MeshRenderer>();
			plane.transform.localPosition = new Vector3(0f - size.x / 2f, 0f - size.y / 2f, 0f);
			if (!dontFollowCam)
			{
				plane.transform.localEulerAngles = Vector3.zero;
			}
			plane.layer = 9;
		}
	}

	private void Clear()
	{
		switch (clearType)
		{
		case ClearType.OnMouseDown:
			if (intScript.InteractingObject == base.gameObject && Input.GetKeyDown(InteractManager.Instance.interactKey))
			{
				DestroyIcon();
				clear = true;
			}
			break;
		case ClearType.OnMouseDownRepeat:
			if (intScript.InteractingObject == base.gameObject)
			{
				if (Input.GetKeyDown(InteractManager.Instance.interactKey) && !clear)
				{
					DestroyIcon();
					clear = true;
				}
			}
			else if (!Input.GetKeyDown(intScript.interactKey))
			{
				clear = false;
			}
			break;
		case ClearType.OnMouseOverRepeat:
			if (intScript.InteractingObject == base.gameObject)
			{
				DestroyIcon();
				clear = true;
			}
			else if (!Input.GetKeyDown(intScript.interactKey))
			{
				clear = false;
			}
			break;
		case ClearType.OnMouseExitRepeat:
			if (intScript.InteractingObject == base.gameObject)
			{
				clear = false;
				break;
			}
			DestroyIcon();
			clear = true;
			break;
		case ClearType.None:
			break;
		}
	}

	private bool CanSeeObject()
	{
		Vector3 direction = col.bounds.center - playerCam.position;
		if (Physics.Raycast(playerCam.position, direction, out var hitInfo, appearDist))
		{
			if (hitInfo.transform == base.transform)
			{
				return true;
			}
			return false;
		}
		return false;
	}

	private void DestroyIcon()
	{
		if (plane != null)
		{
			Object.Destroy(iconParent);
			planeRend = null;
		}
	}

	private void OnDisable()
	{
		if (plane != null)
		{
			plane.SetActive(value: false);
		}
	}

	private void OnEnable()
	{
		if (plane != null)
		{
			plane.SetActive(value: true);
		}
	}
}
