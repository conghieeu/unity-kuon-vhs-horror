using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Icons))]
public class SP_Item : MonoBehaviour
{
	public Vector3 posOffset;

	public Vector3 rotOffset;

	public UnityEvent pickupEvent;

	[Header("Drop offsets")]
	public Vector3 dropPos;

	public Vector3 dropRot;

	private Collider col;

	private Icons icon;

	private void Start()
	{
		icon = GetComponent<Icons>();
	}

	private void Update()
	{
		if (InteractManager.Instance.InteractingObject == base.gameObject && Input.GetKeyDown(InteractManager.Instance.interactKey))
		{
			if (SP_Inventory.Instance.HoldingItem == null)
			{
				PickUp();
			}
			else
			{
				SP_Inventory.Instance.InventoryFull();
			}
		}
	}

	public void PickUp()
	{
		if (icon != null)
		{
			icon.enabled = false;
		}
		if (SP_Inventory.Instance.PickupItemSound != null)
		{
			AudioSource.PlayClipAtPoint(SP_Inventory.Instance.PickupItemSound, base.transform.position);
		}
		pickupEvent.Invoke();
		col = GetComponent<Collider>();
		base.transform.parent = SP_Inventory.Instance.HoldPos.transform;
		col.enabled = false;
		base.transform.localPosition = posOffset;
		base.transform.localEulerAngles = rotOffset;
		base.gameObject.layer = 10;
		Transform[] componentsInChildren = GetComponentsInChildren<Transform>(includeInactive: true);
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].gameObject.layer = LayerMask.NameToLayer("Default");
		}
	}

	public void DropItem()
	{
		col.enabled = true;
		if (icon != null)
		{
			icon.enabled = true;
		}
		base.transform.parent = null;
		base.transform.position += dropPos;
		base.transform.eulerAngles = dropRot;
		base.gameObject.layer = 0;
		Transform[] componentsInChildren = GetComponentsInChildren<Transform>(includeInactive: true);
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].gameObject.layer = LayerMask.NameToLayer("Default");
		}
	}

	public void GetItemPosNRot()
	{
		posOffset = base.transform.localPosition;
		rotOffset = base.transform.localEulerAngles;
	}
}
