using System;
using UnityEngine;

[Serializable]
public class Wheel
{
	public WheelCollider collider;

	public Transform wheelModel;

	public bool drive;

	public bool powerBrake;

	public bool fullBrake;

	public bool steer;

	public Skidmarks skidmark;

	public WheelHit hit;

	public Vector3 lastPosition;

	public int lastSkid;

	public bool skidding;
}
