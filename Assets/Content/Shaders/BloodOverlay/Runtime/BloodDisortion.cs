using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering;
using UnityEngine;
using System;

namespace UHFPS.Rendering
{
    [Serializable, VolumeComponentMenu("UHFPS Post-Processing/Blood Disortion")]
    public class BloodDisortion : VolumeComponent, IPostProcessComponent
    {
        public NoInterpColorParameter BlendColor = new(Color.white);
        public NoInterpColorParameter OverlayColor = new(Color.white);
        public NoInterpTextureParameter BlendTexture = new(null);
        public NoInterpTextureParameter BumpTexture = new(null);

        public ClampedFloatParameter BloodAmount = new(0f, 0f, 1f);
        public NoInterpClampedFloatParameter MinBloodAmount = new(0f, 0f, 1f);
        public NoInterpClampedFloatParameter MaxBloodAmount = new(1f, 0f, 1f);
        public NoInterpClampedFloatParameter EdgeSharpness = new(0.5f, 0f, 1f);
        public NoInterpClampedFloatParameter Distortion = new(0.5f, 0f, 1f);

        private bool State => active && BloodAmount.overrideState;
        public bool IsActive() => State && BloodAmount.value > 0;
        public bool IsTileCompatible() => false;
    }
}