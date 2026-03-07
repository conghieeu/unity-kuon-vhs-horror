using UnityEngine;
using UnityEngine.Events;

public class ClockPuzzle : MonoBehaviour
{
	public float moveSpd = 2f;

	public AudioSource moveAS;

	public Transform minHand;

	public Transform hourHand;

	public ClockPuzzle_Hands minTrigger;

	public ClockPuzzle_Hands hrTrigger;

	public UnityEvent inEvent;

	public UnityEvent tryOutEvent;

	public UnityEvent outEvent;

	public UnityEvent completeEvent;

	private bool controlling;

	private Coroutine CD;

	private InteractManager intManager;

	private bool complete;

	private void Start()
	{
		intManager = InteractManager.Instance;
	}

	public void GoIn()
	{
		if (!complete)
		{
			inEvent.Invoke();
			controlling = true;
		}
	}

	public void GoOut()
	{
		outEvent.Invoke();
		controlling = false;
	}

	private void Update()
	{
		if (complete)
		{
			return;
		}
		if (controlling)
		{
			if (Input.GetKey(KeyCode.A) && !Input.GetKey(KeyCode.D))
			{
				if (!moveAS.isPlaying)
				{
					moveAS.Play();
				}
				minHand.Rotate(Vector3.forward * moveSpd, Space.Self);
			}
			if (Input.GetKey(KeyCode.D) && !Input.GetKey(KeyCode.A))
			{
				if (!moveAS.isPlaying)
				{
					moveAS.Play();
				}
				hourHand.Rotate(Vector3.forward * moveSpd, Space.Self);
			}
			if ((Input.GetKeyUp(KeyCode.D) || Input.GetKeyUp(KeyCode.A)) && !Input.GetKey(KeyCode.A) && !Input.GetKey(KeyCode.D))
			{
				if (moveAS.isPlaying)
				{
					moveAS.Stop();
				}
				Check();
			}
			if (Input.GetKeyDown(intManager.interactKey))
			{
				tryOutEvent.Invoke();
				controlling = false;
			}
		}
		else if (moveAS.isPlaying)
		{
			moveAS.Stop();
		}
	}

	private void Check()
	{
		if (minTrigger.isIn && hrTrigger.isIn)
		{
			complete = true;
			completeEvent.Invoke();
		}
	}
}
