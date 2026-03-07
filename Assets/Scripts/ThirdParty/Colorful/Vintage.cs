using UnityEngine;

namespace Colorful
{
	[HelpURL("http://www.thomashourdel.com/colorful/doc/color-correction/vintage.html")]
	[ExecuteInEditMode]
	[AddComponentMenu("Colorful FX/Color Correction/Vintage (Deprecated)")]
	public class Vintage : LookupFilter
	{
		public enum InstragramFilter
		{
			None = 0,
			F1977 = 1,
			Aden = 2,
			Amaro = 3,
			Brannan = 4,
			Crema = 5,
			Earlybird = 6,
			Hefe = 7,
			Hudson = 8,
			Inkwell = 9,
			Juno = 10,
			Kelvin = 11,
			Lark = 12,
			LoFi = 13,
			Ludwig = 14,
			Mayfair = 15,
			Nashville = 16,
			Perpetua = 17,
			Reyes = 18,
			Rise = 19,
			Sierra = 20,
			Slumber = 21,
			Sutro = 22,
			Toaster = 23,
			Valencia = 24,
			Walden = 25,
			Willow = 26,
			XProII = 27
		}

		public InstragramFilter Filter;

		protected InstragramFilter m_CurrentFilter;

		protected override void OnRenderImage(RenderTexture source, RenderTexture destination)
		{
			if (Filter != m_CurrentFilter)
			{
				m_CurrentFilter = Filter;
				if (Filter == InstragramFilter.None)
				{
					LookupTexture = null;
				}
				else
				{
					LookupTexture = Resources.Load<Texture2D>("Instagram/" + Filter);
				}
			}
			base.OnRenderImage(source, destination);
		}
	}
}
