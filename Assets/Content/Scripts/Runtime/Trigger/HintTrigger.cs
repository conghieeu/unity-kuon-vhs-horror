using Newtonsoft.Json.Linq;
using ThunderWire.Attributes;
using UnityEngine;
using UnityEngine.Events;

namespace UHFPS.Runtime
{
    [InspectorHeader("Hint Trigger")]
    [Summary("Kích hoạt hiển thị thông báo gợi ý (Hint) trên màn hình khi người chơi bước vào hoặc thoát khỏi vùng Trigger.")]
    public class HintTrigger : MonoBehaviour, ISaveable
    {
        public enum TriggerTypeEnum { TriggerEnter, TriggerExit, Event }

        [Tooltip("Loại kích hoạt: Khi bước vào (TriggerEnter), khi thoát ra (TriggerExit) hoặc kích hoạt bằng sự kiện ngoài (Event).")]
        public TriggerTypeEnum TriggerType = TriggerTypeEnum.TriggerEnter;
        [Tooltip("Nội dung tin nhắn gợi ý.")]
        public GString HintMessage;
        [Tooltip("Thời gian (giây) hiển thị thông báo.")]
        public float MessageTime;

        [Header("Settings")]
        [Tooltip("Có cho phép hiển thị gợi ý này nhiều lần không?")]
        public bool ShowMoreTimes;
        [Tooltip("Chỉ gọi sự kiện (OnHintShowed) một lần duy nhất, ngay cả khi gợi ý hiển thị nhiều lần.")]
        public bool CallEventOnce;

        [Header("Events")]
        [Tooltip("Sự kiện gọi ra khi gợi ý được hiển thị.")]
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