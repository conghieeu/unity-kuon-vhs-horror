using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using Newtonsoft.Json.Linq;
using UHFPS.Tools;

namespace UHFPS.Runtime
{
    public class AnimationPlayEvent : MonoBehaviour, ISaveable
    {
        public Animator Animator;
        public string TriggerName;
        public string StateName;
        public float EndEventTimeOffset;
        public bool UseOnlyState;
        public bool PlayMoreTimes;

        public UnityEvent OnAnimationStart;
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