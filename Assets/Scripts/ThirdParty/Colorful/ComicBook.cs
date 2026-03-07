using UnityEngine;

namespace Colorful
{
	[HelpURL("http://www.thomashourdel.com/colorful/doc/artistic-effects/comic-book.html")]
	[ExecuteInEditMode]
	[AddComponentMenu("Colorful FX/Artistic Effects/Comic Book")]
	public class ComicBook : BaseEffect
	{
		[Tooltip("Strip orientation in radians.")]
		public float StripAngle = 0.6f;

		[Min(0f)]
		[Tooltip("Amount of strips to draw.")]
		public float StripDensity = 180f;

		[Range(0f, 1f)]
		[Tooltip("Thickness of the inner strip fill.")]
		public float StripThickness = 0.5f;

		public Vector2 StripLimits = new Vector2(0.25f, 0.4f);

		[ColorUsage(false)]
		public Color StripInnerColor = new Color(0.3f, 0.3f, 0.3f);

		[ColorUsage(false)]
		public Color StripOuterColor = new Color(0.8f, 0.8f, 0.8f);

		[ColorUsage(false)]
		public Color FillColor = new Color(0.1f, 0.1f, 0.1f);

		[ColorUsage(false)]
		public Color BackgroundColor = Color.white;

		[Tooltip("Toggle edge detection (slower).")]
		public bool EdgeDetection;

		[Min(0.01f)]
		[Tooltip("Edge detection threshold. Use lower values for more visible edges.")]
		public float EdgeThreshold = 5f;

		[ColorUsage(false)]
		public Color EdgeColor = Color.black;

		[Range(0f, 1f)]
		[Tooltip("Blending factor.")]
		public float Amount = 1f;

		protected override void OnRenderImage(RenderTexture source, RenderTexture destination)
		{
			base.Material.SetVector("_StripParams", new Vector4(Mathf.Cos(StripAngle), Mathf.Sin(StripAngle), StripLimits.x, StripLimits.y));
			base.Material.SetVector("_StripParams2", new Vector3(StripDensity * 10f, StripThickness, Amount));
			base.Material.SetColor("_StripInnerColor", StripInnerColor);
			base.Material.SetColor("_StripOuterColor", StripOuterColor);
			base.Material.SetColor("_FillColor", FillColor);
			base.Material.SetColor("_BackgroundColor", BackgroundColor);
			if (EdgeDetection)
			{
				base.Material.SetFloat("_EdgeThreshold", 1f / (EdgeThreshold * 100f));
				base.Material.SetColor("_EdgeColor", EdgeColor);
				Graphics.Blit(source, destination, base.Material, 1);
			}
			else
			{
				Graphics.Blit(source, destination, base.Material, 0);
			}
		}

		protected override string GetShaderName()
		{
			return "Hidden/Colorful/Comic Book";
		}
	}
}
