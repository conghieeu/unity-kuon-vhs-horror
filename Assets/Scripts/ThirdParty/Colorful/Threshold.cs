using UnityEngine;

namespace Colorful
{
	[HelpURL("http://www.thomashourdel.com/colorful/doc/color-correction/threshold.html")]
	[ExecuteInEditMode]
	[AddComponentMenu("Colorful FX/Color Correction/Threshold")]
	public class Threshold : BaseEffect
	{
		[Range(1f, 255f)]
		[Tooltip("Luminosity threshold.")]
		public float Value = 128f;

		[Range(0f, 128f)]
		[Tooltip("Aomunt of randomization.")]
		public float NoiseRange = 24f;

		[Tooltip("Adds some randomization to the threshold value.")]
		public bool UseNoise;

		protected override void OnRenderImage(RenderTexture source, RenderTexture destination)
		{
			base.Material.SetFloat("_Threshold", Value / 255f);
			base.Material.SetFloat("_Range", NoiseRange / 255f);
			Graphics.Blit(source, destination, base.Material, UseNoise ? 1 : 0);
		}

		protected override string GetShaderName()
		{
			return "Hidden/Colorful/Threshold";
		}
	}
}
