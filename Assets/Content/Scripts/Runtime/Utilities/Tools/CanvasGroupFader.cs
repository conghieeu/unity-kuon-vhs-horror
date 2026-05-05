using System;
using System.Collections;
using UnityEngine;

namespace UHFPS.Tools
{
    /// <summary>
    /// Công cụ hỗ trợ Fade In/Out cho CanvasGroup bằng Coroutine.
    /// </summary>
    [ThunderWire.Attributes.Summary("Hỗ trợ hiệu ứng làm mờ (Fade In/Out) cho CanvasGroup.")]
    public class CanvasGroupFader : MonoBehaviour
    {
        /// <summary>
        /// Khởi tạo một instance tạm thời để thực hiện hiệu ứng Fade.
        /// </summary>
        public static void StartFadeInstance(CanvasGroup canvasGroup, bool fadeIn, float speed, Action onFade = null)
        {
            GameObject fadeGO = new("CanvasGroupFader");
            CanvasGroupFader canvasGroupFader = fadeGO.AddComponent<CanvasGroupFader>();
            canvasGroupFader.StartCoroutine(StartFade(canvasGroup, fadeIn, speed, () =>
            {
                onFade?.Invoke();
                Destroy(fadeGO);
            }));
        }

        public static IEnumerator StartFade(CanvasGroup canvasGroup, bool fadeIn, float speed, Action onFade = null)
        {
            canvasGroup.gameObject.SetActive(true);
            float currAlpha = canvasGroup.alpha;
            float targetAlpha = fadeIn ? 1f : 0f;

            while (fadeIn ? currAlpha < 1 : currAlpha > 0)
            {
                currAlpha = Mathf.MoveTowards(currAlpha, targetAlpha, Time.deltaTime * speed);
                canvasGroup.alpha = currAlpha;
                yield return null;
            }

            canvasGroup.alpha = targetAlpha;
            onFade?.Invoke();
        }
    }
}