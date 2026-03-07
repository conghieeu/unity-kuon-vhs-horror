using UnityEngine;

namespace IllusionLoop
{
	[HelpURL("http://www.illusionloop.com/docs")]
	[SelectionBase]
	public class CanalGateController : MonoBehaviour
	{
		[Tooltip("The gate object, that moves up and down")]
		public Transform gate;

		[Tooltip("The screw(handle) object, that rotates to move the gate")]
		public Transform screw;

		[Tooltip("Audio source to play the gate sound")]
		public AudioSource audioGate;

		[Tooltip("Audio source to play the wheel sound")]
		public AudioSource audioWheel;

		[Tooltip("Audio source plays when gate reached the top")]
		public AudioSource audioHit;

		[Tooltip("Speed of the gate in m/s")]
		[Range(0.001f, 10f)]
		public float animationSpeed = 0.1f;

		[Tooltip("Rotation speed of the screw. Scales with animationSpeed")]
		[Range(0.001f, 60f)]
		public float screwSpeed = 25f;

		[Tooltip("Local y position of the gate object, when it is closed")]
		public float closedPosition = 1f;

		[Tooltip("Local y position of the gate object, when it is open")]
		public float openPosition = 2f;

		[Tooltip("Automatically deactivates this script, if nothing moves")]
		public bool allowSleep = true;

		private float targetPosition;

		private float defaultVolumeGate = 0.3f;

		private float defaultVolumeWheel = 0.3f;

		private float currentVolume;

		public float TargetPosition
		{
			get
			{
				return targetPosition;
			}
			set
			{
				targetPosition = Mathf.Clamp01(value);
				if (allowSleep)
				{
					base.enabled = true;
				}
			}
		}

		private void Reset()
		{
			Transform[] componentsInChildren = base.transform.GetComponentsInChildren<Transform>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				if (componentsInChildren[i] != base.transform)
				{
					if (gate == null && componentsInChildren[i].name.Contains("Gate"))
					{
						gate = componentsInChildren[i];
					}
					else if (screw == null && componentsInChildren[i].name.Contains("Screw"))
					{
						screw = componentsInChildren[i];
					}
					if (gate != null && screw != null)
					{
						break;
					}
				}
			}
			AudioSource[] componentsInChildren2 = base.transform.GetComponentsInChildren<AudioSource>();
			for (int j = 0; j < componentsInChildren2.Length; j++)
			{
				if (audioGate == null && componentsInChildren2[j].name.Contains("Gate"))
				{
					audioGate = componentsInChildren2[j];
					defaultVolumeGate = audioGate.volume;
				}
				if (audioWheel == null && componentsInChildren2[j].name.Contains("Wheel"))
				{
					audioWheel = componentsInChildren2[j];
					defaultVolumeWheel = audioWheel.volume;
				}
				if (audioHit == null && componentsInChildren2[j].name.Contains("Hit"))
				{
					audioHit = componentsInChildren2[j];
				}
				if (audioGate != null && audioWheel != null && audioHit != null)
				{
					break;
				}
			}
			openPosition = 2f;
			closedPosition = 1f;
			if (gate != null)
			{
				float y = gate.localPosition.y;
				targetPosition = GetRelativeGatePosition();
				if (targetPosition > 1f)
				{
					openPosition = y;
					closedPosition = y - 1f;
					targetPosition = 1f;
				}
				else if (targetPosition < 0f)
				{
					openPosition = y + 1f;
					closedPosition = y;
					targetPosition = 0f;
				}
			}
		}

		private void Start()
		{
			targetPosition = GetRelativeGatePosition();
		}

		public void Open()
		{
			TargetPosition = 1f;
		}

		public void Close()
		{
			TargetPosition = 0f;
		}

		public void Stop()
		{
			TargetPosition = GetRelativeGatePosition();
		}

		public void Snap(float relativePosition)
		{
			targetPosition = Mathf.Clamp01(relativePosition);
			SetRelativeGatePosition(targetPosition);
		}

		private void Update()
		{
			if (!(gate != null))
			{
				return;
			}
			bool flag = false;
			float y = gate.localPosition.y;
			float relativeGatePosition = GetRelativeGatePosition();
			if (relativeGatePosition < targetPosition)
			{
				flag = true;
				gate.localPosition += Vector3.up * animationSpeed * Time.deltaTime;
				if (GetRelativeGatePosition() > targetPosition)
				{
					SetRelativeGatePosition(targetPosition);
					flag = false;
				}
			}
			else if (relativeGatePosition > targetPosition)
			{
				flag = true;
				gate.localPosition -= Vector3.up * animationSpeed * Time.deltaTime;
				if (GetRelativeGatePosition() < targetPosition)
				{
					SetRelativeGatePosition(targetPosition);
					flag = false;
				}
			}
			if (screw != null)
			{
				screw.localRotation *= Quaternion.Euler(0f, (gate.localPosition.y - y) * screwSpeed * 360f, 0f);
			}
			if (allowSleep && GetRelativeGatePosition() == targetPosition)
			{
				base.enabled = false;
			}
			if (flag)
			{
				currentVolume = Mathf.Lerp(currentVolume, 1f, 0.1f);
			}
			else
			{
				if (GetRelativeGatePosition() == 1f && currentVolume > 0f)
				{
					PlayAudioHit();
				}
				currentVolume = 0f;
			}
			PlayAudio(currentVolume);
		}

		private void PlayAudio(float volume)
		{
			if (audioWheel != null && audioGate != null)
			{
				if (!audioWheel.isPlaying && volume > 0f)
				{
					audioWheel.Play();
					audioGate.Play();
				}
				else if (volume <= 0f)
				{
					audioWheel.Stop();
					audioGate.Stop();
				}
				if (volume > 0f)
				{
					audioWheel.volume = volume * defaultVolumeWheel;
					audioGate.volume = volume * defaultVolumeGate;
				}
			}
		}

		private void PlayAudioHit()
		{
			if (audioHit != null && !audioHit.isPlaying)
			{
				audioHit.Play();
			}
		}

		public float GetRelativeGatePosition()
		{
			if (gate != null)
			{
				return (gate.localPosition.y - closedPosition) / (openPosition - closedPosition);
			}
			return targetPosition;
		}

		private void SetRelativeGatePosition(float relativePosition)
		{
			if (gate != null)
			{
				float y = closedPosition + (openPosition - closedPosition) * relativePosition;
				gate.localPosition = new Vector3(gate.localPosition.x, y, gate.localPosition.z);
			}
		}
	}
}
