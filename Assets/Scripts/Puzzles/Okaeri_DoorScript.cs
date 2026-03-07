using System.Collections;
using Sirenix.OdinInspector;
using UnityEngine;

public class Okaeri_DoorScript : MonoBehaviour
{
	public bool locked;

	[Header("Animation")]
	public float interactDelay;

	public Animator animatorObject;

	public string openAnim;

	public string closeAnim;

	[Header("SFX")]
	public AudioClip openSound;

	public AudioClip closeSound;

	public AudioClip unlockSound;

	public AudioClip lockedSound;

	[Header("Debug")]
	[DisplayAsString]
	[ShowInInspector]
	private bool opened;

	private InteractManager intManager;

	private Coroutine co;

	private void Start()
	{
		intManager = InteractManager.Instance;
	}

	private void Update()
	{
		if (!(intManager.InteractingObject == base.gameObject))
		{
			return;
		}
		if (Input.GetKeyDown(intManager.interactKey) && !locked)
		{
			if (co == null)
			{
				co = StartCoroutine(InteractDelay());
				if (!opened)
				{
					OpenDoor();
				}
				else
				{
					CloseDoor();
				}
			}
		}
		else if (Input.GetKeyDown(intManager.interactKey) && locked && co == null)
		{
			co = StartCoroutine(InteractDelay());
			if (lockedSound != null)
			{
				AudioSource.PlayClipAtPoint(lockedSound, base.transform.position);
			}
		}
	}

	private IEnumerator InteractDelay()
	{
		yield return new WaitForSeconds(interactDelay);
		co = null;
	}

	public void CloseDoor()
	{
		AudioSource.PlayClipAtPoint(closeSound, base.transform.position);
		animatorObject.Play(closeAnim);
		opened = false;
	}

	public void OpenDoor()
	{
		AudioSource.PlayClipAtPoint(openSound, base.transform.position);
		animatorObject.Play(openAnim);
		opened = true;
	}

	public void UnlockDoor()
	{
		locked = false;
		AudioSource.PlayClipAtPoint(unlockSound, base.transform.position);
	}

	public void LockDoor()
	{
		locked = true;
		AudioSource.PlayClipAtPoint(lockedSound, base.transform.position);
	}
}
