using UnityEngine;
using ThunderWire.Attributes;
using TMPro;

namespace UHFPS.Runtime
{
    [InspectorHeader("FPS Counter")]
    [Summary("Hiển thị số khung hình trên giây (FPS) lên màn hình.")]
    public class FPSCounter : MonoBehaviour
    {
        [Tooltip("Text UI hiển thị số FPS.")]
        public TMP_Text FPSText;

        [Header("Settings")]
        [Range(0f, 1f)]
        [Tooltip("Hệ số làm mượt giá trị FPS.")]
        public float ExpSmoothingFactor = 0.9f;
        [Tooltip("Tần suất cập nhật Text (giây).")]
        public float RefreshFrequency = 0.4f;

        private float timeSinceUpdate = 0f;
        private float averageFps = 1f;
        private bool disableCounter;

        private void Update()
        {
            if (disableCounter)
                return;

            averageFps = ExpSmoothingFactor * averageFps + (1f - ExpSmoothingFactor) * 1f / Time.unscaledDeltaTime;

            if (timeSinceUpdate < RefreshFrequency)
            {
                timeSinceUpdate += Time.deltaTime;
                return;
            }

            int fps = Mathf.RoundToInt(averageFps);
            FPSText.text = $"{fps} FPS";
            timeSinceUpdate = 0f;
        }

        public void ShowFPS(bool state)
        {
            disableCounter = state == false;
            FPSText.enabled = !disableCounter;
        }
    }
}