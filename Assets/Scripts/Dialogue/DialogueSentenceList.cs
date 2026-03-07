using System;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public class DialogueSentenceList
{
	public AudioClip sfx;

	[TextArea(2, 10)]
	public string sentences;

	public bool options;

	[ShowIf("options", true)]
	public bool localizeSentence = true;

	[ShowIf("options", true)]
	public UnityEvent eventTrigger;
}
