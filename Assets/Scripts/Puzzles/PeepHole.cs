using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class PeepHole : MonoBehaviour
{
	public Animator Fade;

	public UnityEvent Start;

	public UnityEvent Exit;

	private bool isIn;

	public Image peepHoleUI;

	private Coroutine co;

	public Camera PeepCam;

	public Transform PeepCamLookAtPt;

	private Camera MainCam;

	private float mouseX;

	private float mouseY;

	private void Awake()
	{
		MainCam = Camera.main;
	}

	private void Update()
	{
		if (!isIn)
		{
			if (InteractManager.Instance.InteractingObject == base.gameObject && Input.GetKeyDown(InteractManager.Instance.interactKey) && co == null)
			{
				co = StartCoroutine(PeepDelay());
				StartPeep();
			}
		}
		else if (Input.GetKeyDown(InteractManager.Instance.interactKey) && co == null)
		{
			co = StartCoroutine(PeepDelay());
			ExitPeep();
		}
		PeepHoleMovement();
	}

	public void StartPeep()
	{
		if (MainCam == null)
		{
			MainCam = Camera.main;
		}
		StartCoroutine(IN());
		Start.Invoke();
		isIn = true;
	}

	public void ExitPeep()
	{
		StartCoroutine(OUT());
		isIn = false;
	}

	private void PeepHoleMovement()
	{
		if (isIn)
		{
			mouseX += Input.GetAxis("Mouse X") * 0.001f;
			mouseY += Input.GetAxis("Mouse Y") * 0.001f;
			float x = Mathf.Clamp(mouseX, -0.05f, 0.05f);
			float y = Mathf.Clamp(mouseY, -0.05f, 0.05f);
			PeepCamLookAtPt.transform.localPosition = new Vector3(x, y, PeepCamLookAtPt.transform.localPosition.z);
			PeepCam.transform.LookAt(PeepCamLookAtPt);
		}
	}

	private IEnumerator PeepDelay()
	{
		yield return new WaitForSeconds(4f);
		co = null;
	}

	private IEnumerator IN()
	{
		Fade.Play("FadeOut");
		yield return new WaitForSeconds(3f);
		peepHoleUI.gameObject.SetActive(value: true);
		MainCam.enabled = false;
		PeepCam.enabled = true;
		Fade.Play("FadeIn");
	}

	private IEnumerator OUT()
	{
		Fade.Play("FadeOut");
		yield return new WaitForSeconds(3f);
		peepHoleUI.gameObject.SetActive(value: false);
		MainCam.enabled = true;
		PeepCam.enabled = false;
		Fade.Play("FadeIn");
		Exit.Invoke();
	}
}
