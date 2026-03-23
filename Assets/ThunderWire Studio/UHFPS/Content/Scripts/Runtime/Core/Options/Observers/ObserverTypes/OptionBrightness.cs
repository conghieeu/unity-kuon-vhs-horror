using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace UHFPS.Runtime
{
    [Serializable]
    public class OptionBrightness : OptionObserverType
    {
        public Volume Volume;
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