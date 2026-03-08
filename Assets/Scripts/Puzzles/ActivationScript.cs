using System.Collections;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;

public class ActivationScript : MonoBehaviour
{
	public enum ActivationType
	{
		Interact = 0,
		Trigger = 1,
		Start = 2,
		MouseOver = 3,
		MouseOverThenExit = 4,
		Disable = 5,
		Script = 6
	}

	[TextArea]
	public string Description;

	[ShowInInspector]
	[DisplayAsString]
	public bool activated;

	public ActivationType activationType;

	[SerializeField]
	private bool repeatable;

	private bool onStart;

	private bool onInteract;

	private bool onTrigger;

	private bool onMouseOver;

	private bool onMouseOverThenExit;

	private bool onDisable;

	public UnityEvent InteractEvent;

	[SerializeField]
	private AudioClip oneShotSound;

	[Space]
	public bool conditional;

	[ShowIf("conditional", true)]
	public float activationTimer;

	[ShowIf("conditional", true)]
	public int numToActivate = 1;

	[ShowInInspector]
	[DisplayAsString]
	[ShowIf("conditional", true)]
	public int currentNum;

	[Space]
	public bool extra;

	[ShowIf("extra", true)]
	public Transform obj;

	[ShowIf("extra", true)]
	public Transform moveObjTo;

	[ShowIf("extra", true)]
	public bool inputActivation;

	[ShowIf("inputActivation", true)]
	public KeyCode extraInput;

	private InteractManager intScript;

	private bool invokeUp;

	private bool isPlayed;

	private bool onceUnlock;

	private bool loadSound;

	[HideInInspector]
	public bool isInvoked;

	[HideInInspector]
	public bool isUp;

	private void Start()
	{
		intScript = InteractManager.Instance;
		switch (activationType)
		{
		case ActivationType.Interact:
			onInteract = true;
			break;
		case ActivationType.Trigger:
			onTrigger = true;
			break;
		case ActivationType.Start:
			onStart = true;
			break;
		case ActivationType.MouseOver:
			onMouseOver = true;
			break;
		case ActivationType.MouseOverThenExit:
			onMouseOverThenExit = true;
			break;
		case ActivationType.Disable:
			onDisable = true;
			break;
		}
	}

	private void OnEnable()
	{
		ActivationType activationType = this.activationType;
		if (activationType == ActivationType.Disable && repeatable)
		{
			onDisable = true;
		}
	}

	private void OnDisable()
	{
		if (onDisable)
		{
			PreActivation();
		}
		ActivationType activationType = this.activationType;
		if (activationType == ActivationType.Start && repeatable)
		{
			onStart = true;
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		if (onTrigger && other.tag == "Player")
		{
			PreActivation();
		}
	}

	private void Update()
	{
		if (onStart && base.isActiveAndEnabled && base.gameObject.activeSelf)
		{
			PreActivation();
		}
		if (intScript.InteractingObject == base.gameObject)
		{
			if (onMouseOver)
			{
				PreActivation();
			}
			if (onInteract && Input.GetKeyDown(intScript.interactKey))
			{
				PreActivation();
			}
		}
		if (inputActivation && Input.GetKeyDown(extraInput))
		{
			PreActivation();
		}
	}

	private void OnExit()
	{
		if (onMouseOverThenExit)
		{
			FireEvent();
		}
	}

	public void PreActivation()
	{
		onStart = false;
		onDisable = false;
		if (!repeatable)
		{
			onTrigger = false;
			onInteract = false;
			onMouseOver = false;
		}
		if (!activated)
		{
			if (activationTimer == 0f)
			{
				Activation();
			}
			else
			{
				StartCoroutine(Timer());
			}
		}
	}

	private void Activation()
	{
		if (numToActivate > 1)
		{
			if (currentNum + 1 != numToActivate)
			{
				currentNum++;
				return;
			}
			if (currentNum + 1 == numToActivate)
			{
				repeatable = false;
			}
		}
		UnityLogger.Log("Activated: " + base.gameObject.name);
		PlaySound();
		if (!repeatable)
		{
			activated = true;
		}
		FireEvent();
	}

	private void FireEvent()
	{
		if (!invokeUp)
		{
			InteractEvent?.Invoke();
			isInvoked = true;
			isUp = true;
			if (!repeatable)
			{
				invokeUp = true;
			}
			ApplyExtra();
		}
	}

	private void ApplyExtra()
	{
		if (moveObjTo != null && obj != null)
		{
			obj.position = moveObjTo.position;
		}
	}

	private IEnumerator Timer()
	{
		if (!repeatable)
		{
			activated = true;
		}
		yield return new WaitForSeconds(activationTimer);
		Activation();
	}

	private void PlaySound()
	{
		if (oneShotSound != null)
		{
			AudioSource.PlayClipAtPoint(oneShotSound, base.transform.position);
		}
	}
}
