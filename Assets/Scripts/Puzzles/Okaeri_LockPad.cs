using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Okaeri_LockPad : MonoBehaviour
{
	public string description = "To set up, add animators to keys. In the animator, add 'revolveNum' int parameter and animate corresponding to the settings";

	public int digitNum = 4;

	public int revolveNum = 9;

	public float pressDelay = 0.3f;

	public List<int> correctNum;

	public List<Okaeri_LockPad_Key> Keys;

	public AudioClip click;

	public UnityEvent CorrectEvent;

	private Coroutine delayCo;

	private Animator animator;

	private bool ready;

	private int correctCounter;

	private void Awake()
	{
		animator = GetComponent<Animator>();
	}

	public void ButtonPress(Okaeri_LockPad_Key b)
	{
		if (delayCo == null)
		{
			int index = Keys.IndexOf(b);
			if (click != null)
			{
				AudioSource.PlayClipAtPoint(click, base.transform.position);
			}
			if (Keys[index].num == revolveNum)
			{
				Keys[index].num = 0;
			}
			else
			{
				Keys[index].num++;
			}
			Keys[index].anim.SetInteger("revolveNum", Keys[index].num);
			delayCo = StartCoroutine(KeyDelay());
			CheckIfCorrect();
		}
	}

	private void CheckIfCorrect()
	{
		correctCounter = 0;
		for (int i = 0; i < Keys.Count; i++)
		{
			if (Keys[i].num == correctNum[i])
			{
				correctCounter++;
			}
			if (correctCounter == correctNum.Count)
			{
				CorrectEvent?.Invoke();
			}
		}
	}

	private IEnumerator KeyDelay()
	{
		yield return new WaitForSeconds(pressDelay);
		delayCo = null;
	}
}
