using System;
using UnityEngine;

namespace ch.sycoforge.Decal.Demo
{
	public class Sinoid : MonoBehaviour
	{
		public float AngularVelocity = 2f;

		public float SineFreq = 0.2f;

		public float Amplitude = 0.25f;

		private float accuTime;

		private Vector3 startPos;

		private void Start()
		{
			startPos = base.transform.position;
		}

		private void Update()
		{
			accuTime += Time.deltaTime;
			base.transform.position = startPos + Vector3.up * Amplitude * Mathf.Sin(accuTime * 2f * (float)Math.PI * SineFreq);
			base.transform.Rotate((Vector3.up + Vector3.forward) * AngularVelocity * Time.deltaTime);
		}
	}
}
