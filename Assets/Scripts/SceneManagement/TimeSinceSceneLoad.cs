using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

public class TimeSinceSceneLoad : MonoBehaviour
{
	public string sceneToTrack = "AkaManto_v0.10";

	public Text UIhrs;

	public Text UIMins;

	public Text UISecs;

	[ShowInInspector]
	[DisplayAsString]
	public float hours;

	[ShowInInspector]
	[DisplayAsString]
	public float minutes;

	[ShowInInspector]
	[DisplayAsString]
	public float seconds;

	[ShowInInspector]
	[DisplayAsString]
	public float time;

	[ShowInInspector]
	[DisplayAsString]
	public string timeOnStop;

	[ShowInInspector]
	[DisplayAsString]
	public bool tracking = true;

	private bool trackingTime;

	private void Awake()
	{
		Object.DontDestroyOnLoad(base.gameObject);
	}

	private void Update()
	{
		if (Time.timeScale != 0f)
		{
			time = Time.timeSinceLevelLoad;
			trackingTime = true;
			hours = time / 3600f;
			minutes = time % 3600f / 60f;
			seconds = time % 3600f % 60f;
		}
		if (Mathf.FloorToInt(hours) < 10)
		{
			UIhrs.text = "0" + Mathf.FloorToInt(hours);
		}
		else
		{
			UIhrs.text = Mathf.FloorToInt(hours).ToString();
		}
		if (Mathf.FloorToInt(minutes) < 10)
		{
			UIMins.text = "0" + Mathf.FloorToInt(minutes);
		}
		else
		{
			UIMins.text = Mathf.RoundToInt(minutes).ToString();
		}
		if (Mathf.RoundToInt(seconds) < 10)
		{
			UISecs.text = "0" + Mathf.FloorToInt(seconds);
		}
		else
		{
			UISecs.text = Mathf.RoundToInt(seconds).ToString();
		}
	}
}
