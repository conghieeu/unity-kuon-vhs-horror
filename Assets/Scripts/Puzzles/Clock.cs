using System;
using UnityEngine;

public class Clock : MonoBehaviour
{
	public int minutes;

	public int hour;

	public int seconds;

	public bool realTime = true;

	public GameObject pointerSeconds;

	public GameObject pointerMinutes;

	public GameObject pointerHours;

	public float clockSpeed = 1f;

	private float msecs;

	private void Start()
	{
		if (realTime)
		{
			hour = DateTime.Now.Hour;
			minutes = DateTime.Now.Minute;
			seconds = DateTime.Now.Second;
		}
	}

	private void Update()
	{
		msecs += Time.deltaTime * clockSpeed;
		if (msecs >= 1f)
		{
			msecs -= 1f;
			seconds++;
			if (seconds >= 60)
			{
				seconds = 0;
				minutes++;
				if (minutes > 60)
				{
					minutes = 0;
					hour++;
					if (hour >= 24)
					{
						hour = 0;
					}
				}
			}
		}
		float z = 6f * (float)seconds;
		float z2 = 6f * (float)minutes;
		float z3 = 30f * (float)hour + 0.5f * (float)minutes;
		pointerSeconds.transform.localEulerAngles = new Vector3(0f, 0f, z);
		pointerMinutes.transform.localEulerAngles = new Vector3(0f, 0f, z2);
		pointerHours.transform.localEulerAngles = new Vector3(0f, 0f, z3);
	}
}
