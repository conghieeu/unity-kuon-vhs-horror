using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using Newtonsoft.Json.Linq;
using ThunderWire.Attributes;
using UHFPS.Tools;

namespace UHFPS.Runtime
{
    [InspectorHeader("Trigger Enter Events")]
    [Summary("Cung cấp các sự kiện Unity cơ bản (Enter, Stay, Exit) khi một đối tượng thuộc Tag chỉ định chạm vào Trigger.")]
    public class TriggerEnterEvents : MonoBehaviour, ISaveable
    {
        [Tooltip("Danh sách các Tag được phép kích hoạt sự kiện.")]
        public Tag[] TriggerTags;
        [Tooltip("Chỉ cho phép kích hoạt sự kiện một lần duy nhất.")]
        public bool TriggerOnce;
        [Tooltip("Khoảng thời gian (giây) giữa các lần gọi sự kiện TriggerStay.")]
        public float TriggerStayRate;

        [Tooltip("Sự kiện gọi ra khi đối tượng bước vào vùng Trigger.")]
        public UnityEvent<Collider> TriggerEnter;
        [Tooltip("Sự kiện gọi ra khi đối tượng thoát khỏi vùng Trigger.")]
        public UnityEvent<Collider> TriggerExit;
        [Tooltip("Sự kiện gọi ra khi đối tượng đang ở trong vùng Trigger.")]
        public UnityEvent<Collider> TriggerStay;

        private float triggerTime;
        private bool triggerOnce;
        private bool isTriggerEnter;

        private void OnTriggerEnter(Collider other)
        {
            if(TriggerTags.Any(x => other.CompareTag(x)) && !triggerOnce)
            {
                TriggerEnter?.Invoke(other);
                triggerOnce = TriggerOnce;
                isTriggerEnter = true;
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (TriggerTags.Any(x => other.CompareTag(x)) && (!triggerOnce || isTriggerEnter))
            {
                TriggerExit?.Invoke(other);
                isTriggerEnter = false;
            }
        }

        private void OnTriggerStay(Collider other)
        {
            if (TriggerTags.Any(x => other.CompareTag(x)) && (!triggerOnce || isTriggerEnter) && triggerTime <= 0)
            {
                TriggerStay?.Invoke(other);
                triggerTime = TriggerStayRate;
                isTriggerEnter = true;
            }
        }

        private void Update()
        {
            if (isTriggerEnter && triggerTime > 0)
                triggerTime -= Time.deltaTime;
        }

        public StorableCollection OnSave()
        {
            return new StorableCollection()
            {
                { nameof(triggerOnce), triggerOnce },
                { nameof(triggerTime), triggerTime },
            };
        }

        public void OnLoad(JToken data)
        {
            triggerOnce = (bool)data[nameof(triggerOnce)];
            triggerTime = (float)data[nameof(triggerTime)];
        }
    }
}