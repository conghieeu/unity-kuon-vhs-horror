using UnityEngine;

public class UnluckDistanceDisabler : MonoBehaviour
{
	public int _distanceDisable = 1000;

	public Transform _distanceFrom;

	public bool _distanceFromMainCam;

	public float _disableCheckInterval = 10f;

	public float _enableCheckInterval = 1f;

	public bool _disableOnStart;

	public void Start()
	{
		if (_distanceFromMainCam)
		{
			_distanceFrom = Camera.main.transform;
		}
		InvokeRepeating("CheckDisable", _disableCheckInterval + Random.value * _disableCheckInterval, _disableCheckInterval);
		InvokeRepeating("CheckEnable", _enableCheckInterval + Random.value * _enableCheckInterval, _enableCheckInterval);
		Invoke("DisableOnStart", 0.01f);
	}

	public void DisableOnStart()
	{
		if (_disableOnStart)
		{
			base.gameObject.SetActive(value: false);
		}
	}

	public void CheckDisable()
	{
		if (base.gameObject.activeInHierarchy && (base.transform.position - _distanceFrom.position).sqrMagnitude > (float)(_distanceDisable * _distanceDisable))
		{
			base.gameObject.SetActive(value: false);
		}
	}

	public void CheckEnable()
	{
		if (!base.gameObject.activeInHierarchy && (base.transform.position - _distanceFrom.position).sqrMagnitude < (float)(_distanceDisable * _distanceDisable))
		{
			base.gameObject.SetActive(value: true);
		}
	}
}
