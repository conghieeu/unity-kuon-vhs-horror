using System;
using UnityEngine;

namespace UnityStandardAssets.Vehicles.Aeroplane
{
	[RequireComponent(typeof(ParticleSystem))]
	public class JetParticleEffect : MonoBehaviour
	{
		public Color minColour;

		private AeroplaneController m_Jet;

		private ParticleSystem m_System;

		private float m_OriginalStartSize;

		private float m_OriginalLifetime;

		private Color m_OriginalStartColor;

		private void Start()
		{
			m_Jet = FindAeroplaneParent();
			m_System = GetComponent<ParticleSystem>();
			m_OriginalLifetime = m_System.startLifetime;
			m_OriginalStartSize = m_System.startSize;
			m_OriginalStartColor = m_System.startColor;
		}

		private void Update()
		{
			m_System.startLifetime = Mathf.Lerp(0f, m_OriginalLifetime, m_Jet.Throttle);
			m_System.startSize = Mathf.Lerp(m_OriginalStartSize * 0.3f, m_OriginalStartSize, m_Jet.Throttle);
			m_System.startColor = Color.Lerp(minColour, m_OriginalStartColor, m_Jet.Throttle);
		}

		private AeroplaneController FindAeroplaneParent()
		{
			Transform parent = base.transform;
			while (parent != null)
			{
				AeroplaneController component = parent.GetComponent<AeroplaneController>();
				if (component == null)
				{
					parent = parent.parent;
					continue;
				}
				return component;
			}
			throw new Exception(" AeroplaneContoller not found in object hierarchy");
		}
	}
}
