using UnityEngine;
using ThunderWire.Attributes;

namespace UHFPS.Runtime
{
    [InspectorHeader("Flame Flicker")]
    [Summary("Tạo hiệu ứng ánh sáng bập bùng (flicker) cho ngọn lửa. Hỗ trợ thay đổi cường độ ánh sáng và vị trí ngọn lửa.")]
    public class FlameFlicker : MonoBehaviour
    {
        [Tooltip("Ánh sáng ngọn lửa mục tiêu.")]
        public Light FlameLight;
        [Tooltip("Giới hạn cường độ ánh sáng (Min/Max).")]
        public MinMax FlameFlickerLimits;
        [Tooltip("Tốc độ bập bùng của ánh sáng.")]
        public float FlameFlickerSpeed = 1f;

        [Header("Position Flicker")]
        [Tooltip("Bật/tắt hiệu ứng rung vị trí ngọn lửa.")]
        public bool PositionFlicker;
        [Tooltip("Tốc độ rung vị trí.")]
        public float PositionFlickerSpeed = 1f;
        [Tooltip("Biên độ rung vị trí (X, Y, Z).")]
        public Vector3 PositionFlickerMagnitude = new(0.1f, 0.1f, 0.1f);

        [Header("Optimization")]
        [Tooltip("Tự động ngừng tính toán khi người chơi ở xa.")]
        public bool Optimize = true;
        [Tooltip("Khoảng cách tối đa để hiệu ứng hoạt động.")]
        public float FlickerDistance = 10f;

        private Transform player;
        private Vector3 originalPosition;

        private void Awake()
        {
            player = PlayerPresenceManager.Instance.Player.transform;
            originalPosition = transform.position;
        }

        private void Update()
        {
            if (!FlameLight.enabled)
                return;

            // Optimization
            if (Optimize && Vector3.Distance(transform.position, player.position) > FlickerDistance)
                return;

            // Intensity Flicker
            float flicker = Mathf.PerlinNoise1D(Time.time * FlameFlickerSpeed);
            FlameLight.intensity = Mathf.Lerp(FlameFlickerLimits.RealMin, FlameFlickerLimits.RealMax, flicker);

            // Position Flicker
            if (PositionFlicker)
            {
                float xOffset = Perlin(Time.time * PositionFlickerSpeed, 1f);
                float yOffset = Perlin(Time.time * PositionFlickerSpeed, 2f);
                float zOffset = Perlin(Time.time * PositionFlickerSpeed, 3f);

                Vector3 flickerPosition = new(xOffset, yOffset, zOffset);
                transform.position = originalPosition + Vector3.Scale(flickerPosition, PositionFlickerMagnitude);
            }
        }

        private float Perlin(float x, float y)
        {
            float value = Mathf.PerlinNoise(x, y);
            return (value - 0.5f) * 2;
        }
    }
}