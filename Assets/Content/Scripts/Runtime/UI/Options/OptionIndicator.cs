using System.Collections.Generic;
using ThunderWire.Attributes;
using UnityEngine;
using UnityEngine.UI;

namespace UHFPS.Runtime
{
    [InspectorHeader("Option Indicator")]
    [Summary("Hiển thị các dấu chỉ (Indicator) để cho biết mức độ hoặc lựa chọn hiện tại trong một tùy chọn cài đặt.")]
    public class OptionIndicator : MonoBehaviour
    {
        [Tooltip("Danh sách các Image dấu chỉ.")]
        public List<Image> Indicators = new();

        [Header("Colors")]
        [Tooltip("Màu sắc của dấu chỉ khi được chọn.")]
        public Color EnabledColor = Color.white;
        [Tooltip("Màu sắc của dấu chỉ khi không được chọn.")]
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