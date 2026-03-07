using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class SP_CoinStackable : MonoBehaviour
{
	public Text CoinStackable;

	[ShowInInspector]
	[DisplayAsString]
	private int currentAmount;

	public int completeAmount = 100;

	public UnityEvent CompleteEvent;

	private bool complete;

	public void PickUp10Yen()
	{
		currentAmount += 10;
		CoinStackable.text = currentAmount.ToString();
		CheckIfComplete();
	}

	public void PickUp50Yen()
	{
		currentAmount += 50;
		CoinStackable.text = currentAmount.ToString();
		CheckIfComplete();
	}

	private void CheckIfComplete()
	{
		if (!complete && currentAmount >= completeAmount)
		{
			complete = true;
			CompleteEvent.Invoke();
		}
	}
}
