using System.Collections;
using Sirenix.OdinInspector;
using UnityEngine;

public class SP_Inventory : MonoBehaviour
{
	public static SP_Inventory Instance;

	public Material iconMat;

	public GameObject HoldPos;

	[HideInInspector]
	public GameObject playerGO;

	[DisplayAsString]
	[ShowInInspector]
	public string HoldingItem;

	public KeyCode dropKey;

	public AudioClip DropItemSound;

	public AudioClip PickupItemSound;

	public GameObject ItemDropUI;

	private Animator itemDropAnim;

	private Coroutine co;

	private CharacterController player;

	private void Awake()
	{
		Instance = this;
		itemDropAnim = ItemDropUI.GetComponent<Animator>();
		player = GetComponent<CharacterController>();
		playerGO = base.gameObject;
	}

	private void Update()
	{
		if (HoldPos.transform.childCount > 0)
		{
			HoldingItem = HoldPos.transform.GetChild(0).name;
		}
		else
		{
			HoldingItem = null;
		}
		if (Input.GetKeyDown(dropKey) && HoldPos.transform.childCount > 0)
		{
			Transform child = HoldPos.transform.GetChild(0);
			child.GetComponent<SP_Item>().DropItem();
			child.position = base.transform.position - Vector3.up * (player.height / 2f);
			AudioSource.PlayClipAtPoint(DropItemSound, base.transform.position);
		}
	}

	public void InventoryFull()
	{
		if (co == null)
		{
			co = StartCoroutine(showForALittle());
			return;
		}
		StopCoroutine(co);
		co = null;
		co = StartCoroutine(showForALittle());
	}

	private IEnumerator showForALittle()
	{
		itemDropAnim.Play("FadeIn");
		yield return new WaitForSeconds(4f);
		itemDropAnim.Play("FadeOut");
		co = null;
	}
}
