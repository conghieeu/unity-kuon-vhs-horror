using System;
using System.Collections.Generic;
using UnityEngine.Events;

[Serializable]
public class DialogueSet
{
	public List<DialogueSentenceList> sentenceList;

	public UnityEvent OnFinish;
}
