using UnityEngine;

public class FlockScare : MonoBehaviour
{
	public LandingSpotController[] landingSpotControllers;

	public float scareInterval = 0.1f;

	public float distanceToScare = 2f;

	public int checkEveryNthLandingSpot = 1;

	public int InvokeAmounts = 1;

	private int lsc;

	private int ls;

	private LandingSpotController currentController;

	private void CheckProximityToLandingSpots()
	{
		IterateLandingSpots();
		if (currentController._activeLandingSpots > 0 && CheckDistanceToLandingSpot(landingSpotControllers[lsc]))
		{
			landingSpotControllers[lsc].ScareAll();
		}
		Invoke("CheckProximityToLandingSpots", scareInterval);
	}

	private void IterateLandingSpots()
	{
		ls += checkEveryNthLandingSpot;
		currentController = landingSpotControllers[lsc];
		int childCount = currentController.transform.childCount;
		if (ls > childCount - 1)
		{
			ls -= childCount;
			if (lsc < landingSpotControllers.Length - 1)
			{
				lsc++;
			}
			else
			{
				lsc = 0;
			}
		}
	}

	private bool CheckDistanceToLandingSpot(LandingSpotController lc)
	{
		Transform child = lc.transform.GetChild(ls);
		if (child.GetComponent<LandingSpot>().landingChild != null && (child.position - base.transform.position).sqrMagnitude < distanceToScare * distanceToScare)
		{
			return true;
		}
		return false;
	}

	private void Invoker()
	{
		for (int i = 0; i < InvokeAmounts; i++)
		{
			float num = scareInterval / (float)InvokeAmounts * (float)i;
			Invoke("CheckProximityToLandingSpots", scareInterval + num);
		}
	}

	private void OnEnable()
	{
		CancelInvoke("CheckProximityToLandingSpots");
		if (landingSpotControllers.Length != 0)
		{
			Invoker();
		}
	}

	private void OnDisable()
	{
		CancelInvoke("CheckProximityToLandingSpots");
	}
}
