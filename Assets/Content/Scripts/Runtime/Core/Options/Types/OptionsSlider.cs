using System;
using UnityEngine;
using UnityEngine.UI;
using ThunderWire.Attributes;
using TMPro;

namespace UHFPS.Runtime
{
    [InspectorHeader("Options Slider")]
    public class OptionsSlider : OptionBehaviour
    {
        public enum SliderTypeEnum { FloatSlider, IntegerSlider }

        public TMP_Text SliderText;
        public Slider Slider;

        [Header("Slider Settings")]
        public SliderTypeEnum SliderType = SliderTypeEnum.FloatSlider;
        public MinMax SliderLimits = new(0, 1);
        public float SliderValue = 0f;

        [Header("Snap Settings")]
        public bool UseSnapping;
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