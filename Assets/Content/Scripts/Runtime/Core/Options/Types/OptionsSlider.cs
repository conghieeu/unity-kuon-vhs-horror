using System;
using UnityEngine;
using UnityEngine.UI;
using ThunderWire.Attributes;
using TMPro;

namespace UHFPS.Runtime
{
    [InspectorHeader("Options Slider")]
    [Summary("Điều khiển UI cho một tùy chọn dạng thanh trượt (Slider), hỗ trợ cả số nguyên và số thực, có chức năng làm tròn (Snapping).")]
    public class OptionsSlider : OptionBehaviour
    {
        public enum SliderTypeEnum { FloatSlider, IntegerSlider }

        [Tooltip("Text hiển thị giá trị số hiện tại của thanh trượt.")]
        public TMP_Text SliderText;

        [Tooltip("Component Slider của Unity UI.")]
        public Slider Slider;

        [Header("Slider Settings")]
        [Tooltip("Kiểu giá trị của thanh trượt: Float (Số thực), Integer (Số nguyên).")]
        public SliderTypeEnum SliderType = SliderTypeEnum.FloatSlider;

        [Tooltip("Giới hạn Min và Max của thanh trượt.")]
        public MinMax SliderLimits = new(0, 1);

        [Tooltip("Giá trị mặc định ban đầu.")]
        public float SliderValue = 0f;

        [Header("Snap Settings")]
        [Tooltip("Bật tính năng làm tròn giá trị thành các mốc cố định khi kéo (VD: 0, 0.5, 1.0).")]
        public bool UseSnapping;

        [Tooltip("Bước nhảy (Giá trị làm tròn) khi bật Snapping.")]
        public float SnapValue = 0.05f;

        private void Start()
        {
            Slider.value = SliderValue;
            SliderText.text = SliderValue.ToString();
        }

        public override void SetOptionValue(object value)
        {
            SetSliderValue((float)value);
            Slider.value = SliderValue;
            IsChanged = false;
        }

        public override object GetOptionValue()
        {
            return SliderType switch
            {
                SliderTypeEnum.FloatSlider => SliderValue,
                SliderTypeEnum.IntegerSlider => Mathf.RoundToInt(SliderValue),
                _ => SliderValue
            };
        }

        public override void SetOptionData(StorableCollection data)
        {
            if(data.TryGetValue("settings", out object[] settings))
            {
                SliderType = (SliderTypeEnum)settings[0];
                SliderLimits = (MinMax)settings[1];
                UseSnapping = (bool)settings[2];
                SnapValue = (float)settings[3];

                Slider.minValue = SliderLimits.RealMin;
                Slider.maxValue = SliderLimits.RealMax;
            }

            if (data.TryGetValue("defaultValue", out object value))
            {
                SliderValue = (float)value;
            }

            Slider.wholeNumbers = SliderType == SliderTypeEnum.IntegerSlider;
            SliderText.text = SliderValue.ToString();
        }

        public void SetSliderValue(float value)
        {
            if (SliderType == SliderTypeEnum.FloatSlider)
                SliderValue = (float)Math.Round(value, 2);
            else if (SliderType == SliderTypeEnum.IntegerSlider)
                SliderValue = Mathf.RoundToInt(value);

            if (UseSnapping)
                SliderValue = SnapTo(SliderValue, SnapValue);

            SliderText.text = SliderValue.ToString();
            IsChanged = true;
        }

        private float SnapTo(float value, float multiple)
        {
            return Mathf.Round(value / multiple) * multiple;
        }
    }
}