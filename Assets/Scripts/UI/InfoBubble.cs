using UnityEngine;

public class InfoBubble : MonoBehaviour
{
	public Vector3 WobbleAxis = Vector3.one;

	public float WobbleFrequency = 1f;

	public float WobbleAmplitude = 0.25f;

	public Transform TrackTarget;

	private Vector3 startOffsetTarget;

	private void Start()
	{
		startOffsetTarget = base.transform.position - TrackTarget.position;
	}

	private void Update()
	{
		Vector3 eulers = Mathf.Sin(WobbleFrequency * Time.timeSinceLevelLoad) * WobbleAxis * WobbleAmplitude;
		base.transform.Rotate(eulers);
		base.transform.position = TrackTarget.position + startOffsetTarget;
	}
}
