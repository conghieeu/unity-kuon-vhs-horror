using System.Collections;
using UnityEngine;
using ThunderWire.Attributes;
using TMPro;

namespace UHFPS.Runtime
{
    [InspectorHeader("Objective Notification")]
    public class ObjectiveNotification : MonoBehaviour
    {
        public Animator Animator;
        public TMP_Text Title;

        [Header("Animation")]
        public string ShowTrigger = "Show";
        public string HideTrigger = "Hide";
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