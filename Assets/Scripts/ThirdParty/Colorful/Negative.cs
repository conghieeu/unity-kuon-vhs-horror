using UnityEngine;

namespace Colorful
{
	[HelpURL("http://www.thomashourdel.com/colorful/doc/color-correction/negative.html")]
	[ExecuteInEditMode]
	[AddComponentMenu("Colorful FX/Color Correction/Negative")]
	public class Negative : BaseEffect
	{
		[Range(0f, 1f)]
		[Tooltip("Blending factor.")]
		public float Amount = 1f;

		protected override void OnRenderImage(RenderTexture source, RenderTexture destination)
		{
			if (Amount <= 0f)
			{
				Graphics.Blit(source, destination);
				return;
			}
			base.Material.SetFloat("_Amount", Amount);
			Graphics.Blit(source, destination, base.Material, CLib.IsLinearColorSpace() ? 1 : 0);
		}

		protected override string GetShaderName()
		{
			return "Hidden/Colorful/Negative";
		}
	}
}
