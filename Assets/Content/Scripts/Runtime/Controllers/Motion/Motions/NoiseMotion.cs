using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace UHFPS.Runtime
{
    [Serializable]
    public class NoiseMotion : SpringMotionModule
    {
        public override string Name => "General/Noise Motion";

        [Header("General Settings")]
        [Range(0f, 5f)] public float noiseSpeed = 1f;
        [Range(0f, 1f)] public float noiseJitter = 0f;

        [Header("Amplitude Settings")]
        public Vector3 positionAmplitude = Vector3.zero;
        public Vector3 rotationAmplitude = Vector3.zero;

        public override void MotionUpdate(float deltaTime)
        {
            if (!IsUpdatable)
            {
                SetTargetPosition(Vector3.zero);
                SetTargetRotation(Vector3.zero);
                return;
            }

            float jitterValue = noiseJitter < 0.01f ? 0f : Random.Range(0f, noiseJitter);
            float noiseSpeedValue = Time.time * noiseSpeed;

            Vector3 positionNoise = new Vector3()
            {
                x = (Mathf.PerlinNoise(jitterValue, noiseSpeedValue) - 0.5f) * positionAmplitude.x,
                y = (Mathf.PerlinNoise(jitterValue + 1f, noiseSpeedValue) - 0.5f) * positionAmplitude.y,
                z = (Mathf.PerlinNoise(jitterValue + 2f, noiseSpeedValue) - 0.5f) * positionAmplitude.z
            };

            Vector3 rotationNoise = new Vector3()
            {
                x = (Mathf.PerlinNoise(jitterValue, noiseSpeedValue) - 0.5f) * rotationAmplitude.x,
                y = (Mathf.PerlinNoise(jitterValue + 1f, noiseSpeedValue) - 0.5f) * rotationAmplitude.y,
                z = (Mathf.PerlinNoise(jitterValue + 2f, noiseSpeedValue) - 0.5f) * rotationAmplitude.z
            };

            SetTargetPosition(positionNoise);
            SetTargetRotation(rotationNoise);
        }
    }
}
