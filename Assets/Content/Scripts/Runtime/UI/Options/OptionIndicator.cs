using System.Collections.Generic;
using ThunderWire.Attributes;
using UnityEngine;
using UnityEngine.UI;

namespace UHFPS.Runtime
{
    [InspectorHeader("Option Indicator")]
    public class OptionIndicator : MonoBehaviour
    {
        public List<Image> Indicators = new();

        [Header("Colors")]
        public Color EnabledColor = Color.white;
        public Color DisabledColor = Color.white;

        public void SetIndicator(int index)
        {
            for (int i = 0; i < Indicators.Count; i++)
            {
                var indicator = Indicators[i];
                indicator.color = i == index
                    ? EnabledColor : DisabledColor;
            }
        }
    }
}