using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using Newtonsoft.Json.Linq;
using UHFPS.Tools;
using ThunderWire.Attributes;

namespace UHFPS.Runtime
{
    [Docs("https://docs.twgamesdev.com/uhfps/guides/jumpscares")]
    public class JumpscareTrigger : MonoBehaviour, ISaveable
    {
        public enum JumpscareTypeEnum { Direct, Indirect, Audio }
        public enum DirectTypeEnum { Image, Model }
        public enum TriggerTypeEnum { Event, TriggerEnter, TriggerExit }

        public JumpscareTypeEnum JumpscareType = JumpscareTypeEnum.Direct;
        public DirectTypeEnum DirectType = DirectTypeEnum.Image;
        public TriggerTypeEnum TriggerType = TriggerTypeEnum.Event;

        public Sprite JumpscareImage;
        public string JumpscareModelID = "scare_zombie";
        public SoundClip JumpscareSound;

        public Animator Animator;
        public string AnimatorStateName = "Jumpscare";
        public string AnimatorTrigger = "Jumpscare";

        public bool InfluenceFear;
        [Range(0f, 1f)] public float TentaclesIntensity = 0f;
        [Range(0.1f, 3f)] public float TentaclesSpeed = 1f;
        [Range(0f, 1f)] public float VignetteStrength = 0f;

        public bool LookAtJumpscare;
        public Transform LookAtTarget;
        public float LookAtDuration;
        public bool LockPlayer;
        public bool EndJumpscareWithEvent;

        public bool InfluenceWobble;
        public float WobbleAmplitudeGain = 1f;
        public float WobbleFrequencyGain = 1f;

        public float WobbleDuration = 0.2f;
        public float DirectDuration = 1f;
        public float FearDuration = 1f;

        public UnityEvent TriggerEnter;
        public UnityEvent TriggerExit;

        public UnityEvent OnJumpscareStarted;
        public UnityEvent OnJumpscareEnded;

        private bool jumpscareStarted;
        private bool triggerEntered;

        private JumpscareManager jumpscareManager;

        private void Awake()
        {
            jumpscareManager = JumpscareManager.Instance;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (TriggerType == TriggerTypeEnum.Event)
                return;

            if (other.CompareTag("Player") && !jumpscareStarted && !triggerEntered)
            {
                TriggerEnter?.Invoke();

                if (TriggerType == TriggerTypeEnum.TriggerEnter)
                {
                    TriggerJumpscare();
                }
                else if (TriggerType == TriggerTypeEnum.TriggerExit)
                {
                    triggerEntered = true;
                }
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (TriggerType == TriggerTypeEnum.Event)
                return;

            if (other.CompareTag("Player") && !jumpscareStarted && triggerEntered)
            {
                TriggerExit?.Invoke();

                if (TriggerType == TriggerTypeEnum.TriggerExit)
                {
                    TriggerJumpscare();
                }
            }
        }

        public void TriggerJumpscare()
        {
            if (jumpscareStarted)
                return;

            OnJumpscareStarted?.Invoke();

            if(JumpscareType == JumpscareTypeEnum.Indirect)
            {
                Animator.SetTrigger(AnimatorTrigger);
                StartCoroutine(IndirectJumpscare());
            }

            jumpscareManager.StartJumpscareEffect(this);
            GameTools.PlayOneShot2D(transform.position, JumpscareSound, "Jumpscare Sound");

            jumpscareStarted = true;
        }

        public void TriggerJumpscareEnded()
        {
            if (EndJumpscareWithEvent) jumpscareManager.EndJumpscareEffect();
        }

        IEnumerator IndirectJumpscare()
        {
            yield return new WaitForAnimatorClip(Animator, AnimatorStateName);
            if (!EndJumpscareWithEvent) jumpscareManager.EndJumpscareEffect();
            OnJumpscareEnded?.Invoke();
        }

        public StorableCollection OnSave()
        {
            return new StorableCollection()
            {
                { nameof(jumpscareStarted), jumpscareStarted }
            };
        }

        public void OnLoad(JToken data)
        {
            jumpscareStarted = (bool)data[nameof(jumpscareStarted)];
        }
    }
}