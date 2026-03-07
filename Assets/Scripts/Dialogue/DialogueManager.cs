using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using StarterAssets;

public class DialogueManager : MonoBehaviour
{
	[Header("UI Text Slots")]
	public Text dialogueText;

	public Image dialogueContinue;

	public float delay;

	public Image textBubble;

	[Space]
	[HideInInspector]
	public GameObject YesNoPar;

	public Text yesText;

	public Text noText;

	public GameObject YesNoPointer;

	private string currentText;

	public Queue<string> sentences;

	[HideInInspector]
	public Coroutine typeCo;

	[HideInInspector]
	public bool typing;

	public static DialogueManager Instance;

	private bool fadeInBubble;

	public FirstPersonController fpController;

	private void Awake()
	{
		Instance = this;
		YesNoPar = YesNoPointer.transform.parent.gameObject;
	}

	private void Start()
	{
		sentences = new Queue<string>();
	}

	public void StartDialogue(DialogueSet dialogue)
	{
		sentences.Clear();
		StartDialogue();
		foreach (DialogueSentenceList sentence in dialogue.sentenceList)
		{
			sentences.Enqueue(sentence.sentences);
		}
		DisplayNextSentence(dialogue, 0);
	}

	public void DisplayNextSentence(DialogueSet dialogue, int lineNum)
	{
		if (sentences.Count == 0)
		{
			EndDialogue(unlockPlayer: true);
			return;
		}
		string sentence = sentences.Dequeue();
		StopAllCoroutines();
		typeCo = StartCoroutine(TypeSentence(sentence));
	}

	public IEnumerator TypeSentence(string sentence)
	{
		for (int i = 0; i < sentence.Length + 1; i++)
		{
			typing = true;
			currentText = sentence.Substring(0, i);
			dialogueText.text = currentText;
			yield return new WaitForSeconds(delay);
			if (currentText == sentence)
			{
				typing = false;
			}
		}
	}

	public void StartDialogue()
	{
		fpController.enabled = false;
		textBubble.gameObject.SetActive(value: true);
	}

	public void EndDialogue(bool unlockPlayer)
	{
		if (typeCo != null)
		{
			StopCoroutine(typeCo);
		}
		dialogueText.text = "";
		dialogueContinue.gameObject.SetActive(value: false);
		if (unlockPlayer)
		{
			fpController.enabled = true;
		}
		textBubble.gameObject.SetActive(value: false);
	}

	public void SkipToDisplayAll(string completeText)
	{
		typing = false;
		StopAllCoroutines();
		dialogueText.text = completeText;
	}
}
