using System;
using UnityEngine;

[Serializable]
public class EntryPoint
{
	public Transform entryPoint;

	public float seatHeight;

	public bool driver;

	public bool leftSide;

	public bool hasDoor;

	public Animator animator;

	public Transform doorModel;

	public Vector3 hingeAxis = Vector3.up;

	public AnimationCurve enterCurve;

	public AnimationCurve exitCurve;

	public bool enter;

	public bool exit;

	public float startTime;

	public float curveDuration;
}
