using UnityEngine;
using ThunderWire.Attributes;
using TMPro;

namespace UHFPS.Runtime
{
    [InspectorHeader("FPS Counter")]
    public class FPSCounter : MonoBehaviour
    {
        public TMP_Text FPSText;

        [Header("Settings")]
        [Range(0f, 1f)]
        public float ExpSmoothingFactor = 0.9f;
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