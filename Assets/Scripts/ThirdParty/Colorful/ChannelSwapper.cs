using UnityEngine;

namespace Colorful
{
	[HelpURL("http://www.thomashourdel.com/colorful/doc/color-correction/channel-swapper.html")]
	[ExecuteInEditMode]
	[AddComponentMenu("Colorful FX/Color Correction/Channel Swapper")]
	public class ChannelSwapper : BaseEffect
	{
		public enum Channel
		{
			Red = 0,
			Green = 1,
			Blue = 2
		}

		[Tooltip("Source channel to use for the output red channel.")]
		public Channel RedSource;

		[Tooltip("Source channel to use for the output green channel.")]
		public Channel GreenSource = Channel.Green;

		[Tooltip("Source channel to use for the output blue channel.")]
		public Channel BlueSource = Channel.Blue;

		private static Vector4[] m_Channels = new Vector4[3]
		{
			new Vector4(1f, 0f, 0f, 0f),
			new Vector4(0f, 1f, 0f, 0f),
			new Vector4(0f, 0f, 1f, 0f)
		};

		protected override void OnRenderImage(RenderTexture source, RenderTexture destination)
		{
			base.Material.SetVector("_Red", m_Channels[(int)RedSource]);
			base.Material.SetVector("_Green", m_Channels[(int)GreenSource]);
			base.Material.SetVector("_Blue", m_Channels[(int)BlueSource]);
			Graphics.Blit(source, destination, base.Material);
		}

		protected override string GetShaderName()
		{
			return "Hidden/Colorful/Channel Swapper";
		}
	}
}
