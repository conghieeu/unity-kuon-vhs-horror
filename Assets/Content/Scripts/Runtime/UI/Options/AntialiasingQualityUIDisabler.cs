using UnityEngine;

namespace UHFPS.Runtime
{
    public class AntialiasingQualityUIDisabler : MonoBehaviour
    {
        public CanvasGroup AntialiasingQuality;
        public float DisabledAlpha = 0.5f;
        public bool[] AntialiasingState = new[]
        {
            false, // disabled
            false, // FXAA
            true, // SMAA
            true // TAA
        };

        public void OnAntialiasingChange(int type)
        {
            bool state = AntialiasingState[type];
            AntialiasingQuality.alpha = state ? 1f : DisabledAlpha;
            AntialiasingQuality.blocksRaycasts = state;
        }
    }
}