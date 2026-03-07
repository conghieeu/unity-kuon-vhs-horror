using System;
using UnityEngine;

namespace Colorful
{
	[HelpURL("http://www.thomashourdel.com/colorful/doc/camera-effects/glitch.html")]
	[ExecuteInEditMode]
	[AddComponentMenu("Colorful FX/Camera Effects/Glitch")]
	public class Glitch : BaseEffect
	{
		public enum GlitchingMode
		{
			Interferences = 0,
			Tearing = 1,
			Complete = 2
		}

		[Serializable]
		public class InterferenceSettings
		{
			public float Speed = 10f;

			public float Density = 8f;

			public float MaxDisplacement = 2f;
		}

		[Serializable]
		public class TearingSettings
		{
			public float Speed = 1f;

			[Range(0f, 1f)]
			public float Intensity = 0.25f;

			[Range(0f, 0.5f)]
			public float MaxDisplacement = 0.05f;

			public bool AllowFlipping;

			public bool YuvColorBleeding = true;

			[Range(-2f, 2f)]
			public float YuvOffset = 0.5f;
		}

		[Tooltip("Automatically activate/deactivate the effect randomly.")]
		public bool RandomActivation;

		public Vector2 RandomEvery = new Vector2(1f, 2f);

		public Vector2 RandomDuration = new Vector2(1f, 2f);

		[Tooltip("Glitch type.")]
		public GlitchingMode Mode;

		public InterferenceSettings SettingsInterferences = new InterferenceSettings();

		public TearingSettings SettingsTearing = new TearingSettings();

		protected bool m_Activated = true;

		protected float m_EveryTimer;

		protected float m_EveryTimerEnd;

		protected float m_DurationTimer;

		protected float m_DurationTimerEnd;

		public bool IsActive => m_Activated;

		protected override void Start()
		{
			base.Start();
			m_DurationTimerEnd = UnityEngine.Random.Range(RandomDuration.x, RandomDuration.y);
		}

		protected virtual void Update()
		{
			if (!RandomActivation)
			{
				return;
			}
			if (m_Activated)
			{
				m_DurationTimer += Time.deltaTime;
				if (m_DurationTimer >= m_DurationTimerEnd)
				{
					m_DurationTimer = 0f;
					m_Activated = false;
					m_EveryTimerEnd = UnityEngine.Random.Range(RandomEvery.x, RandomEvery.y);
				}
			}
			else
			{
				m_EveryTimer += Time.deltaTime;
				if (m_EveryTimer >= m_EveryTimerEnd)
				{
					m_EveryTimer = 0f;
					m_Activated = true;
					m_DurationTimerEnd = UnityEngine.Random.Range(RandomDuration.x, RandomDuration.y);
				}
			}
		}

		protected override void OnRenderImage(RenderTexture source, RenderTexture destination)
		{
			if (!m_Activated)
			{
				Graphics.Blit(source, destination);
				return;
			}
			if (Mode == GlitchingMode.Interferences)
			{
				DoInterferences(source, destination, SettingsInterferences);
				return;
			}
			if (Mode == GlitchingMode.Tearing)
			{
				DoTearing(source, destination, SettingsTearing);
				return;
			}
			RenderTexture temporary = RenderTexture.GetTemporary(source.width, source.width, 0, RenderTextureFormat.ARGB32);
			DoTearing(source, temporary, SettingsTearing);
			DoInterferences(temporary, destination, SettingsInterferences);
			temporary.Release();
		}

		protected virtual void DoInterferences(RenderTexture source, RenderTexture destination, InterferenceSettings settings)
		{
			base.Material.SetVector("_Params", new Vector3(settings.Speed, settings.Density, settings.MaxDisplacement));
			Graphics.Blit(source, destination, base.Material, 0);
		}

		protected virtual void DoTearing(RenderTexture source, RenderTexture destination, TearingSettings settings)
		{
			base.Material.SetVector("_Params", new Vector4(settings.Speed, settings.Intensity, settings.MaxDisplacement, settings.YuvOffset));
			int pass = 1;
			if (settings.AllowFlipping && settings.YuvColorBleeding)
			{
				pass = 4;
			}
			else if (settings.AllowFlipping)
			{
				pass = 2;
			}
			else if (settings.YuvColorBleeding)
			{
				pass = 3;
			}
			Graphics.Blit(source, destination, base.Material, pass);
		}

		protected override string GetShaderName()
		{
			return "Hidden/Colorful/Glitch";
		}
	}
}
