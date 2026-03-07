using UnityEngine;

namespace Colorful
{
	[HelpURL("http://www.thomashourdel.com/colorful/doc/color-correction/technicolor.html")]
	[ExecuteInEditMode]
	[AddComponentMenu("Colorful FX/Color Correction/Technicolor")]
	public class Technicolor : BaseEffect
	{
		[Range(0f, 8f)]
		public float Exposure = 4f;

		public Vector3 Balance = new Vector3(0.25f, 0.25f, 0.25f);

		[Range(0f, 1f)]
		[Tooltip("Blending factor.")]
		public float Amount = 0.5f;

		protected override void OnRenderImage(RenderTexture source, RenderTexture destination)
		{
			if (Amount <= 0f)
			{
				Graphics.Blit(source, destination);
				return;
			}
			base.Material.SetFloat("_Exposure", 8f - Exposure);
			base.Material.SetVector("_Balance", Vector3.one - Balance);
			base.Material.SetFloat("_Amount", Amount);
			Graphics.Blit(source, destination, base.Material);
		}

		protected override string GetShaderName()
		{
			return "Hidden/Colorful/Technicolor";
		}
	}
}
