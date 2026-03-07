using UnityEngine;

namespace Colorful
{
	[HelpURL("http://www.thomashourdel.com/colorful/doc/color-correction/dynamic-lookup.html")]
	[ExecuteInEditMode]
	[AddComponentMenu("Colorful FX/Color Correction/Dynamic Lookup")]
	public class DynamicLookup : BaseEffect
	{
		[ColorUsage(false)]
		public Color White = new Color(1f, 1f, 1f);

		[ColorUsage(false)]
		public Color Black = new Color(0f, 0f, 0f);

		[ColorUsage(false)]
		public Color Red = new Color(1f, 0f, 0f);

		[ColorUsage(false)]
		public Color Green = new Color(0f, 1f, 0f);

		[ColorUsage(false)]
		public Color Blue = new Color(0f, 0f, 1f);

		[ColorUsage(false)]
		public Color Yellow = new Color(1f, 1f, 0f);

		[ColorUsage(false)]
		public Color Magenta = new Color(1f, 0f, 1f);

		[ColorUsage(false)]
		public Color Cyan = new Color(0f, 1f, 1f);

		[Range(0f, 1f)]
		[Tooltip("Blending factor.")]
		public float Amount = 1f;

		protected override void OnRenderImage(RenderTexture source, RenderTexture destination)
		{
			base.Material.SetColor("_White", White);
			base.Material.SetColor("_Black", Black);
			base.Material.SetColor("_Red", Red);
			base.Material.SetColor("_Green", Green);
			base.Material.SetColor("_Blue", Blue);
			base.Material.SetColor("_Yellow", Yellow);
			base.Material.SetColor("_Magenta", Magenta);
			base.Material.SetColor("_Cyan", Cyan);
			base.Material.SetFloat("_Amount", Amount);
			Graphics.Blit(source, destination, base.Material, CLib.IsLinearColorSpace() ? 1 : 0);
		}

		protected override string GetShaderName()
		{
			return "Hidden/Colorful/DynamicLookup";
		}
	}
}
