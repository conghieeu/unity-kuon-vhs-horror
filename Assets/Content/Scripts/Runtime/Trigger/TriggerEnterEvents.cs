using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using Newtonsoft.Json.Linq;

namespace UHFPS.Runtime
{
    public class TriggerEnterEvents : MonoBehaviour, ISaveable
    {
        public Tag[] TriggerTags;
        public bool TriggerOnce;
        public float TriggerStayRate;

        public UnityEvent<Collider> TriggerEnter;
        public UnityEvent<Collider> TriggerExit;
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