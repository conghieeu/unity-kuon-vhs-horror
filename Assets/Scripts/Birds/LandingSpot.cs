using System.Collections;
using UnityEngine;

public class LandingSpot : MonoBehaviour
{
	[HideInInspector]
	public FlockChild landingChild;

	[HideInInspector]
	public bool landing;

	private int lerpCounter;

	[HideInInspector]
	public LandingSpotController _controller;

	private bool _idle;

	public Transform _thisT;

	public bool _gotcha;

	public void Start()
	{
		if (_thisT == null)
		{
			_thisT = base.transform;
		}
		if (_controller == null)
		{
			_controller = _thisT.parent.GetComponent<LandingSpotController>();
		}
		if (_controller._autoCatchDelay.x > 0f)
		{
			StartCoroutine(GetFlockChild(_controller._autoCatchDelay.x, _controller._autoCatchDelay.y));
		}
	}

	public void OnDrawGizmos()
	{
		if (_thisT == null)
		{
			_thisT = base.transform;
		}
		if (_controller == null)
		{
			_controller = _thisT.parent.GetComponent<LandingSpotController>();
		}
		Gizmos.color = Color.yellow;
		if (landingChild != null && landing)
		{
			Gizmos.DrawLine(_thisT.position, landingChild._thisT.position);
		}
		if (_thisT.rotation.eulerAngles.x != 0f || _thisT.rotation.eulerAngles.z != 0f)
		{
			_thisT.eulerAngles = new Vector3(0f, _thisT.eulerAngles.y, 0f);
		}
		Gizmos.DrawCube(new Vector3(_thisT.position.x, _thisT.position.y, _thisT.position.z), Vector3.one * _controller._gizmoSize);
		Gizmos.DrawCube(_thisT.position + _thisT.forward * _controller._gizmoSize, Vector3.one * _controller._gizmoSize * 0.5f);
		Gizmos.color = new Color(1f, 1f, 0f, 0.05f);
		Gizmos.DrawWireSphere(_thisT.position, _controller._maxBirdDistance);
	}

	public void LateUpdate()
	{
		if (landingChild == null)
		{
			_gotcha = false;
			_idle = false;
			lerpCounter = 0;
			return;
		}
		if (_gotcha)
		{
			landingChild.transform.position = _thisT.position + landingChild._landingPosOffset;
			RotateBird();
			return;
		}
		if (_controller._flock.gameObject.activeInHierarchy && landing && landingChild != null)
		{
			if (!landingChild.gameObject.activeInHierarchy)
			{
				Invoke("ReleaseFlockChild", 0f);
			}
			float num = Vector3.Distance(landingChild._thisT.position, _thisT.position + landingChild._landingPosOffset);
			if (num < 5f && num > 0.5f)
			{
				if (_controller._soarLand)
				{
					landingChild._model.GetComponent<Animation>().CrossFade(landingChild._spawner._soarAnimation, 0.5f);
					if (num < 2f)
					{
						landingChild._model.GetComponent<Animation>().CrossFade(landingChild._spawner._flapAnimation, 0.5f);
					}
				}
				landingChild._targetSpeed = landingChild._spawner._maxSpeed * _controller._landingSpeedModifier;
				landingChild._wayPoint = _thisT.position + landingChild._landingPosOffset;
				landingChild._damping = _controller._landingTurnSpeedModifier;
				landingChild._avoid = false;
			}
			else if (num <= 0.5f)
			{
				landingChild._wayPoint = _thisT.position + landingChild._landingPosOffset;
				if (num < _controller._snapLandDistance && !_idle)
				{
					_idle = true;
					landingChild._model.GetComponent<Animation>().CrossFade(landingChild._spawner._idleAnimation, 0.55f);
				}
				if (num > _controller._snapLandDistance)
				{
					landingChild._targetSpeed = landingChild._spawner._minSpeed * _controller._landingSpeedModifier;
					landingChild._thisT.position += (_thisT.position + landingChild._landingPosOffset - landingChild._thisT.position) * Time.deltaTime * landingChild._speed * _controller._landingSpeedModifier * 2f;
				}
				else
				{
					_gotcha = true;
				}
				landingChild._move = false;
				RotateBird();
			}
			else
			{
				landingChild._wayPoint = _thisT.position + landingChild._landingPosOffset;
			}
			landingChild._damping += 0.01f;
		}
		StraightenBird();
	}

	public void StraightenBird()
	{
		if (landingChild._thisT.eulerAngles.x != 0f)
		{
			Vector3 eulerAngles = landingChild._thisT.eulerAngles;
			eulerAngles.z = 0f;
			landingChild._thisT.eulerAngles = eulerAngles;
		}
	}

	public void RotateBird()
	{
		if (!_controller._randomRotate || !_idle)
		{
			lerpCounter++;
			Quaternion rotation = landingChild._thisT.rotation;
			Vector3 eulerAngles = rotation.eulerAngles;
			eulerAngles.y = Mathf.LerpAngle(landingChild._thisT.rotation.eulerAngles.y, _thisT.rotation.eulerAngles.y, (float)lerpCounter * Time.deltaTime * _controller._landedRotateSpeed);
			rotation.eulerAngles = eulerAngles;
			landingChild._thisT.rotation = rotation;
		}
	}

	public IEnumerator GetFlockChild(float minDelay, float maxDelay)
	{
		yield return new WaitForSeconds(Random.Range(minDelay, maxDelay));
		if (!_controller._flock.gameObject.activeInHierarchy || !(landingChild == null))
		{
			yield break;
		}
		FlockChild flockChild = null;
		for (int i = 0; i < _controller._flock._roamers.Count; i++)
		{
			FlockChild flockChild2 = _controller._flock._roamers[i];
			if (flockChild2._landing || flockChild2._dived)
			{
				continue;
			}
			if (!_controller._onlyBirdsAbove)
			{
				if (flockChild == null && _controller._maxBirdDistance > Vector3.Distance(flockChild2._thisT.position, _thisT.position) && _controller._minBirdDistance < Vector3.Distance(flockChild2._thisT.position, _thisT.position))
				{
					flockChild = flockChild2;
					if (!_controller._takeClosest)
					{
						break;
					}
				}
				else if (flockChild != null && Vector3.Distance(flockChild._thisT.position, _thisT.position) > Vector3.Distance(flockChild2._thisT.position, _thisT.position))
				{
					flockChild = flockChild2;
				}
			}
			else if (flockChild == null && flockChild2._thisT.position.y > _thisT.position.y && _controller._maxBirdDistance > Vector3.Distance(flockChild2._thisT.position, _thisT.position) && _controller._minBirdDistance < Vector3.Distance(flockChild2._thisT.position, _thisT.position))
			{
				flockChild = flockChild2;
				if (!_controller._takeClosest)
				{
					break;
				}
			}
			else if (flockChild != null && flockChild2._thisT.position.y > _thisT.position.y && Vector3.Distance(flockChild._thisT.position, _thisT.position) > Vector3.Distance(flockChild2._thisT.position, _thisT.position))
			{
				flockChild = flockChild2;
			}
		}
		if (flockChild != null)
		{
			landingChild = flockChild;
			landing = true;
			landingChild._landing = true;
			if (_controller._autoDismountDelay.x > 0f)
			{
				Invoke("ReleaseFlockChild", Random.Range(_controller._autoDismountDelay.x, _controller._autoDismountDelay.y));
			}
			_controller._activeLandingSpots++;
		}
		else if (_controller._autoCatchDelay.x > 0f)
		{
			StartCoroutine(GetFlockChild(_controller._autoCatchDelay.x, _controller._autoCatchDelay.y));
		}
	}

	public void InstantLand()
	{
		if (!_controller._flock.gameObject.activeInHierarchy || !(landingChild == null))
		{
			return;
		}
		FlockChild flockChild = null;
		for (int i = 0; i < _controller._flock._roamers.Count; i++)
		{
			FlockChild flockChild2 = _controller._flock._roamers[i];
			if (!flockChild2._landing && !flockChild2._dived)
			{
				flockChild = flockChild2;
			}
		}
		if (flockChild != null)
		{
			landingChild = flockChild;
			landing = true;
			_controller._activeLandingSpots++;
			landingChild._landing = true;
			landingChild._thisT.position = _thisT.position + landingChild._landingPosOffset;
			landingChild._model.GetComponent<Animation>().Play(landingChild._spawner._idleAnimation);
			landingChild._thisT.Rotate(Vector3.up, Random.Range(0f, 360f));
			if (_controller._autoDismountDelay.x > 0f)
			{
				Invoke("ReleaseFlockChild", Random.Range(_controller._autoDismountDelay.x, _controller._autoDismountDelay.y));
			}
		}
		else if (_controller._autoCatchDelay.x > 0f)
		{
			StartCoroutine(GetFlockChild(_controller._autoCatchDelay.x, _controller._autoCatchDelay.y));
		}
	}

	public void ReleaseFlockChild()
	{
		if (_controller._flock.gameObject.activeInHierarchy && landingChild != null)
		{
			_gotcha = false;
			lerpCounter = 0;
			if (_controller._featherPS != null)
			{
				_controller._featherPS.position = landingChild._thisT.position;
				_controller._featherPS.GetComponent<ParticleSystem>().Emit(Random.Range(0, 3));
			}
			landing = false;
			_idle = false;
			landingChild._avoid = true;
			landingChild._damping = landingChild._spawner._maxDamping;
			landingChild._model.GetComponent<Animation>().CrossFade(landingChild._spawner._flapAnimation, 0.2f);
			landingChild._dived = true;
			landingChild._speed = 0f;
			landingChild._move = true;
			landingChild._landing = false;
			landingChild.Flap();
			landingChild._wayPoint = new Vector3(landingChild._wayPoint.x, _thisT.position.y + 10f, landingChild._wayPoint.z);
			if (_controller._autoCatchDelay.x > 0f)
			{
				StartCoroutine(GetFlockChild(_controller._autoCatchDelay.x + 0.1f, _controller._autoCatchDelay.y + 0.1f));
			}
			landingChild = null;
			_controller._activeLandingSpots--;
		}
	}
}
