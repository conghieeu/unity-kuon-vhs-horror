using UnityEngine;

public class CarMotor : MonoBehaviour
{
	public Vector3 angularVelocity;

	public Vector3 centerOfMass;

	public float velocity;

	public float RBVelocity;

	public float jumpDistance;

	private Vector3 jumpStart;

	public bool wheelsUp;

	public Wheel[] wheels;

	public Rigidbody body;

	public float steerAngle;

	public float maxEngineTorque;

	public float currentTorque;

	public GameObject rampWheel;

	public Transform[] ramps;

	private Vector3 lastPosition;

	private float lastTime;

	public bool parked = true;

	public bool hasDriver;

	private void Start()
	{
		Physics.gravity = new Vector3(0f, -20.81f, 0f);
		Transform[] array = Object.FindObjectsOfType<Transform>();
		int num = 0;
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i].name.ToLower().Contains("ramp"))
			{
				num++;
			}
		}
		ramps = new Transform[num];
		num = 0;
		for (int j = 0; j < array.Length; j++)
		{
			if (array[j].name.ToLower().Contains("ramp"))
			{
				ramps[num] = array[j];
				num++;
			}
		}
		lastTime = Time.fixedTime;
		lastPosition = base.transform.position;
		body.centerOfMass = centerOfMass;
		Skidmarks component = GameObject.Find("Skidmarks").GetComponent<Skidmarks>();
		for (int k = 0; k < wheels.Length; k++)
		{
			wheels[k].lastPosition = wheels[k].collider.transform.position;
			wheels[k].lastSkid = -1;
			if (component != null)
			{
				wheels[k].skidmark = component;
			}
		}
		rampWheel.SetActive(value: true);
		rampWheel.SetActive(value: false);
	}

	private void FixedUpdate()
	{
		if (parked)
		{
			for (int i = 0; i < wheels.Length; i++)
			{
				wheels[i].collider.brakeTorque = 350000f;
			}
		}
		if (!hasDriver)
		{
			return;
		}
		angularVelocity = body.angularVelocity;
		steerAngle = 0f;
		if (Input.GetKey(KeyCode.LeftArrow))
		{
			steerAngle = -20f;
		}
		if (Input.GetKey(KeyCode.RightArrow))
		{
			steerAngle = 20f;
		}
		float num = Time.fixedTime - lastTime;
		float num2 = Vector3.Distance(base.transform.position, lastPosition);
		lastTime = Time.fixedTime;
		lastPosition = base.transform.position;
		velocity = 1f / num * num2 * 2.2369363f;
		RBVelocity = 1f / num * body.velocity.magnitude / 50f * 2.2369363f;
		wheelsUp = true;
		for (int j = 0; j < wheels.Length; j++)
		{
			if (wheels[j].drive)
			{
				wheels[j].collider.motorTorque = currentTorque;
			}
			if (wheels[j].steer)
			{
				wheels[j].collider.steerAngle = steerAngle;
			}
			if (!parked)
			{
				Vector3 pos = wheels[j].wheelModel.position;
				Quaternion quat = wheels[j].wheelModel.rotation;
				wheels[j].collider.GetWorldPose(out pos, out quat);
				wheels[j].wheelModel.position = pos;
				wheels[j].wheelModel.rotation = quat;
			}
			if (!(wheels[j].skidmark != null))
			{
				continue;
			}
			if (wheels[j].collider.isGrounded)
			{
				wheelsUp = false;
				wheels[j].collider.GetGroundHit(out wheels[j].hit);
				Vector3 normalized = (wheels[j].lastPosition - wheels[j].collider.transform.position).normalized;
				if (Mathf.Abs(Vector3.Dot(wheels[j].collider.transform.forward, normalized)) < 0.98f)
				{
					wheels[j].skidding = true;
					wheels[j].lastSkid = wheels[j].skidmark.AddSkidMark(wheels[j].hit.point, wheels[j].hit.normal, 0.8f, wheels[j].lastSkid);
				}
				else
				{
					wheels[j].skidding = false;
					wheels[j].lastSkid = -1;
				}
				wheels[j].lastPosition = wheels[j].collider.transform.position;
			}
			else
			{
				wheels[j].skidding = false;
				wheels[j].lastSkid = -1;
			}
		}
		if (ramps != null)
		{
			for (int k = 0; k < ramps.Length; k++)
			{
				if (ramps[k] != null)
				{
					if (Vector3.Distance(ramps[k].position, base.transform.position) < 20f)
					{
						rampWheel.SetActive(value: true);
					}
					else
					{
						rampWheel.SetActive(value: false);
					}
					if (Vector3.Distance(ramps[k].position, base.transform.position) < 2f)
					{
						body.AddForceAtPosition(base.transform.up * 100f, base.transform.forward * 4f);
					}
				}
			}
		}
		if (!wheelsUp)
		{
			currentTorque = 0f;
			if (Input.GetKey(KeyCode.UpArrow))
			{
				body.AddForce(base.transform.forward * 200000f);
				currentTorque = 20000f;
				for (int l = 0; l < wheels.Length; l++)
				{
					wheels[l].collider.brakeTorque = 0f;
				}
				parked = false;
			}
			if (Input.GetKey(KeyCode.DownArrow))
			{
				body.AddForce(-base.transform.forward * 200000f);
				currentTorque = -20000f;
				for (int m = 0; m < wheels.Length; m++)
				{
					wheels[m].collider.brakeTorque = 0f;
				}
				parked = false;
			}
			if (Input.GetKey(KeyCode.B))
			{
				for (int n = 0; n < wheels.Length; n++)
				{
					WheelFrictionCurve sidewaysFriction = wheels[n].collider.sidewaysFriction;
					sidewaysFriction.stiffness = 0.5f;
					if (!wheels[n].steer)
					{
						wheels[n].collider.sidewaysFriction = sidewaysFriction;
						wheels[n].collider.brakeTorque = 35000f;
					}
				}
			}
			else
			{
				for (int num3 = 0; num3 < wheels.Length; num3++)
				{
				}
			}
			if (steerAngle > 1f)
			{
				body.AddRelativeTorque(0f, 0f, -2000f * (velocity + 1f));
				body.AddForce(base.transform.right * 2000f * (velocity + 1f));
			}
			if (steerAngle < -1f)
			{
				body.AddRelativeTorque(0f, 0f, 2000f * (velocity + 1f));
				body.AddForce(-base.transform.right * 2000f * (velocity + 1f));
			}
			for (int num4 = 0; num4 < wheels.Length; num4++)
			{
				if (!wheels[num4].collider.isGrounded)
				{
					body.AddForceAtPosition(base.transform.up * 2000f, wheels[num4].collider.transform.position);
				}
			}
			jumpStart = base.transform.position;
		}
		else
		{
			if (steerAngle > 1f)
			{
				body.AddRelativeTorque(0f, 0f, -20000f);
			}
			if (steerAngle < -1f)
			{
				body.AddRelativeTorque(0f, 0f, 20000f);
			}
			if (Input.GetKey(KeyCode.Space))
			{
				body.AddForceAtPosition(base.transform.up * 20000f, base.transform.right * 4f);
			}
			jumpDistance = Vector3.Distance(base.transform.position, jumpStart);
		}
	}
}
