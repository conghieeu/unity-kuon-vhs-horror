using Colorful;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using StarterAssets;

public class MenuManager : MonoBehaviour
{
	[Header("UI Elements")]
	public GameObject menu;

	public Slider mouseSensitivity;

	public Slider volume;

	public Slider brightness;

	public Toggle vhs;

	public Toggle headbob;

	private int vhsToggleInt = 1;

	private int headbobToggleInt = 1;

	[Header("Related Components")]
	public postVHSPro vhsPro;

	[Header("On Menu Close")]
	public UnityEvent MenuClose;

	private FirstPersonController fpsController;

	private BrightnessContrastGamma brightnessFX;

	private void Awake()
	{
		fpsController = FindFirstObjectByType<FirstPersonController>();
		brightnessFX = FindFirstObjectByType<BrightnessContrastGamma>();
		GetPlayerPref();
	}

	private void GetPlayerPref()
	{
		mouseSensitivity.value = PlayerPrefs.GetFloat("mouseSensitivity", 1f);
		volume.value = PlayerPrefs.GetFloat("volume", 1f);
		brightness.value = PlayerPrefs.GetFloat("brightness", 1f);
		vhsToggleInt = PlayerPrefs.GetInt("vhs", 1);
		headbobToggleInt = PlayerPrefs.GetInt("headbob", 1);
		if (vhsToggleInt == 1)
		{
			vhs.isOn = true;
		}
		else
		{
			vhs.isOn = false;
		}
		if (headbobToggleInt == 1)
		{
			headbob.isOn = true;
		}
		else
		{
			headbob.isOn = false;
		}
	}

	public void SetToDefault()
	{
		mouseSensitivity.value = 1f;
		volume.value = 1f;
		brightness.value = 1f;
		vhsToggleInt = 1;
		headbobToggleInt = 1;
		vhs.isOn = true;
		headbob.isOn = true;
		SavePlayerPref();
	}

	private void SavePlayerPref()
	{
		PlayerPrefs.SetFloat("mouseSensitivity", mouseSensitivity.value);
		PlayerPrefs.SetFloat("volume", volume.value);
		PlayerPrefs.SetFloat("brightness", brightness.value);
		PlayerPrefs.SetInt("vhs", vhsToggleInt);
		PlayerPrefs.SetInt("headbob", vhsToggleInt);
	}

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.Escape))
		{
			if (!fpsController.enabled && !menu.activeSelf)
			{
				return;
			}
			menu.SetActive(!menu.activeSelf);
			fpsController.enabled = false;
			Cursor.lockState = CursorLockMode.None;
			Cursor.visible = true;
			if (!menu.activeSelf)
			{
				MenuClose.Invoke();
				Cursor.lockState = CursorLockMode.Locked;
				Cursor.visible = false;
				fpsController.enabled = true;
				SavePlayerPref();
			}
		}
		if (vhs.isOn)
		{
			vhsPro.filmgrainOn = true;
			vhsPro.vignetteOn = true;
			vhsPro.fisheyeOn = true;
			vhsPro.lineNoiseOn = true;
			vhsPro.tapeNoiseOn = true;
			vhsToggleInt = 1;
		}
		else
		{
			vhsPro.filmgrainOn = false;
			vhsPro.vignetteOn = false;
			vhsPro.fisheyeOn = false;
			vhsPro.lineNoiseOn = false;
			vhsPro.tapeNoiseOn = false;
			vhsToggleInt = 0;
		}
		if (headbob.isOn)
		{
			fpsController.m_UseHeadBob = true;
			headbobToggleInt = 1;
		}
		else
		{
			fpsController.m_UseHeadBob = false;
			headbobToggleInt = 0;
		}
		fpsController.m_MouseLook.XSensitivity = mouseSensitivity.value;
		fpsController.m_MouseLook.YSensitivity = mouseSensitivity.value;
		AudioListener.volume = volume.value;
		brightnessFX.Brightness = brightness.value;
	}
}
