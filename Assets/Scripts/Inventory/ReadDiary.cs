using System.Collections.Generic;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class ReadDiary : MonoBehaviour
{
	[ShowInInspector]
	[DisplayAsString]
	public bool goodEnding;

	public TextMeshPro counterText;

	public List<GameObject> DiaryList;

	private int currentDiary;

	public UnityEvent BadEnding;

	public UnityEvent GoodEnding;

	public void NextDiary()
	{
		DiaryList[currentDiary].SetActive(value: true);
		currentDiary++;
		counterText.text = currentDiary.ToString();
		CheckComplete();
	}

	private void CheckComplete()
	{
		if (DiaryList.Count <= currentDiary)
		{
			if (goodEnding)
			{
				GoodEnding.Invoke();
			}
			else
			{
				BadEnding.Invoke();
			}
		}
	}

	public void ChangeToGoodEnding()
	{
		goodEnding = true;
	}
}
