using UnityEngine;

public class CarModelControl : MonoBehaviour
{
	public int vehicleType;

	public EntryPoint[] entryPoints;

	public CarMotor carMotor;

	public MeshCollider collider;

	private void Start()
	{
		collider = GetComponent<MeshCollider>();
		carMotor = GetComponent<CarMotor>();
	}

	private void LateUpdate()
	{
		for (int i = 0; i < entryPoints.Length; i++)
		{
			if (!((entryPoints[i].animator == null) & entryPoints[i].hasDoor) || !(entryPoints[i].exit | entryPoints[i].enter))
			{
				continue;
			}
			if (entryPoints[i].startTime + entryPoints[i].curveDuration < Time.time)
			{
				entryPoints[i].enter = false;
				entryPoints[i].exit = false;
				continue;
			}
			AnimationCurve animationCurve = new AnimationCurve();
			if (entryPoints[i].enter)
			{
				animationCurve = entryPoints[i].enterCurve;
			}
			if (entryPoints[i].exit)
			{
				animationCurve = entryPoints[i].exitCurve;
			}
			entryPoints[i].doorModel.localRotation = Quaternion.AngleAxis(animationCurve.Evaluate(Time.time - entryPoints[i].startTime), entryPoints[i].hingeAxis);
		}
	}
}
