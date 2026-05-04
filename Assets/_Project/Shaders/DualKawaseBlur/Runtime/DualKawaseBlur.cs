using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering;
using System;

namespace UHFPS.Rendering
{
    [Serializable, VolumeComponentMenu("UHFPS Post-Processing/Dual Kawase Blur")]
    public class DualKawaseBlur : VolumeComponent, IPostProcessComponent
    {
        public ClampedFloatParameter BlurRadius = new(0, 0, 15);
        public ClampedIntParameter Iteration = new(4, 1, 10);
        public ClampedFloatParameter RTDownScaling = new(2, 1, 10);

        public bool IsActive() => BlurRadius.value > 0f;
        public bool IsTileCompatible() => false;
    }
}