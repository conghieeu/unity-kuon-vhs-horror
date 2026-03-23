using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering;
using UnityEngine;
using System;

namespace UHFPS.Rendering
{
    [Serializable, VolumeComponentMenu("UHFPS Post-Processing/Raindrop")]
    public class Raindrop : VolumeComponent, IPostProcessComponent
    {
        public ClampedFloatParameter Raining = new(1f, 0f, 1f);
        public NoInterpTextureParameter DropletsMask = new(null);
        public NoInterpVector2Parameter Tiling = new(Vector2.one);
        public NoInterpFloatParameter Distortion = new(0.5f);
        public NoInterpClampedFloatParameter TilingScale = new(1f, 0.1f, 2f);
        public NoInterpClampedFloatParameter GlobalRotation = new(0f, -180f, 180f);
        public NoInterpClampedFloatParameter DropletsGravity = new(0f, 0f, 1f);
        public NoInterpClampedFloatParameter DropletsSpeed = new(1f, 0f, 2f);
        public NoInterpClampedFloatParameter DropletsStrength = new(1f, 0f, 1f);

        private bool State => active && Raining.overrideState;
        public bool IsActive() => State && Raining.value > 0;
        public bool IsTileCompatible() => false;
    }
}