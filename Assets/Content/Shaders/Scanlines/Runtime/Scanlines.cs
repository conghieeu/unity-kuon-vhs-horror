using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering;
using System;

namespace UHFPS.Rendering
{
    [Serializable, VolumeComponentMenu("UHFPS Post-Processing/Scanlines")]
    public class Scanlines : VolumeComponent, IPostProcessComponent
    {
        public ClampedFloatParameter ScanlinesStrength = new (0f, 0f, 2f);
        public ClampedFloatParameter ScanlinesSharpness = new (1.5f, 0f, 5f);
        public ClampedFloatParameter ScanlinesScroll = new (2f, 0f, 5f);
        public FloatParameter ScanlinesFrequency = new (5);

        public FloatParameter GlitchIntensity = new(0);
        public FloatParameter GlitchFrequency = new (0);

        private bool State => active && ScanlinesStrength.overrideState;
        public bool IsActive() => State && ScanlinesStrength.value > 0;
        public bool IsTileCompatible() => false;
    }
}