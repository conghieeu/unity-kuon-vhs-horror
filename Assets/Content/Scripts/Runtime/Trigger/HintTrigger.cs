using Newtonsoft.Json.Linq;
using ThunderWire.Attributes;
using UnityEngine;
using UnityEngine.Events;

namespace UHFPS.Runtime
{
    [InspectorHeader("Hint Trigger")]
    public class HintTrigger : MonoBehaviour, ISaveable
    {
        public enum TriggerTypeEnum { TriggerEnter, TriggerExit, Event }

        public TriggerTypeEnum TriggerType = TriggerTypeEnum.TriggerEnter;
        public GString HintMessage;
        public float MessageTime;

        [Header("Settings")]
        public bool ShowMoreTimes;
        public bool CallEventOnce;

        [Header("Events")]
        public UnityEvent OnHintShowed;

        private bool isTriggered;
        private bool isEventCalled;
        private bool triggerEntered;

        private GameManager gameManager;

        private void Awake()
        {
            gameManager = GameManager.Instance;
        }

        private void Start()
        {
            HintMessage.SubscribeGloc();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (TriggerType == TriggerTypeEnum.Event)
                return;

            if (other.CompareTag("Player") && !isTriggered && !triggerEntered)
            {
                if (TriggerType == TriggerTypeEnum.TriggerEnter)
                {
                    TriggerHint();
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

            if (other.CompareTag("Player") && !isTriggered && triggerEntered)
            {
                if (TriggerType == TriggerTypeEnum.TriggerExit)
                {
                    TriggerHint();
                }
            }
        }

        public void TriggerHint()
        {
            if (isTriggered)
                return;

            gameManager.ShowHintMessage(HintMessage, MessageTime);

            if (!isEventCalled)
            {
                OnHintShowed?.Invoke();
                isEventCalled = ShowMoreTimes && CallEventOnce;
            }

            isTriggered = !ShowMoreTimes;
        }

        public void DisableTrigger()
        {
            isTriggered = true;
        }

        public StorableCollection OnSave()
        {
            return new StorableCollection()
            {
                { nameof(isTriggered), isTriggered },
                { nameof(isEventCalled), isEventCalled },
            };
        }

        public void OnLoad(JToken data)
        {
            isTriggered = (bool)data[nameof(isTriggered)];
            isEventCalled = (bool)data[nameof(isEventCalled)];
        }
    }
}