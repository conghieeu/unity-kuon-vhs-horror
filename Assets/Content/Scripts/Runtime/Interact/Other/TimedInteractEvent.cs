using UnityEngine.Events;
using UnityEngine;
using UHFPS.Tools;
using Newtonsoft.Json.Linq;

namespace UHFPS.Runtime
{
    public class TimedInteractEvent : MonoBehaviour, IInteractStart, IInteractStop, IInteractTimed, ISaveable
    {
        [field: SerializeField]
        public float InteractTime { get; set; }

        public bool InteractOnce;
        public bool UseResetInteract;

        public bool RequireInventoryItem;
        public ItemGuid RequiredItem;

        public bool ShowRequireItemHint;
        public float HintMessageTime = 2f;
        public GString HintMessage;

        public SoundClip InteractSound;
        public SoundClip ResetSound;

        public UnityEvent OnInteract;
        public UnityEvent OnReset;

        public bool ContainsRequiredItem => !RequireInventoryItem || Inventory.Instance.ContainsItem(RequiredItem);

        public bool IsResetState => isInteractTimed;

        public bool NoInteract
        {
            get
            {
                if (!ContainsRequiredItem)
                    return true;

                return noInteract;
            }
        }

        private bool noInteract;
        private bool isInteractTimed;
        private bool isInteractStart;

        private void Start()
        {
            HintMessage.SubscribeGloc();
        }

        public void InteractTimed()
        {
            if (NoInteract || isInteractTimed)
                return;

            OnInteract?.Invoke();
            noInteract = InteractOnce || UseResetInteract;
            isInteractTimed = UseResetInteract;
            GameTools.PlayOneShot2D(transform.position, InteractSound, "InteractSound");
        }

        public void InteractStart()
        {
            if (ShowRequireItemHint && !ContainsRequiredItem)
                GameManager.Instance.ShowHintMessage(HintMessage, HintMessageTime);

            if (!isInteractTimed)
                return;

            OnReset?.Invoke();
            GameTools.PlayOneShot2D(transform.position, ResetSound, "ResetSound");
            isInteractStart = true;
        }

        public void InteractStop()
        {
            if (!isInteractTimed || !isInteractStart)
                return;

            noInteract = InteractOnce;
            isInteractTimed = false;
            isInteractStart = false;
        }

        public StorableCollection OnSave()
        {
            return new StorableCollection()
            {
                { "interactOnce", NoInteract },
                { "isInteractTimed", isInteractTimed }
            };
        }

        public void OnLoad(JToken data)
        {
            noInteract = (bool)data["interactOnce"];
            isInteractTimed = (bool)data["isInteractTimed"];
        }
    }
}