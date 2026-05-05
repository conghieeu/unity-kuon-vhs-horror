using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering;
using System;

namespace UHFPS.Rendering
{
    [Serializable, VolumeComponentMenu("UHFPS Post-Processing/Fear Tentacles")]
    public class FearTentancles : VolumeComponent, IPostProcessComponent
    {
        public ClampedFloatParameter EffectFade = new(0, 0f, 1f);
        public ClampedFloatParameter TentaclesPosition = new(0, -0.2f, 0.2f);
        public ClampedFloatParameter LayerPosition = new(0, -2f, 2f);
        public ClampedFloatParameter VignetteStrength = new(0, 0f, 1f);
        public ClampedFloatParameter TentaclesSpeed = new(1f, 0.1f, 3f);
        public ClampedIntParameter Tentacles = new(20, 10, 50);
        public BoolParameter TopLayer = new(false);

        private bool State => active && EffectFade.overrideState;
        public bool IsActive() => State && EffectFade.value > 0f;
        public bool IsTileCompatible() => false;
    }
}