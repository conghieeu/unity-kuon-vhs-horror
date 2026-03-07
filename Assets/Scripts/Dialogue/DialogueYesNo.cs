using I2.Loc;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;

public class DialogueYesNo : MonoBehaviour
{
	public bool repeatable = true;

	public bool useItem;

	[ShowIf("useItem", true)]
	public string ItemName;

	[ShowIf("useItem", true)]
	public bool removeFromInventory = true;

	[TextArea]
	public string sentence;

	private Localize i2Localize;

	[Space]
	public bool unlockPlayerOnYes = true;

	public UnityEvent yesEvent;

	public bool unlockPlayerOnNo = true;

	public UnityEvent noEvent;

	private InteractManager intManager;

	private DialogueManager diaManager;

	private Icons icon;

	private DialogueTrigger diaTrigger;

	private bool show;

	private bool onYes;

	private string currentText;

	private bool activated;

	private void Start()
	{
		intManager = InteractManager.Instance;
		diaManager = DialogueManager.Instance;
		i2Localize = GetComponent<Localize>();
		if (i2Localize != null)
		{
			sentence = i2Localize.mTranslation;
		}
		icon = GetComponent<Icons>();
		diaTrigger = GetComponent<DialogueTrigger>();
	}

	public void Ask()
	{
		show = true;
		diaManager.YesNoPar.SetActive(value: true);
		diaManager.StartDialogue();
		diaManager.typeCo = StartCoroutine(diaManager.TypeSentence(sentence));
	}

	private void Update()
	{
		if (activated)
		{
			return;
		}
		if (intManager.InteractingObject == base.gameObject && !show)
		{
			if (Input.GetKeyDown(intManager.interactKey))
			{
				if (useItem && (SP_Inventory.Instance.HoldingItem == null || SP_Inventory.Instance.HoldingItem != ItemName))
				{
					return;
				}
				Ask();
			}
		}
		else if (show && Input.GetKeyDown(intManager.interactKey))
		{
			if (onYes)
			{
				if (useItem)
				{
					UseItem();
				}
				yesEvent.Invoke();
				diaManager.YesNoPar.SetActive(value: false);
				diaManager.sentences.Clear();
				diaManager.EndDialogue(unlockPlayerOnYes);
				diaManager.YesNoPointer.transform.position = diaManager.yesText.transform.position;
				diaManager.dialogueText.text = "";
				show = false;
				if (!repeatable)
				{
					activated = true;
				}
			}
			else
			{
				noEvent.Invoke();
				diaManager.YesNoPar.SetActive(value: false);
				diaManager.sentences.Clear();
				diaManager.EndDialogue(unlockPlayerOnNo);
				diaManager.YesNoPointer.transform.position = diaManager.noText.transform.position;
				diaManager.dialogueText.text = "";
				show = false;
				onYes = false;
			}
		}
		if (show)
		{
			if (Input.GetKeyDown(KeyCode.W))
			{
				onYes = true;
			}
			else if (Input.GetKeyDown(KeyCode.S))
			{
				onYes = false;
			}
			if (onYes)
			{
				diaManager.YesNoPointer.transform.position = diaManager.yesText.transform.position;
			}
			else
			{
				diaManager.YesNoPointer.transform.position = diaManager.noText.transform.position;
			}
			diaManager.dialogueContinue.enabled = false;
		}
		else
		{
			diaManager.dialogueContinue.enabled = true;
		}
	}

	public void UseItem()
	{
		if (SP_Inventory.Instance.HoldingItem != null && SP_Inventory.Instance.HoldingItem == ItemName && !activated)
		{
			activated = true;
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
