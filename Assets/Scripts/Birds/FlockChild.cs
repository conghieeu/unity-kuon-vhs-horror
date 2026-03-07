using UnityEngine;

public class FlockChild : MonoBehaviour
{
	[HideInInspector]
	public FlockController _spawner;

	[HideInInspector]
	public Vector3 _wayPoint;

	public float _speed;

	[HideInInspector]
	public bool _dived = true;

	[HideInInspector]
	public float _stuckCounter;

	[HideInInspector]
	public float _damping;

	[HideInInspector]
	public bool _soar = true;

	[HideInInspector]
	public bool _landing;

	[HideInInspector]
	public float _targetSpeed;

	[HideInInspector]
	public bool _move = true;

	public GameObject _model;

	public Transform _modelT;

	[HideInInspector]
	public float _avoidValue;

	[HideInInspector]
	public float _avoidDistance;

	private float _soarTimer;

	private bool _instantiated;

	private static int _updateNextSeed;

	private int _updateSeed = -1;

	[HideInInspector]
	public bool _avoid = true;

	public Transform _thisT;

	public Vector3 _landingPosOffset;

	public void Start()
	{
		FindRequiredComponents();
		Wander(0f);
		SetRandomScale();
		_thisT.position = findWaypoint();
		RandomizeStartAnimationFrame();
		InitAvoidanceValues();
		_speed = _spawner._minSpeed;
		_spawner._activeChildren += 1f;
		_instantiated = true;
		if (_spawner._updateDivisor > 1)
		{
			int num = _spawner._updateDivisor - 1;
			_updateNextSeed++;
			_updateSeed = _updateNextSeed;
			_updateNextSeed %= num;
		}
	}

	public void Update()
	{
		if (_spawner._updateDivisor <= 1 || _spawner._updateCounter == _updateSeed)
		{
			SoarTimeLimit();
			CheckForDistanceToWaypoint();
			RotationBasedOnWaypointOrAvoidance();
			LimitRotationOfModel();
		}
	}

	public void OnDisable()
	{
		CancelInvoke();
		_spawner._activeChildren -= 1f;
	}

	public void OnEnable()
	{
		if (_instantiated)
		{
			_spawner._activeChildren += 1f;
			if (_landing)
			{
				_model.GetComponent<Animation>().Play(_spawner._idleAnimation);
			}
			else
			{
				_model.GetComponent<Animation>().Play(_spawner._flapAnimation);
			}
		}
	}

	public void FindRequiredComponents()
	{
		if (_thisT == null)
		{
			_thisT = base.transform;
		}
		if (_model == null)
		{
			_model = _thisT.Find("Model").gameObject;
		}
		if (_modelT == null)
		{
			_modelT = _model.transform;
		}
	}

	public void RandomizeStartAnimationFrame()
	{
		foreach (AnimationState item in _model.GetComponent<Animation>())
		{
			item.time = Random.value * item.length;
		}
	}

	public void InitAvoidanceValues()
	{
		_avoidValue = Random.Range(0.3f, 0.1f);
		if (_spawner._birdAvoidDistanceMax != _spawner._birdAvoidDistanceMin)
		{
			_avoidDistance = Random.Range(_spawner._birdAvoidDistanceMax, _spawner._birdAvoidDistanceMin);
		}
		else
		{
			_avoidDistance = _spawner._birdAvoidDistanceMin;
		}
	}

	public void SetRandomScale()
	{
		float num = Random.Range(_spawner._minScale, _spawner._maxScale);
		_thisT.localScale = new Vector3(num, num, num);
	}

	public void SoarTimeLimit()
	{
		if (_soar && _spawner._soarMaxTime > 0f)
		{
			if (_soarTimer > _spawner._soarMaxTime)
			{
				Flap();
				_soarTimer = 0f;
			}
			else
			{
				_soarTimer += _spawner._newDelta;
			}
		}
	}

	public void CheckForDistanceToWaypoint()
	{
		if (!_landing && (_thisT.position - _wayPoint).magnitude < _spawner._waypointDistance + _stuckCounter)
		{
			Wander(0f);
			_stuckCounter = 0f;
		}
		else if (!_landing)
		{
			_stuckCounter += _spawner._newDelta;
		}
		else
		{
			_stuckCounter = 0f;
		}
	}

	public void RotationBasedOnWaypointOrAvoidance()
	{
		Vector3 vector = _wayPoint - _thisT.position;
		if (_targetSpeed > -1f && vector != Vector3.zero)
		{
			Quaternion b = Quaternion.LookRotation(vector);
			_thisT.rotation = Quaternion.Slerp(_thisT.rotation, b, _spawner._newDelta * _damping);
		}
		if (_spawner._childTriggerPos && (_thisT.position - _spawner._posBuffer).magnitude < 1f)
		{
			_spawner.SetFlockRandomPosition();
		}
		_speed = Mathf.Lerp(_speed, _targetSpeed, _spawner._newDelta * 2.5f);
		if (_move)
		{
			_thisT.position += _thisT.forward * _speed * _spawner._newDelta;
			if (_avoid && _spawner._birdAvoid)
			{
				Avoidance();
			}
		}
	}

	public bool Avoidance()
	{
		RaycastHit hitInfo = default(RaycastHit);
		Vector3 forward = _modelT.forward;
		bool result = false;
		Quaternion identity = Quaternion.identity;
		Vector3 zero = Vector3.zero;
		Vector3 zero2 = Vector3.zero;
		zero2 = _thisT.position;
		identity = _thisT.rotation;
		zero = _thisT.rotation.eulerAngles;
		if (Physics.Raycast(_thisT.position, forward + _modelT.right * _avoidValue, out hitInfo, _avoidDistance, _spawner._avoidanceMask))
		{
			zero.y -= (float)_spawner._birdAvoidHorizontalForce * _spawner._newDelta * _damping;
			identity.eulerAngles = zero;
			_thisT.rotation = identity;
			result = true;
		}
		else if (Physics.Raycast(_thisT.position, forward + _modelT.right * (0f - _avoidValue), out hitInfo, _avoidDistance, _spawner._avoidanceMask))
		{
			zero.y += (float)_spawner._birdAvoidHorizontalForce * _spawner._newDelta * _damping;
			identity.eulerAngles = zero;
			_thisT.rotation = identity;
			result = true;
		}
		if (_spawner._birdAvoidDown && !_landing && Physics.Raycast(_thisT.position, -Vector3.up, out hitInfo, _avoidDistance, _spawner._avoidanceMask))
		{
			zero.x -= (float)_spawner._birdAvoidVerticalForce * _spawner._newDelta * _damping;
			identity.eulerAngles = zero;
			_thisT.rotation = identity;
			zero2.y += (float)_spawner._birdAvoidVerticalForce * _spawner._newDelta * 0.01f;
			_thisT.position = zero2;
			result = true;
		}
		else if (_spawner._birdAvoidUp && !_landing && Physics.Raycast(_thisT.position, Vector3.up, out hitInfo, _avoidDistance, _spawner._avoidanceMask))
		{
			zero.x += (float)_spawner._birdAvoidVerticalForce * _spawner._newDelta * _damping;
			identity.eulerAngles = zero;
			_thisT.rotation = identity;
			zero2.y -= (float)_spawner._birdAvoidVerticalForce * _spawner._newDelta * 0.01f;
			_thisT.position = zero2;
			result = true;
		}
		return result;
	}

	public void LimitRotationOfModel()
	{
		Quaternion identity = Quaternion.identity;
		Vector3 zero = Vector3.zero;
		identity = _modelT.localRotation;
		zero = identity.eulerAngles;
		if ((((_soar && _spawner._flatSoar) || (_spawner._flatFly && !_soar)) && _wayPoint.y > _thisT.position.y) || _landing)
		{
			zero.x = Mathf.LerpAngle(_modelT.localEulerAngles.x, 0f - _thisT.localEulerAngles.x, _spawner._newDelta * 1.75f);
			identity.eulerAngles = zero;
			_modelT.localRotation = identity;
		}
		else
		{
			zero.x = Mathf.LerpAngle(_modelT.localEulerAngles.x, 0f, _spawner._newDelta * 1.75f);
			identity.eulerAngles = zero;
			_modelT.localRotation = identity;
		}
	}

	public void Wander(float delay)
	{
		if (!_landing)
		{
			_damping = Random.Range(_spawner._minDamping, _spawner._maxDamping);
			_targetSpeed = Random.Range(_spawner._minSpeed, _spawner._maxSpeed);
			Invoke("SetRandomMode", delay);
		}
	}

	public void SetRandomMode()
	{
		CancelInvoke("SetRandomMode");
		if (!_dived && Random.value < _spawner._soarFrequency)
		{
			Soar();
		}
		else if (!_dived && Random.value < _spawner._diveFrequency)
		{
			Dive();
		}
		else
		{
			Flap();
		}
	}

	public void Flap()
	{
		if (_move)
		{
			if (_model != null)
			{
				_model.GetComponent<Animation>().CrossFade(_spawner._flapAnimation, 0.5f);
			}
			_soar = false;
			animationSpeed();
			_wayPoint = findWaypoint();
			_dived = false;
		}
	}

	public Vector3 findWaypoint()
	{
		Vector3 zero = Vector3.zero;
		zero.x = Random.Range(0f - _spawner._spawnSphere, _spawner._spawnSphere) + _spawner._posBuffer.x;
		zero.z = Random.Range(0f - _spawner._spawnSphereDepth, _spawner._spawnSphereDepth) + _spawner._posBuffer.z;
		zero.y = Random.Range(0f - _spawner._spawnSphereHeight, _spawner._spawnSphereHeight) + _spawner._posBuffer.y;
		return zero;
	}

	public void Soar()
	{
		if (_move)
		{
			_model.GetComponent<Animation>().CrossFade(_spawner._soarAnimation, 1.5f);
			_wayPoint = findWaypoint();
			_soar = true;
		}
	}

	public void Dive()
	{
		if (_spawner._soarAnimation != null)
		{
			_model.GetComponent<Animation>().CrossFade(_spawner._soarAnimation, 1.5f);
		}
		else
		{
			foreach (AnimationState item in _model.GetComponent<Animation>())
			{
				if (_thisT.position.y < _wayPoint.y + 25f)
				{
					item.speed = 0.1f;
				}
			}
		}
		_wayPoint = findWaypoint();
		_wayPoint.y -= _spawner._diveValue;
		_dived = true;
	}

	public void animationSpeed()
	{
		foreach (AnimationState item in _model.GetComponent<Animation>())
		{
			if (!_dived && !_landing)
			{
				item.speed = Random.Range(_spawner._minAnimationSpeed, _spawner._maxAnimationSpeed);
			}
			else
			{
				item.speed = _spawner._maxAnimationSpeed;
			}
		}
	}
}
