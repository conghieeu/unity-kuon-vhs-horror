using System.Collections.Generic;
using UnityEngine;

public class FlockController : MonoBehaviour
{
	public FlockChild _childPrefab;

	public int _childAmount = 250;

	public bool _slowSpawn;

	public float _spawnSphere = 3f;

	public float _spawnSphereHeight = 3f;

	public float _spawnSphereDepth = -1f;

	public float _minSpeed = 6f;

	public float _maxSpeed = 10f;

	public float _minScale = 0.7f;

	public float _maxScale = 1f;

	public float _soarFrequency;

	public string _soarAnimation = "Soar";

	public string _flapAnimation = "Flap";

	public string _idleAnimation = "Idle";

	public float _diveValue = 7f;

	public float _diveFrequency = 0.5f;

	public float _minDamping = 1f;

	public float _maxDamping = 2f;

	public float _waypointDistance = 1f;

	public float _minAnimationSpeed = 2f;

	public float _maxAnimationSpeed = 4f;

	public float _randomPositionTimer = 10f;

	public float _positionSphere = 25f;

	public float _positionSphereHeight = 25f;

	public float _positionSphereDepth = -1f;

	public bool _childTriggerPos;

	public bool _forceChildWaypoints;

	public float _forcedRandomDelay = 1.5f;

	public bool _flatFly;

	public bool _flatSoar;

	public bool _birdAvoid;

	public int _birdAvoidHorizontalForce = 1000;

	public bool _birdAvoidDown;

	public bool _birdAvoidUp;

	public int _birdAvoidVerticalForce = 300;

	public float _birdAvoidDistanceMax = 4.5f;

	public float _birdAvoidDistanceMin = 5f;

	public float _soarMaxTime;

	public LayerMask _avoidanceMask = -1;

	public List<FlockChild> _roamers;

	public Vector3 _posBuffer;

	public int _updateDivisor = 1;

	public float _newDelta;

	public int _updateCounter;

	public float _activeChildren;

	public bool _groupChildToNewTransform;

	public Transform _groupTransform;

	public string _groupName = "";

	public bool _groupChildToFlock;

	public Vector3 _startPosOffset;

	public Transform _thisT;

	public void Start()
	{
		_thisT = base.transform;
		if (_positionSphereDepth == -1f)
		{
			_positionSphereDepth = _positionSphere;
		}
		if (_spawnSphereDepth == -1f)
		{
			_spawnSphereDepth = _spawnSphere;
		}
		_posBuffer = _thisT.position + _startPosOffset;
		if (!_slowSpawn)
		{
			AddChild(_childAmount);
		}
		if (_randomPositionTimer > 0f)
		{
			InvokeRepeating("SetFlockRandomPosition", _randomPositionTimer, _randomPositionTimer);
		}
	}

	public void AddChild(int amount)
	{
		if (_groupChildToNewTransform)
		{
			InstantiateGroup();
		}
		for (int i = 0; i < amount; i++)
		{
			FlockChild flockChild = Object.Instantiate(_childPrefab);
			flockChild._spawner = this;
			_roamers.Add(flockChild);
			AddChildToParent(flockChild.transform);
		}
	}

	public void AddChildToParent(Transform obj)
	{
		if (_groupChildToFlock)
		{
			obj.parent = base.transform;
		}
		else if (_groupChildToNewTransform)
		{
			obj.parent = _groupTransform;
		}
	}

	public void RemoveChild(int amount)
	{
		for (int i = 0; i < amount; i++)
		{
			FlockChild flockChild = _roamers[_roamers.Count - 1];
			_roamers.RemoveAt(_roamers.Count - 1);
			Object.Destroy(flockChild.gameObject);
		}
	}

	public void Update()
	{
		if (_activeChildren > 0f)
		{
			if (_updateDivisor > 1)
			{
				_updateCounter++;
				_updateCounter %= _updateDivisor;
				_newDelta = Time.deltaTime * (float)_updateDivisor;
			}
			else
			{
				_newDelta = Time.deltaTime;
			}
		}
		UpdateChildAmount();
	}

	public void InstantiateGroup()
	{
		if (!(_groupTransform != null))
		{
			GameObject gameObject = new GameObject();
			_groupTransform = gameObject.transform;
			_groupTransform.position = _thisT.position;
			if (_groupName != "")
			{
				gameObject.name = _groupName;
			}
			else
			{
				gameObject.name = _thisT.name + " Fish Container";
			}
		}
	}

	public void UpdateChildAmount()
	{
		if (_childAmount >= 0 && _childAmount < _roamers.Count)
		{
			RemoveChild(1);
		}
		else if (_childAmount > _roamers.Count)
		{
			AddChild(1);
		}
	}

	public void OnDrawGizmos()
	{
		if (_thisT == null)
		{
			_thisT = base.transform;
		}
		if (!Application.isPlaying && _posBuffer != _thisT.position + _startPosOffset)
		{
			_posBuffer = _thisT.position + _startPosOffset;
		}
		if (_positionSphereDepth == -1f)
		{
			_positionSphereDepth = _positionSphere;
		}
		if (_spawnSphereDepth == -1f)
		{
			_spawnSphereDepth = _spawnSphere;
		}
		Gizmos.color = Color.blue;
		Gizmos.DrawWireCube(_posBuffer, new Vector3(_spawnSphere * 2f, _spawnSphereHeight * 2f, _spawnSphereDepth * 2f));
		Gizmos.color = Color.cyan;
		Gizmos.DrawWireCube(_thisT.position, new Vector3(_positionSphere * 2f + _spawnSphere * 2f, _positionSphereHeight * 2f + _spawnSphereHeight * 2f, _positionSphereDepth * 2f + _spawnSphereDepth * 2f));
	}

	public void SetFlockRandomPosition()
	{
		Vector3 zero = Vector3.zero;
		zero.x = Random.Range(0f - _positionSphere, _positionSphere) + _thisT.position.x;
		zero.z = Random.Range(0f - _positionSphereDepth, _positionSphereDepth) + _thisT.position.z;
		zero.y = Random.Range(0f - _positionSphereHeight, _positionSphereHeight) + _thisT.position.y;
		_posBuffer = zero;
		if (_forceChildWaypoints)
		{
			for (int i = 0; i < _roamers.Count; i++)
			{
				_roamers[i].Wander(Random.value * _forcedRandomDelay);
			}
		}
	}

	public void destroyBirds()
	{
		for (int i = 0; i < _roamers.Count; i++)
		{
			Object.Destroy(_roamers[i].gameObject);
		}
		_childAmount = 0;
		_roamers.Clear();
	}
}
