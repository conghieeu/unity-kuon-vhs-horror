using System.Collections;
using UnityEngine;
using ThunderWire.Attributes;
using TMPro;

namespace UHFPS.Runtime
{
    [InspectorHeader("Objective Notification")]
    [Summary("Quản lý thông báo (Notification) góc màn hình khi nhận hoặc hoàn thành nhiệm vụ.")]
    public class ObjectiveNotification : MonoBehaviour
    {
        [Tooltip("Animator để điều khiển hoạt ảnh hiển thị/ẩn thông báo.")]
        public Animator Animator;

        [Tooltip("UI Text hiển thị nội dung thông báo.")]
        public TMP_Text Title;

        [Header("Animation")]
        [Tooltip("Trigger kích hoạt hoạt ảnh hiện thông báo.")]
        public string ShowTrigger = "Show";

        [Tooltip("Trigger kích hoạt hoạt ảnh ẩn thông báo.")]
        public string HideTrigger = "Hide";

        [Tooltip("Tên của trạng thái (State) trong Animator khi thông báo đã bị ẩn hoàn toàn.")]
        public string HideState = "Hide";

        private bool isShowed;

        public void ShowNotification(string title, float duration)
        {
            if (isShowed)
                return;

            Title.text = title;
            Animator.SetTrigger(ShowTrigger);
            StartCoroutine(OnShowNotification(duration));
            isShowed = true;
        }

        IEnumerator OnShowNotification(float duration)
        {
            yield return new WaitForSeconds(duration);
            Animator.SetTrigger(HideTrigger);
            yield return new WaitForAnimatorStateExit(Animator, HideState);
            isShowed = false;
        }
    }
}