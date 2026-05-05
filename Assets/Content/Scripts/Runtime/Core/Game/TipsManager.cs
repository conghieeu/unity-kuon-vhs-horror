using System.Collections;
using UnityEngine;
using UHFPS.Tools;
using TMPro;
using ThunderWire.Attributes;

namespace UHFPS.Runtime
{
    [InspectorHeader("Tips Manager")]
    [Summary("Quản lý việc hiển thị các Mẹo (Tips) ngẫu nhiên trên màn hình Loading hoặc trong Game.")]
    public class TipsManager : MonoBehaviour
    {
        [Tooltip("Danh sách các chuỗi văn bản dùng làm Mẹo (Hỗ trợ Localization).")]
        public GString[] TipsList;

        [Header("References")]
        [Tooltip("CanvasGroup dùng để hiệu ứng mờ dần (Fade in/out) danh sách Tip.")]
        public CanvasGroup TipsGroup;

        [Tooltip("Text dùng để hiển thị nội dung Tip.")]
        public TMP_Text TipText;

        [Header("Settings")]
        [Tooltip("Thời gian hiển thị một Tip (tính bằng giây).")]
        public float TipTime = 5f;

        [Tooltip("Tốc độ chuyển đổi Fade giữa các Tip.")]
        public float TipChangeSpeed = 1f;

        private int lastTip;

        private void Awake()
        {
            for (int i = 0; i < TipsList.Length; i++)
            {
                TipsList[i].SubscribeGloc();
            }
        }

        public void StopTips()
        {
            StopAllCoroutines();
        }

        IEnumerator Start()
        {
            if (TipsList.Length == 1)
            {
                TipText.text = TipsList[0];
                TipsGroup.alpha = 1f;
            }
            else if (TipsList.Length > 1)
            {
                TipsGroup.alpha = 0f;

                while (true)
                {
                    lastTip = GameTools.RandomUnique(0, TipsList.Length, lastTip);
                    TipText.text = TipsList[lastTip];

                    yield return CanvasGroupFader.StartFade(TipsGroup, true, TipChangeSpeed);
                    yield return new WaitForSeconds(TipTime);
                    yield return CanvasGroupFader.StartFade(TipsGroup, false, TipChangeSpeed);
                }
            }
        }
    }
}