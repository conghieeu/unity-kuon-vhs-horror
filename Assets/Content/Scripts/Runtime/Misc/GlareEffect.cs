using ThunderWire.Attributes;
using UHFPS.Tools;
using UnityEngine;

namespace UHFPS.Runtime
{
    [InspectorHeader("Glare Effect")]
    [Summary("Tạo hiệu ứng lóa sáng (Glare) luôn hướng về phía camera (Billboard).")]
    public class GlareEffect : MonoBehaviour
    {
        [Tooltip("Tên tham số màu sắc trong Shader.")]
        public string ColorParam = "_BaseColor";
        [Tooltip("Trạng thái hiển thị hiện tại.")]
        public bool GlareState = true;

        [Tooltip("Khoảng cách mà hiệu ứng lóa sáng sẽ thay đổi kích thước.")]
        public MinMax ScaleDistance;

        [Range(0f, 1f), Tooltip("Khoảng cách tối thiểu để bắt đầu thay đổi tỉ lệ (Scale).")]
        public float MinScaleDistance = 0.5f;

        [Tooltip("Khoảng cách mà vật thể sẽ mờ dần (Fade) khi camera quá gần hoặc quá xa.")]
        public MinMax NearFarDistance;

        [Tooltip("Khoảng cách chuyển đổi mờ dần.")]
        public float BlendDistance = 0.5f;

        [Tooltip("Giới hạn tỉ lệ rung động (Min/Max).")]
        public MinMax PulseScale;

        [Tooltip("Thời gian chờ ở mức rung tối thiểu.")]
        public float MinWaitTime = 1f;

        [Tooltip("Tốc độ rung động.")]
        public float PulseSpeed = 1f;
        [Tooltip("Tốc độ xoay.")]
        public float RotateSpeed = 3f;

        [Tooltip("Bật/tắt việc thay đổi kích thước theo khoảng cách.")]
        public bool EnableDistanceScaling = true;
        [Tooltip("Bật/tắt việc mờ dần khi camera ở gần.")]
        public bool EnableNearFading = true;
        [Tooltip("Bật/tắt việc xoay tròn hiệu ứng.")]
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