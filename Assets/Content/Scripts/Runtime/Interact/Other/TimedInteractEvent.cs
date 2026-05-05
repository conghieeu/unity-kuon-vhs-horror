using UnityEngine.Events;
using UnityEngine;
using UHFPS.Tools;
using Newtonsoft.Json.Linq;
using ThunderWire.Attributes;

namespace UHFPS.Runtime
{
    [Summary("Sự kiện tương tác cần giữ nút (Hold) trong một khoảng thời gian nhất định (Ví dụ: Giữ E trong 2s để mở cửa).")]
    public class TimedInteractEvent : MonoBehaviour, IInteractStart, IInteractStop, IInteractTimed, ISaveable
    {
        [field: SerializeField]
        [Tooltip("Thời gian (giây) cần giữ nút để hoàn thành tương tác.")]
        public float InteractTime { get; set; }

        [Tooltip("Chỉ cho phép hoàn thành tương tác này 1 lần duy nhất.")]
        public bool InteractOnce;

        [Tooltip("Sử dụng cơ chế Reset (Cho phép tương tác lại để khôi phục trạng thái).")]
        public bool UseResetInteract;

        [Tooltip("Yêu cầu người chơi phải có vật phẩm cụ thể trong kho đồ.")]
        public bool RequireInventoryItem;

        [Tooltip("ID của vật phẩm yêu cầu.")]
        public ItemGuid RequiredItem;

        [Tooltip("Hiển thị thông báo gợi ý nếu người chơi không có vật phẩm yêu cầu.")]
        public bool ShowRequireItemHint;

        [Tooltip("Thời gian hiển thị thông báo.")]
        public float HintMessageTime = 2f;

        [Tooltip("Nội dung thông báo gợi ý.")]
        public GString HintMessage;

        [Tooltip("Âm thanh phát ra khi quá trình giữ nút (Timed) hoàn thành.")]
        public SoundClip InteractSound;

        [Tooltip("Âm thanh phát ra khi quá trình Reset hoàn thành.")]
        public SoundClip ResetSound;

        [Tooltip("Sự kiện gọi ra khi tương tác Hold thành công.")]
        public UnityEvent OnInteract;

        [Tooltip("Sự kiện gọi ra khi thao tác Reset thành công.")]
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