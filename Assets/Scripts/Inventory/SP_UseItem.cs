using UnityEngine;
using UnityEngine.Events;

public class SP_UseItem : MonoBehaviour
{
	public string ItemName;

	public bool removeFromInventory = true;

	public UnityEvent UseEvent;

	private bool activated;

	private Icons icon;

	private DialogueTrigger diaTrigger;

	private void Start()
	{
		icon = GetComponent<Icons>();
		diaTrigger = GetComponent<DialogueTrigger>();
	}

	private void Update()
	{
		if (InteractManager.Instance.InteractingObject == base.gameObject && Input.GetKeyDown(InteractManager.Instance.interactKey))
		{
			UseItem();
		}
	}

	public void UseItem()
	{
		if (SP_Inventory.Instance.HoldingItem != null && SP_Inventory.Instance.HoldingItem == ItemName && !activated)
		{
			activated = true;
			UseEvent.Invoke();
			if (removeFromInventory)
			{
				Object.Destroy(SP_Inventory.Instance.HoldPos.transform.GetChild(0).gameObject);
			}
			if (icon != null)
			{
				icon.enabled = false;
			}
			if (diaTrigger != null)
			{
				diaTrigger.enabled = false;
			}
		}
	}
}
