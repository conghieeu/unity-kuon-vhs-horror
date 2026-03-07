using System;
using UnityEngine;

namespace Colorful
{
	[HelpURL("http://www.thomashourdel.com/colorful/doc/artistic-effects/halftone.html")]
	[ExecuteInEditMode]
	[AddComponentMenu("Colorful FX/Artistic Effects/Halftone")]
	public class Halftone : BaseEffect
	{
		[Min(0f)]
		[Tooltip("Global haltfoning scale.")]
		public float Scale = 12f;

		[Min(0f)]
		[Tooltip("Individual dot size.")]
		public float DotSize = 1.35f;

		[Tooltip("Rotates the dot placement according to the Center point.")]
		public float Angle = 1.2f;

		[Range(0f, 1f)]
		[Tooltip("Dots antialiasing")]
		public float Smoothness = 0.08f;

		[Tooltip("Center point to use for the rotation.")]
		public Vector2 Center = new Vector2(0.5f, 0.5f);

		[Tooltip("Turns the effect black & white.")]
		public bool Desaturate;

		protected override void OnRenderImage(RenderTexture source, RenderTexture destination)
		{
			base.Material.SetVector("_Center", new Vector2(Center.x * (float)source.width, Center.y * (float)source.height));
			base.Material.SetVector("_Params", new Vector3(Scale, DotSize, Smoothness));
			Matrix4x4 value = default(Matrix4x4);
			value.SetRow(0, CMYKRot(Angle + (float)Math.PI / 12f));
			value.SetRow(1, CMYKRot(Angle + 1.3089969f));
			value.SetRow(2, CMYKRot(Angle));
			value.SetRow(3, CMYKRot(Angle + (float)Math.PI / 4f));
			base.Material.SetMatrix("_MatRot", value);
			Graphics.Blit(source, destination, base.Material, Desaturate ? 1 : 0);
		}

		private Vector4 CMYKRot(float angle)
		{
			float num = Mathf.Cos(angle);
			float num2 = Mathf.Sin(angle);
			return new Vector4(num, 0f - num2, num2, num);
		}

		protected override string GetShaderName()
		{
			return "Hidden/Colorful/Halftone";
		}
	}
}
