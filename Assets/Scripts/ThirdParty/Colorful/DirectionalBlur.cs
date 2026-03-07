using UnityEngine;

namespace Colorful
{
	[HelpURL("http://www.thomashourdel.com/colorful/doc/blur-effects/directional-blur.html")]
	[ExecuteInEditMode]
	[AddComponentMenu("Colorful FX/Blur Effects/Directional Blur")]
	public class DirectionalBlur : BaseEffect
	{
		public enum QualityPreset
		{
			Low = 2,
			Medium = 4,
			High = 6,
			Custom = 7
		}

		[Tooltip("Quality preset. Higher means better quality but slower processing.")]
		public QualityPreset Quality = QualityPreset.Medium;

		[Range(1f, 16f)]
		[Tooltip("Sample count. Higher means better quality but slower processing.")]
		public int Samples = 5;

		[Range(0f, 5f)]
		[Tooltip("Blur strength (distance).")]
		public float Strength = 1f;

		[Tooltip("Blur direction in radians.")]
		public float Angle;

		protected override void OnRenderImage(RenderTexture source, RenderTexture destination)
		{
			int num = ((Quality == QualityPreset.Custom) ? Samples : ((int)Quality));
			float x = Mathf.Sin(Angle) * Strength * 0.05f / (float)num;
			float y = Mathf.Cos(Angle) * Strength * 0.05f / (float)num;
			base.Material.SetVector("_Params", new Vector3(x, y, num));
			Graphics.Blit(source, destination, base.Material);
		}

		protected override string GetShaderName()
		{
			return "Hidden/Colorful/DirectionalBlur";
		}
	}
}
