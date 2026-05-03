using ThunderWire.Attributes;
using UHFPS.Tools;
using UnityEngine;

namespace UHFPS.Runtime
{
    [InspectorHeader("Glare Effect")]
    public class GlareEffect : MonoBehaviour
    {
        public string ColorParam = "_BaseColor";
        public bool GlareState = true;

        [Tooltip("Distance at which the glare scales. (MIN = 0 * Scale - to - MAX = 1 * Scale)")]
        public MinMax ScaleDistance;

        [Range(0f, 1f), Tooltip("Minimum distance at which the object scales.")]
        public float MinScaleDistance = 0.5f;

        [Tooltip("Distance at which the object fades out when the camera is near or far.")]
        public MinMax NearFarDistance;

        [Tooltip("Distance at which the object begins to blend into the fading state.")]
        public float BlendDistance = 0.5f;

        [Tooltip("Minimum and maximum pulse scale.")]
        public MinMax PulseScale;

        [Tooltip("Time for which the glare remains on the minimum pulse scale.")]
        public float MinWaitTime = 1f;

        public float PulseSpeed = 1f;
        public float RotateSpeed = 3f;

        public bool EnableDistanceScaling = true;
        public bool EnableNearFading = true;
        public bool EnableRotation = true;

        private Transform mainCamera;
        private MeshRenderer meshRenderer;
        private float rotateAngle = 0.0f;

        private void Awake()
        {
            mainCamera = PlayerPresenceManager.Instance.PlayerCamera.transform;
            meshRenderer = GetComponent<MeshRenderer>();
        }

        private void Update()
        {
            float distance = Vector3.Distance(transform.position, mainCamera.position);

            // calculate scale fade distance
            float scaleFade = 1f;
            if (EnableDistanceScaling)
            {
                float distanceFade = Mathf.InverseLerp(ScaleDistance.RealMin, ScaleDistance.RealMax, distance);
                scaleFade *= Mathf.Clamp(distanceFade, MinScaleDistance, 1f);
            }

            // calculate hide distance
            float colorAlpha = GlareState ? 1f : 0f;
            if (EnableNearFading)
            {
                float blendDistance = NearFarDistance.RealMin + BlendDistance;
                colorAlpha *= Mathf.InverseLerp(NearFarDistance.RealMin, blendDistance, distance);
            }

            float fadeOutDistance = NearFarDistance.RealMax - BlendDistance;
            colorAlpha *= Mathf.InverseLerp(NearFarDistance.RealMax, fadeOutDistance, distance);
            SetColorAlpha(colorAlpha);

            // do nothing if glare is faded out
            if (colorAlpha <= 0)
            {
                transform.localScale = Vector3.zero;
                return;
            }

            // billboard effect
            Vector3 direction = mainCamera.transform.position - transform.position;
            Quaternion lookRotation = Quaternion.LookRotation(direction);

            // compute rotation around forward direction
            Quaternion offsetRotation = Quaternion.identity;
            if (EnableRotation)
            {
                rotateAngle = (rotateAngle + RotateSpeed * 10f * Time.deltaTime) % 360f;
                offsetRotation = Quaternion.AngleAxis(rotateAngle, Vector3.forward);
            }

            // combine rotations
            transform.rotation = lookRotation * offsetRotation;

            // scale pulsation
            float pingPongMin = -MinWaitTime * PulseSpeed * 0.5f;
            float pulse = Mathf.Clamp01(GameTools.PingPong(pingPongMin, 1f, PulseSpeed));
            float scale = Mathf.Lerp(PulseScale.RealMin, PulseScale.RealMax, pulse);

            scale *= scaleFade;
            transform.localScale = new(scale, scale, scale);
        }

        private void SetColorAlpha(float alpha)
        {
            Color color = meshRenderer.material.GetColor(ColorParam);
            color.a = alpha;
            meshRenderer.material.SetColor(ColorParam, color);
        }

        public void SetGlareVisibility(bool state)
        {
            GlareState = state;
        }
    }
}