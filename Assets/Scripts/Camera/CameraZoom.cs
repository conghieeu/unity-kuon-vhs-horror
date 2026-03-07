using UnityEngine;
using UnityEngine.UI;

public class CameraZoom : MonoBehaviour
{
	private Camera cam;

	public Camera iconCam;

	public AudioSource zoomAS;

	public Vector2 clamp = new Vector2(45f, 60f);

	public float speed = 7f;

	[Space]
	public Image zoomPointer;

	public GameObject zoomBarParent;

	private float currentZoom;

	private float maxZoom;

	private void Start()
	{
		cam = Camera.main;
		currentZoom = cam.fieldOfView;
		maxZoom = currentZoom - clamp.x;
	}

	private void Update()
	{
		if (zoomPointer.fillAmount <= 0f)
		{
			zoomBarParent.gameObject.SetActive(value: false);
		}
		else
		{
			zoomBarParent.gameObject.SetActive(value: true);
		}
		zoomPointer.fillAmount = 1f - (currentZoom - clamp.x) / maxZoom;
		if (Input.GetKey(KeyCode.Mouse1) && currentZoom > clamp.x)
		{
			currentZoom -= Time.deltaTime * speed;
			cam.fieldOfView = Mathf.Clamp(currentZoom, clamp.x, clamp.y);
			iconCam.fieldOfView = Mathf.Clamp(currentZoom, clamp.x, clamp.y);
			PlayAudio();
		}
		else if (!Input.GetKey(KeyCode.Mouse1) && currentZoom < clamp.y)
		{
			currentZoom += Time.deltaTime * speed;
			cam.fieldOfView = Mathf.Clamp(currentZoom, clamp.x, clamp.y);
			iconCam.fieldOfView = Mathf.Clamp(currentZoom, clamp.x, clamp.y);
			PlayAudio();
		}
		else
		{
			zoomAS.Stop();
		}
	}

	private void PlayAudio()
	{
		if (!zoomAS.isPlaying)
		{
			zoomAS.Play();
		}
	}
}
