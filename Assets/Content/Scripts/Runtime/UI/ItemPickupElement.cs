using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ThunderWire.Attributes;

namespace UHFPS.Runtime
{
    [Summary("Xử lý việc hiển thị thông báo nhặt vật phẩm trên HUD.")]
    public class ItemPickupElement : MonoBehaviour
    {
        [Tooltip("Text hiển thị tên vật phẩm.")]
        public TMP_Text PickupText;
        [Tooltip("Icon của vật phẩm.")]
        public Image PickupIcon;

        [Header("Fit Settings")]
        [Tooltip("Tự động căn chỉnh kích thước icon cho vừa với ô hiển thị.")]
        public bool FitIcon = true;
        [Tooltip("Kích thước mục tiêu để căn chỉnh icon.")]
        public float FitSize = 50f;

        [Header("Animation")]
        [Tooltip("Animator xử lý hiệu ứng hiện/ẩn thông báo.")]
        public Animator Animator;
        [Tooltip("Tên Trigger/State để hiện thông báo.")]
        public string ShowAnimation = "Show";
        [Tooltip("Tên Trigger/State để ẩn thông báo.")]
        public string HideAnimation = "Hide";

        public void ShowItemPickup(string text, Sprite icon, float time)
        {
            PickupText.text = text;
            PickupIcon.sprite = icon;

            Vector2 slotSize = Vector2.one * FitSize;
            Vector2 iconSize = icon.rect.size;

            Vector2 scaleRatio = slotSize / iconSize;
            float scaleFactor = Mathf.Min(scaleRatio.x, scaleRatio.y);
            PickupIcon.rectTransform.sizeDelta = iconSize * scaleFactor;

            StartCoroutine(OnShowPickupElement(time));
        }

        IEnumerator OnShowPickupElement(float time)
        {
            Animator.SetTrigger(ShowAnimation);
            yield return new WaitForAnimatorClip(Animator, ShowAnimation);

            yield return new WaitForSeconds(time);

            Animator.SetTrigger(HideAnimation);
            yield return new WaitForAnimatorClip(Animator, HideAnimation);

            yield return new WaitForEndOfFrame();
            Destroy(gameObject);
        }
    }
}