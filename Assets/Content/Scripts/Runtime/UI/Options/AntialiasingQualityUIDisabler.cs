using UnityEngine;
using ThunderWire.Attributes;

namespace UHFPS.Runtime
{
    [Summary("Vô hiệu hóa hoặc làm mờ các tùy chọn chất lượng khử răng cưa khi chế độ khử răng cưa hiện tại là Disabled hoặc FXAA.")]
    public class AntialiasingQualityUIDisabler : MonoBehaviour
    {
        [Tooltip("CanvasGroup chứa các tùy chọn chất lượng cần vô hiệu hóa.")]
        public CanvasGroup AntialiasingQuality;
        [Tooltip("Độ mờ khi bị vô hiệu hóa.")]
        public float DisabledAlpha = 0.5f;
        [Tooltip("Mảng trạng thái tương ứng với từng loại khử răng cưa (Disabled, FXAA, SMAA, TAA).")]
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