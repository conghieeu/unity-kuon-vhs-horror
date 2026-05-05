using UnityEngine.Events;
using UnityEngine;
using UHFPS.Tools;
using Newtonsoft.Json.Linq;
using ThunderWire.Attributes;

namespace UHFPS.Runtime
{
    [Summary("Quản lý các sự kiện tương tác tự do (Interact Event), cho phép gắn các Unity Event khi Bắt đầu, Đang giữ và Thả nút tương tác.")]
    public class CustomInteractEvent : MonoBehaviour, IInteractStart, IInteractHold, IInteractStop, ISaveable
    {
        [Tooltip("Sử dụng sự kiện OnStart khi bắt đầu bấm nút tương tác.")]
        public bool UseOnStartEvent;

        [Tooltip("Sử dụng sự kiện OnHold khi đang giữ nút tương tác.")]
        public bool UseOnHoldEvent;

        [Tooltip("Sử dụng sự kiện OnStop khi thả nút tương tác.")]
        public bool UseOnStopEvent;

        [Tooltip("Đóng băng người chơi (Không cho di chuyển/xoay camera) khi tương tác.")]
        public bool FreezePlayer;

        [Tooltip("Chỉ cho phép tương tác một lần duy nhất.")]
        public bool InteractOnce;

        [Tooltip("Sử dụng âm thanh khi tương tác.")]
        public bool UseInteractSound;

        [Tooltip("Âm thanh phát ra khi tương tác.")]
        public SoundClip InteractSound;

        [Tooltip("Sự kiện gọi ra khi Bắt đầu bấm nút.")]
        public UnityEvent OnStart;

        [Tooltip("Sự kiện gọi ra liên tục khi Đang giữ nút.")]
        public UnityEvent<Vector3> OnHold;

        [Tooltip("Sự kiện gọi ra khi Thả nút.")]
        public UnityEvent OnStop;

        private PlayerPresenceManager playerPresence;
        private bool isInteracted;

        private void Awake()
        {
            playerPresence = PlayerPresenceManager.Instance;
        }

        public void InteractStart()
        {
            if (isInteracted) 
                return;

            if (UseOnStartEvent) OnStart?.Invoke();
            if (FreezePlayer) playerPresence.FreezePlayer(true);
            if (UseInteractSound) GameTools.PlayOneShot2D(transform.position, InteractSound, "InteractSound");
        }

        public void InteractHold(Vector3 point)
        {
            if (isInteracted) 
                return;

            if (UseOnHoldEvent) OnHold?.Invoke(point);
        }

        public void InteractStop()
        {
            if (isInteracted) 
                return;

            if (UseOnStopEvent) OnStop?.Invoke();
            if (FreezePlayer) playerPresence.FreezePlayer(false);
            if (InteractOnce) isInteracted = true;
        }

        public StorableCollection OnSave()
        {
            return new StorableCollection()
            {
                { "interactOnce", isInteracted }
            };
        }

        public void OnLoad(JToken data)
        {
            isInteracted = (bool)data["interactOnce"];
        }
    }
}