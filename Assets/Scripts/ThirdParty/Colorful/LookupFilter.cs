using UnityEngine;

namespace Colorful
{
	[HelpURL("http://www.thomashourdel.com/colorful/doc/color-correction/lookup-filter.html")]
	[ExecuteInEditMode]
	[AddComponentMenu("Colorful FX/Color Correction/Lookup Filter (Deprecated)")]
	public class LookupFilter : BaseEffect
	{
		[Tooltip("The lookup texture to apply. Read the documentation to learn how to create one.")]
		public Texture LookupTexture;

		[Range(0f, 1f)]
		[Tooltip("Blending factor.")]
		public float Amount = 1f;

		protected override void OnRenderImage(RenderTexture source, RenderTexture destination)
		{
			if (LookupTexture == null || Amount <= 0f)
			{
				Graphics.Blit(source, destination);
				return;
			}
			base.Material.SetTexture("_LookupTex", LookupTexture);
			base.Material.SetFloat("_Amount", Amount);
			Graphics.Blit(source, destination, base.Material, CLib.IsLinearColorSpace() ? 1 : 0);
		}

		protected override string GetShaderName()
		{
			return "Hidden/Colorful/Lookup Filter (Deprecated)";
		}
	}
}
