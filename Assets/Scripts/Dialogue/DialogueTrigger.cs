using System.Collections;
using System.Collections.Generic;
using I2.Loc;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;

public class DialogueTrigger : MonoBehaviour
{
	[ShowInInspector]
	[DisplayAsString]
	public int dialogueNum;

	[ShowInInspector]
	[DisplayAsString]
	public int lineNum;

	public bool repeatable = true;

	public bool interactToTalk = true;

	public bool Localize = true;

	[EnableIf("Localize")]
	public GameObject localizeDialogue;

	[Space]
	public UnityEvent StartEvent;

	public UnityEvent EndEvent;

	public List<DialogueSet> Dialogues;

	private Localize[] diaLocList;

	private DialogueManager manager;

	private Coroutine convoCD;

	private Coroutine numChangeCo;

	private bool startedConvo;

	private bool completed;

	private int gibberNum;

	private int sentenceIndex;

	private int dialogueIndex;

	[HideInInspector]
	public bool show;

	private void Start()
	{
		manager = DialogueManager.Instance;
		if (Localize)
		{
			diaLocList = localizeDialogue.GetComponents<Localize>();
		}
		LocalizeDialogue();
	}

	private void LocalizeDialogue()
	{
		if (!Localize)
		{
			return;
		}
		diaLocList = localizeDialogue.GetComponents<Localize>();
		dialogueIndex = 0;
		for (int i = 0; i < diaLocList.Length; i++)
		{
			if (dialogueIndex < Dialogues.Count && sentenceIndex == Dialogues[dialogueIndex].sentenceList.Count)
			{
				sentenceIndex = 0;
				dialogueIndex++;
			}
			if (dialogueIndex < Dialogues.Count)
			{
				if (Dialogues[dialogueIndex].sentenceList[sentenceIndex].localizeSentence)
				{
					Dialogues[dialogueIndex].sentenceList[sentenceIndex].sentences = diaLocList[i].mTranslation;
					sentenceIndex++;
				}
				else
				{
					sentenceIndex++;
					Dialogues[dialogueIndex].sentenceList[sentenceIndex].sentences = diaLocList[i].mTranslation;
					sentenceIndex++;
				}
			}
		}
	}

	public void TriggerDialogue()
	{
		if (completed)
		{
			return;
		}
		LocalizeDialogue();
		if (manager == null)
		{
			manager = DialogueManager.Instance;
		}
		if (convoCD != null)
		{
			return;
		}
		if (lineNum != Dialogues[dialogueNum].sentenceList.Count)
		{
			if (Dialogues[dialogueNum].sentenceList[lineNum].sfx != null)
			{
				AudioSource.PlayClipAtPoint(Dialogues[dialogueNum].sentenceList[lineNum].sfx, base.transform.position);
			}
			if (Dialogues[dialogueNum].sentenceList[lineNum].eventTrigger != null)
			{
				Dialogues[dialogueNum].sentenceList[lineNum].eventTrigger.Invoke();
			}
		}
		if (lineNum != Dialogues[dialogueNum].sentenceList.Count)
		{
			if (!startedConvo)
			{
				StartEvent?.Invoke();
				manager.StartDialogue(Dialogues[dialogueNum]);
				startedConvo = true;
				lineNum++;
			}
			else if (manager.typing)
			{
				manager.SkipToDisplayAll(Dialogues[dialogueNum].sentenceList[lineNum - 1].sentences);
			}
			else
			{
				manager.DisplayNextSentence(Dialogues[dialogueNum], lineNum);
				lineNum++;
			}
			return;
		}
		if (manager.typing)
		{
			manager.SkipToDisplayAll(Dialogues[dialogueNum].sentenceList[lineNum - 1].sentences);
			return;
		}
		startedConvo = false;
		lineNum = 0;
		manager.sentences.Clear();
		manager.dialogueText.text = "";
		manager.EndDialogue(unlockPlayer: true);
		Dialogues[dialogueNum].OnFinish?.Invoke();
		convoCD = StartCoroutine(DelayOnEnd());
		manager.dialogueContinue.gameObject.SetActive(value: false);
		if (!repeatable)
		{
			completed = true;
		}
	}

	private IEnumerator DelayOnEnd()
	{
		yield return new WaitForSeconds(1.5f);
		convoCD = null;
	}

	private void Update()
	{
		if (interactToTalk)
		{
			if (InteractManager.Instance.InteractingObject == base.gameObject)
			{
				show = true;
				if (Input.GetKeyDown(InteractManager.Instance.interactKey))
				{
					TriggerDialogue();
				}
			}
			else if (show)
			{
				if (manager.typeCo != null)
				{
					StopCoroutine(manager.typeCo);
				}
				EndEvent?.Invoke();
				show = false;
				startedConvo = false;
				lineNum = 0;
			}
		}
		else if (Input.GetKeyDown(InteractManager.Instance.interactKey))
		{
			TriggerDialogue();
		}
		if (manager.dialogueText.text != "" && startedConvo)
		{
			if (manager.typing)
			{
				manager.dialogueContinue.gameObject.SetActive(value: false);
			}
			else if (lineNum < Dialogues[dialogueNum].sentenceList.Count)
			{
				manager.dialogueContinue.gameObject.SetActive(value: true);
			}
			else
			{
				manager.dialogueContinue.gameObject.SetActive(value: false);
			}
		}
	}

	public void StartConvo()
	{
		startedConvo = false;
		lineNum = 0;
		show = true;
		TriggerDialogue();
	}

	public void SetDialogue(int num)
	{
		ClearDialogue();
		dialogueNum = num;
	}

	public void ClearDialogue()
	{
		if (manager.typeCo != null)
		{
			StopCoroutine(manager.typeCo);
			manager.typeCo = null;
		}
		manager.sentences.Clear();
		manager.dialogueText.text = "";
	}
}
