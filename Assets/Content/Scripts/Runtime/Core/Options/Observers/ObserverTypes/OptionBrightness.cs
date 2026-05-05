using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

using ThunderWire.Attributes;

namespace UHFPS.Runtime
{
    [Serializable]
    [Summary("Observer lắng nghe thay đổi Option độ sáng và tự động điều chỉnh Post Exposure trong Volume Profile.")]
    public class OptionBrightness : OptionObserverType
    {
        [Tooltip("Volume Profile chứa Post Processing Color Adjustments.")]
        public Volume Volume;

        [Tooltip("Giới hạn độ sáng (Exposure) tối thiểu và tối đa.")]
        public MinMax ExposureLimits;

        public override string Name => "Brightness";

        public override void OptionUpdate(object value)
        {
            if (value == null || Volume == null)
                return;

            if (Volume.profile.TryGet<ColorAdjustments>(out var colorAdjustments))
            {
                float exposure = Mathf.Lerp(ExposureLimits.RealMin, ExposureLimits.RealMax, (float)value);
                colorAdjustments.postExposure.value = exposure;
            }
        }
    }
}