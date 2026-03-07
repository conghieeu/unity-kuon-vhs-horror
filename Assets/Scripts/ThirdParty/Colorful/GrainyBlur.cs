using UnityEngine;

namespace Colorful
{
	[HelpURL("http://www.thomashourdel.com/colorful/doc/blur-effects/grainy-blur.html")]
	[ExecuteInEditMode]
	[AddComponentMenu("Colorful FX/Blur Effects/Grainy Blur")]
	public class GrainyBlur : BaseEffect
	{
		[Min(0f)]
		[Tooltip("Blur radius.")]
		public float Radius = 32f;

		[Range(1f, 32f)]
		[Tooltip("Sample count. Higher means better quality but slower processing.")]
		public int Samples = 16;

		protected override void OnRenderImage(RenderTexture source, RenderTexture destination)
		{
			if (Mathf.Approximately(Radius, 0f))
			{
				Graphics.Blit(source, destination);
				return;
			}
			base.Material.SetVector("_Params", new Vector2(Radius, Samples));
			Graphics.Blit(source, destination, base.Material);
		}

		protected override string GetShaderName()
		{
			return "Hidden/Colorful/GrainyBlur";
		}
	}
}
