using System;
using UnityEngine;

namespace UHFPS.Runtime
{
    [Serializable]
    public class WobbleMotion : BlendMotionModule
    {
        public override string Name => "General/Wobble Motion";

        private float amplitude;
        private float frequency;
        private float duration;
        private bool playWobble;

        public void ApplyWobble(float amplitude, float frequency, float duration)
        {
            this.amplitude = amplitude;
            this.frequency = frequency;
            this.duration = duration;
            playWobble = true;
        }

        public override void MotionUpdate(float deltaTime)
        {
            base.MotionUpdate(deltaTime);

            if (IsUpdating)
            {
                float noiseSpeed = Time.time * frequency;
                float xOffset = Perlin(noiseSpeed, 1f) * amplitude;
                float yOffset = Perlin(noiseSpeed, 2f) * amplitude;
                float zOffset = Perlin(noiseSpeed, 3f) * amplitude;
                Vector3 offset = new(xOffset, yOffset, zOffset);
                SetTargetPosition(offset);
            }

            if (duration > 0)
            {
                duration -= Time.deltaTime;
                SetMotionWeight(1f);
            }
            else if(playWobble)
            {
                SetTargetWeight(0f);
                playWobble = false;
            }
        }

        private float Perlin(float x, float y)
        {
            float value = Mathf.PerlinNoise(x, y);
            return (value - 0.5f) * 2;
        }

        public override void Reset()
        {
            amplitude = 0;
            frequency = 0;
            duration = 0;
        }
    }
}