using Sirenix.OdinInspector;
using Steamworks;
using UnityEngine;

public class AchievementManager : MonoBehaviour
{
	public bool DisableAchievements;

	[Space]
	public string LockAchievementName = "achievement_";

	public void UnlockSteamAchievement(string ID)
	{
		if (!DisableAchievements && !TestSteamAchievement(ID))
		{
			SteamUserStats.SetAchievement(ID);
			SteamUserStats.StoreStats();
		}
	}

	private bool TestSteamAchievement(string ID)
	{
		SteamUserStats.GetAchievement(ID, out var pbAchieved);
		return pbAchieved;
	}

	[Button]
	private void LockAchievement()
	{
		SteamUserStats.ClearAchievement(LockAchievementName);
		SteamUserStats.StoreStats();
	}
}
