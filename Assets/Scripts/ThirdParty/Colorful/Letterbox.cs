using UnityEngine;

namespace Colorful
{
	[HelpURL("http://www.thomashourdel.com/colorful/doc/camera-effects/letterbox.html")]
	[ExecuteInEditMode]
	[AddComponentMenu("Colorful FX/Camera Effects/Letterbox")]
	public class Letterbox : BaseEffect
	{
		[Min(0f)]
		[Tooltip("Crop the screen to the given aspect ratio.")]
		public float Aspect = 2.3333333f;

		[Tooltip("Letter/Pillar box color. Alpha is transparency.")]
		public Color FillColor = Color.black;

		protected override void OnRenderImage(RenderTexture source, RenderTexture destination)
		{
			float num = source.width;
			float num2 = source.height;
			float num3 = num / num2;
			float num4 = 0f;
			int pass = 0;
			base.Material.SetColor("_FillColor", FillColor);
			if (num3 < Aspect)
			{
				num4 = (num2 - num / Aspect) * 0.5f / num2;
			}
			else
			{
				if (!(num3 > Aspect))
				{
					Graphics.Blit(source, destination);
					return;
				}
				num4 = (num - num2 * Aspect) * 0.5f / num;
				pass = 1;
			}
			base.Material.SetVector("_Offsets", new Vector2(num4, 1f - num4));
			Graphics.Blit(source, destination, base.Material, pass);
		}

		protected override string GetShaderName()
		{
			return "Hidden/Colorful/Letterbox";
		}
	}
}
