using UnityEngine;

namespace Colorful
{
	[HelpURL("http://www.thomashourdel.com/colorful/doc/camera-effects/tv-vignette.html")]
	[ExecuteInEditMode]
	[AddComponentMenu("Colorful FX/Camera Effects/TV Vignette")]
	public class TVVignette : BaseEffect
	{
		[Min(0f)]
		public float Size = 25f;

		[Range(0f, 1f)]
		public float Offset = 0.2f;

		protected override void OnRenderImage(RenderTexture source, RenderTexture destination)
		{
			if (Offset >= 1f)
			{
				Graphics.Blit(source, destination);
				return;
			}
			base.Material.SetVector("_Params", new Vector2(Size, Offset));
			Graphics.Blit(source, destination, base.Material);
		}

		protected override string GetShaderName()
		{
			return "Hidden/Colorful/TV Vignette";
		}
	}
}
