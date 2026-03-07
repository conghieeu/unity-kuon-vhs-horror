using UnityEngine;

namespace Colorful
{
	[HelpURL("http://www.thomashourdel.com/colorful/doc/color-correction/levels.html")]
	[ExecuteInEditMode]
	[AddComponentMenu("Colorful FX/Color Correction/Levels")]
	public class Levels : BaseEffect
	{
		public enum ColorMode
		{
			Monochrome = 0,
			RGB = 1
		}

		public ColorMode Mode;

		public Vector3 InputL = new Vector3(0f, 255f, 1f);

		public Vector3 InputR = new Vector3(0f, 255f, 1f);

		public Vector3 InputG = new Vector3(0f, 255f, 1f);

		public Vector3 InputB = new Vector3(0f, 255f, 1f);

		public Vector2 OutputL = new Vector2(0f, 255f);

		public Vector2 OutputR = new Vector2(0f, 255f);

		public Vector2 OutputG = new Vector2(0f, 255f);

		public Vector2 OutputB = new Vector2(0f, 255f);

		protected override void OnRenderImage(RenderTexture source, RenderTexture destination)
		{
			if (Mode == ColorMode.Monochrome)
			{
				base.Material.SetVector("_InputMin", new Vector4(InputL.x / 255f, InputL.x / 255f, InputL.x / 255f, 1f));
				base.Material.SetVector("_InputMax", new Vector4(InputL.y / 255f, InputL.y / 255f, InputL.y / 255f, 1f));
				base.Material.SetVector("_InputGamma", new Vector4(InputL.z, InputL.z, InputL.z, 1f));
				base.Material.SetVector("_OutputMin", new Vector4(OutputL.x / 255f, OutputL.x / 255f, OutputL.x / 255f, 1f));
				base.Material.SetVector("_OutputMax", new Vector4(OutputL.y / 255f, OutputL.y / 255f, OutputL.y / 255f, 1f));
			}
			else
			{
				base.Material.SetVector("_InputMin", new Vector4(InputR.x / 255f, InputG.x / 255f, InputB.x / 255f, 1f));
				base.Material.SetVector("_InputMax", new Vector4(InputR.y / 255f, InputG.y / 255f, InputB.y / 255f, 1f));
				base.Material.SetVector("_InputGamma", new Vector4(InputR.z, InputG.z, InputB.z, 1f));
				base.Material.SetVector("_OutputMin", new Vector4(OutputR.x / 255f, OutputG.x / 255f, OutputB.x / 255f, 1f));
				base.Material.SetVector("_OutputMax", new Vector4(OutputR.y / 255f, OutputG.y / 255f, OutputB.y / 255f, 1f));
			}
			Graphics.Blit(source, destination, base.Material);
		}

		protected override string GetShaderName()
		{
			return "Hidden/Colorful/Levels";
		}
	}
}
