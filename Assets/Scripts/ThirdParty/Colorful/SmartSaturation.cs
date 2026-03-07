using UnityEngine;

namespace Colorful
{
	[HelpURL("http://www.thomashourdel.com/colorful/doc/color-correction/smart-saturation.html")]
	[ExecuteInEditMode]
	[AddComponentMenu("Colorful FX/Color Correction/Smart Saturation")]
	public class SmartSaturation : BaseEffect
	{
		[Range(0f, 2f)]
		[Tooltip("Saturation boost. Default: 1 (no boost).")]
		public float Boost = 1f;

		public AnimationCurve Curve;

		private Texture2D _CurveTexture;

		protected Texture2D m_CurveTexture
		{
			get
			{
				if (_CurveTexture == null)
				{
					UpdateCurve();
				}
				return _CurveTexture;
			}
		}

		protected virtual void Reset()
		{
			Curve = new AnimationCurve(new Keyframe(0f, 0.5f, 0f, 0f), new Keyframe(1f, 0.5f, 0f, 0f));
		}

		protected virtual void OnEnable()
		{
			if (Curve == null)
			{
				Reset();
			}
		}

		protected override void OnDisable()
		{
			base.OnDisable();
			if (_CurveTexture != null)
			{
				Object.DestroyImmediate(_CurveTexture);
			}
		}

		public virtual void UpdateCurve()
		{
			if (_CurveTexture == null)
			{
				_CurveTexture = new Texture2D(256, 1, TextureFormat.Alpha8, mipChain: false)
				{
					name = "Saturation Curve Texture",
					wrapMode = TextureWrapMode.Clamp,
					anisoLevel = 0,
					filterMode = FilterMode.Bilinear,
					hideFlags = HideFlags.DontSave
				};
			}
			Color[] pixels = _CurveTexture.GetPixels();
			for (int i = 0; i < 256; i++)
			{
				float num = Mathf.Clamp01(Curve.Evaluate((float)i / 255f));
				pixels[i] = new Color(num, num, num, num);
			}
			_CurveTexture.SetPixels(pixels);
			_CurveTexture.Apply();
		}

		protected override void OnRenderImage(RenderTexture source, RenderTexture destination)
		{
			base.Material.SetTexture("_Curve", m_CurveTexture);
			base.Material.SetFloat("_Boost", Boost);
			Graphics.Blit(source, destination, base.Material);
		}

		protected override string GetShaderName()
		{
			return "Hidden/Colorful/Smart Saturation";
		}
	}
}
