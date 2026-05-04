using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace UHFPS.Runtime
{
    public class ItemPickupElement : MonoBehaviour
    {
        public TMP_Text PickupText;
        public Image PickupIcon;

        [Header("Fit Settings")]
        public bool FitIcon = true;
        public float FitSize = 50f;

        [Header("Animation")]
        public Animator Animator;
        public string ShowAnimation = "Show";
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