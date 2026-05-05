using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using Newtonsoft.Json.Linq;
using UHFPS.Tools;
using ThunderWire.Attributes;

namespace UHFPS.Runtime
{
    [Summary("Điều khiển việc phát Animation và gọi sự kiện (UnityEvent) khi bắt đầu/kết thúc quá trình phát.")]
    public class AnimationPlayEvent : MonoBehaviour, ISaveable
    {
        [Tooltip("Thành phần Animator điều khiển hoạt ảnh.")]
        public Animator Animator;
        [Tooltip("Tên tham số Trigger dùng để kích hoạt Animation (nếu không dùng UseOnlyState).")]
        public string TriggerName;
        [Tooltip("Tên trạng thái Animation sẽ được phát trực tiếp (nếu dùng UseOnlyState) hoặc để kiểm tra thời gian kết thúc.")]
        public string StateName;
        [Tooltip("Khoảng thời gian bù trừ (Offset) trước khi sự kiện kết thúc (OnAnimationEnd) được gọi.")]
        public float EndEventTimeOffset;
        [Tooltip("Bật tùy chọn này để phát trực tiếp trạng thái Animation (Play) thay vì dùng Trigger.")]
        public bool UseOnlyState;
        [Tooltip("Bật tùy chọn này để cho phép phát Animation nhiều lần.")]
        public bool PlayMoreTimes;

        [Tooltip("Sự kiện Unity được gọi ngay khi Animation bắt đầu phát.")]
        public UnityEvent OnAnimationStart;
        [Tooltip("Sự kiện Unity được gọi khi Animation kết thúc (sau khi chờ hết thời lượng clip).")]
        public UnityEvent OnAnimationEnd;

        private bool isPlayed;

        public void PlayAnimation()
        {
            if (Animator.IsAnyPlaying() && !isPlayed)
                return;

            if (UseOnlyState) Animator.Play(StateName);
            else Animator.SetTrigger(TriggerName);

            StartCoroutine(OnAnimationPlay());
            OnAnimationStart?.Invoke();
            isPlayed = !PlayMoreTimes;
        }

        IEnumerator OnAnimationPlay()
        {
            yield return new WaitForEndOfFrame();
            yield return new WaitForAnimatorClip(Animator, StateName, EndEventTimeOffset);
            OnAnimationEnd?.Invoke();
        }

        public StorableCollection OnSave()
        {
            return new StorableCollection()
            {
                { nameof(isPlayed), isPlayed }
            };
        }

        public void OnLoad(JToken data)
        {
            isPlayed = (bool)data[nameof(isPlayed)];
        }
    }
}