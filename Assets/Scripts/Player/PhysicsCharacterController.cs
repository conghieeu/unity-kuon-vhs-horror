using UnityEngine;

public class PhysicsCharacterController : MonoBehaviour
{
	private Animator animator;

	public CharacterController physicsController;

	public bool wasAttacking;

	public bool stop = true;

	private float rotateSpeed = 50f;

	public Vector3 movementTargetPosition;

	public Vector3 attackPos;

	public Vector3 lookAtPos;

	private float gravity = 5f;

	public CarModelControl[] cars;

	public CarModelControl targetCar;

	public bool usingCar;

	public bool insideVehicle;

	public int entryPoint;

	private RaycastHit hit;

	private Ray ray;

	public bool rightButtonDown;

	private void Start()
	{
		physicsController = GetComponent<CharacterController>();
		animator = GetComponentInChildren<Animator>();
		movementTargetPosition = base.transform.position;
		lookAtPos = base.transform.position + base.transform.forward;
		cars = Object.FindObjectsByType<CarModelControl>(FindObjectsSortMode.None);
	}

	private void LateUpdate()
	{
		if (!Input.GetKey(KeyCode.LeftAlt) && Input.GetMouseButton(0))
		{
			ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			if (Physics.Raycast(ray, out hit, 500f))
			{
				movementTargetPosition = hit.point;
				Vector3 vector = movementTargetPosition - base.transform.position;
				lookAtPos = movementTargetPosition + vector.normalized * 2f;
				stop = false;
			}
		}
		if (!Input.GetKey(KeyCode.LeftAlt) && Input.GetMouseButton(1) && !rightButtonDown)
		{
			if (!insideVehicle)
			{
				ray = Camera.main.ScreenPointToRay(Input.mousePosition);
				for (int i = 0; i < cars.Length; i++)
				{
					if (!cars[i].coll.Raycast(ray, out hit, 500f))
					{
						continue;
					}
					targetCar = cars[i];
					entryPoint = 0;
					for (int j = 0; j < cars[i].entryPoints.Length; j++)
					{
						if (cars[i].entryPoints[j].entryPoint != cars[i].entryPoints[entryPoint].entryPoint && Vector3.Distance(base.transform.position, cars[i].entryPoints[j].entryPoint.position) < Vector3.Distance(cars[i].entryPoints[entryPoint].entryPoint.position, base.transform.position))
						{
							entryPoint = j;
						}
					}
					if (Vector3.Distance(base.transform.position, targetCar.entryPoints[entryPoint].entryPoint.position) > 0.5f)
					{
						movementTargetPosition = targetCar.entryPoints[entryPoint].entryPoint.position;
						Vector3 vector2 = movementTargetPosition - base.transform.position;
						lookAtPos = movementTargetPosition + vector2.normalized * 2f;
						stop = false;
					}
					else
					{
						base.transform.position = targetCar.entryPoints[entryPoint].entryPoint.position;
						base.transform.rotation = targetCar.entryPoints[entryPoint].entryPoint.rotation;
						movementTargetPosition = targetCar.entryPoints[entryPoint].entryPoint.position;
						lookAtPos = movementTargetPosition + targetCar.entryPoints[entryPoint].entryPoint.forward;
						stop = true;
					}
					usingCar = true;
				}
			}
			else
			{
				animator.SetBool("GetIn", value: false);
				if (targetCar.entryPoints[entryPoint].hasDoor && targetCar.entryPoints[entryPoint].animator != null)
				{
					targetCar.entryPoints[entryPoint].animator.SetBool("GetIn", value: false);
				}
			}
			rightButtonDown = true;
		}
		if (Input.GetMouseButtonUp(1) && rightButtonDown)
		{
			rightButtonDown = false;
		}
		if (insideVehicle)
		{
			return;
		}
		Debug.DrawLine(movementTargetPosition + base.transform.up * 2f, movementTargetPosition);
		if ((Vector3.Distance(movementTargetPosition, base.transform.position) > 1f) & !stop)
		{
			animator.SetBool("Idling", value: false);
			lookAtPos.y = base.transform.position.y;
			Quaternion rotation = base.transform.rotation;
			base.transform.LookAt(lookAtPos);
			Quaternion rotation2 = base.transform.rotation;
			base.transform.rotation = Quaternion.Slerp(rotation, rotation2, Time.deltaTime * rotateSpeed);
		}
		else
		{
			animator.SetBool("Idling", value: true);
			stop = true;
			if (usingCar)
			{
				base.transform.position = targetCar.entryPoints[entryPoint].entryPoint.position;
				physicsController.enabled = false;
				lookAtPos = base.transform.position + targetCar.entryPoints[entryPoint].entryPoint.forward;
				lookAtPos.y = base.transform.position.y;
				base.transform.LookAt(lookAtPos);
				movementTargetPosition = base.transform.position + targetCar.entryPoints[entryPoint].entryPoint.forward * 0.08f;
				base.transform.position = targetCar.entryPoints[entryPoint].entryPoint.position;
				base.transform.parent = targetCar.transform;
				usingCar = false;
				insideVehicle = true;
				animator.SetInteger("VehicleType", targetCar.vehicleType);
				animator.SetBool("GetIn", value: true);
				animator.SetFloat("SeatHeight", targetCar.entryPoints[entryPoint].seatHeight);
				if (targetCar.entryPoints[entryPoint].leftSide)
				{
					animator.SetBool("Left", value: true);
				}
				else
				{
					animator.SetBool("Left", value: false);
				}
				if (targetCar.entryPoints[entryPoint].hasDoor)
				{
					if (targetCar.entryPoints[entryPoint].animator != null)
					{
						targetCar.entryPoints[entryPoint].animator.SetBool("GetIn", value: true);
					}
					else
					{
						targetCar.entryPoints[entryPoint].enter = true;
						targetCar.entryPoints[entryPoint].startTime = Time.time;
					}
				}
				if (targetCar.entryPoints[entryPoint].driver)
				{
					targetCar.carMotor.hasDriver = true;
					animator.SetBool("Driver", value: true);
				}
				else
				{
					animator.SetBool("Driver", value: false);
				}
			}
			else
			{
				stop = true;
				lookAtPos = base.transform.position + base.transform.forward;
				base.transform.LookAt(lookAtPos);
				movementTargetPosition = base.transform.position;
			}
		}
		physicsController.Move(Vector3.down * gravity * Time.deltaTime);
	}

	private void OnGUI()
	{
	}
}
