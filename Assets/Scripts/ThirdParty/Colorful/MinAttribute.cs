using UnityEngine;

namespace Colorful
{
	public sealed class MinAttribute : PropertyAttribute
	{
		public readonly float Min;

		public MinAttribute(float min)
		{
			Min = min;
		}
	}
}
