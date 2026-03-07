using UnityEngine;

namespace Colorful
{
	[HelpURL("http://www.thomashourdel.com/colorful/doc/color-correction/vintage-fast.html")]
	[ExecuteInEditMode]
	[AddComponentMenu("Colorful FX/Color Correction/Vintage")]
	public class VintageFast : LookupFilter3D
	{
		public Vintage.InstragramFilter Filter;

		protected Vintage.InstragramFilter m_CurrentFilter;

		protected override void OnRenderImage(RenderTexture source, RenderTexture destination)
		{
			if (Filter != m_CurrentFilter)
			{
				m_CurrentFilter = Filter;
				if (Filter == Vintage.InstragramFilter.None)
				{
					LookupTexture = null;
				}
				else
				{
					LookupTexture = Resources.Load<Texture2D>("InstagramFast/" + Filter);
				}
			}
			base.OnRenderImage(source, destination);
		}
	}
}
