using System.Collections.Generic;
using I2.Loc;
using UnityEngine;

public class AM_LocalizationManager : MonoBehaviour
{
	public static AM_LocalizationManager Instance;

	public GameObject ItemDescriptions;

	public GameObject DialogueNames;

	public Localize pocketFullText;

	[HideInInspector]
	public List<GameObject> itemDescriptionList;

	public List<Localize> dialogueNameList;

	[HideInInspector]
	public List<string> itemNameList;

	private void Awake()
	{
		Instance = this;
		for (int i = 0; i < ItemDescriptions.transform.childCount; i++)
		{
			itemDescriptionList.Add(ItemDescriptions.transform.GetChild(i).gameObject);
		}
		for (int j = 0; j < ItemDescriptions.transform.childCount; j++)
		{
			itemNameList.Add(ItemDescriptions.transform.GetChild(j).name);
		}
		for (int k = 0; k < DialogueNames.transform.childCount; k++)
		{
			dialogueNameList.Add(DialogueNames.transform.GetChild(k).GetComponent<Localize>());
		}
	}

	public GameObject FindItemLocalization(string name)
	{
		return itemDescriptionList[itemNameList.IndexOf(name)];
	}

	public GameObject FindDialogueName(int nameIndex)
	{
		return dialogueNameList[nameIndex].gameObject;
	}
}
