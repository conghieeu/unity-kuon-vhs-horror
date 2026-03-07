using UnityEngine;

namespace Colorful
{
	[HelpURL("http://www.thomashourdel.com/colorful/doc/artistic-effects/dithering.html")]
	[ExecuteInEditMode]
	[AddComponentMenu("Colorful FX/Artistic Effects/Dithering")]
	public class Dithering : BaseEffect
	{
		[Tooltip("Show the original picture under the dithering pass.")]
		public bool ShowOriginal;

		[Tooltip("Convert the original render to black & white.")]
		public bool ConvertToGrayscale;

		[Range(0f, 1f)]
		[Tooltip("Amount of red to contribute to the luminosity.")]
		public float RedLuminance = 0.299f;

		[Range(0f, 1f)]
		[Tooltip("Amount of green to contribute to the luminosity.")]
		public float GreenLuminance = 0.587f;

		[Range(0f, 1f)]
		[Tooltip("Amount of blue to contribute to the luminosity.")]
		public float BlueLuminance = 0.114f;

		[Range(0f, 1f)]
		[Tooltip("Blending factor.")]
		public float Amount = 1f;

		protected Texture2D m_DitherPattern;

		protected override void OnRenderImage(RenderTexture source, RenderTexture destination)
		{
			if (Amount <= 0f)
			{
				Graphics.Blit(source, destination);
				return;
			}
			if (m_DitherPattern == null)
			{
				m_DitherPattern = Resources.Load<Texture2D>("Misc/DitherPattern");
			}
			base.Material.SetTexture("_Pattern", m_DitherPattern);
			base.Material.SetVector("_Params", new Vector4(RedLuminance, GreenLuminance, BlueLuminance, Amount));
			int num = (ShowOriginal ? 4 : 0);
			num += (ConvertToGrayscale ? 2 : 0);
			num += (CLib.IsLinearColorSpace() ? 1 : 0);
			Graphics.Blit(source, destination, base.Material, num);
		}

		protected override string GetShaderName()
		{
			return "Hidden/Colorful/Dithering";
		}
	}
}
