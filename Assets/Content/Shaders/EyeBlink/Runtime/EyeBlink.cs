using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering;
using System;

namespace UHFPS.Rendering
{
    [Serializable, VolumeComponentMenu("UHFPS Post-Processing/Eye Blink")]
    public class EyeBlink : VolumeComponent, IPostProcessComponent
    {
        public ClampedFloatParameter Blink = new(0f, 0f, 1f);
        public ClampedFloatParameter VignetteOuterRing = new(0.4f, 0f, 1f);
        public ClampedFloatParameter VignetteInnerRing = new(0.5f, 0f, 1f);
        public ClampedFloatParameter VignetteAspectRatio = new(1f, 0f, 1f);

        private bool State => active && Blink.overrideState;
        public bool IsActive() => State && Blink.value > 0;
        public bool IsTileCompatible() => false;
    }
}