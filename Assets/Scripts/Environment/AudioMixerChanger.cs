using UnityEngine;
using UnityEngine.Audio;

public class AudioMixerChanger : MonoBehaviour
{
	public AudioMixerSnapshot exteriorAudioSS;

	public AudioMixerSnapshot indoorAudioSS;

	public void SwitchToExterior()
	{
		exteriorAudioSS.TransitionTo(0.01f);
	}

	public void SwitchToInDoor()
	{
		indoorAudioSS.TransitionTo(0.01f);
	}
}
