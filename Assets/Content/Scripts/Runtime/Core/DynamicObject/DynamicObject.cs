using UnityEngine;
using UnityEngine.Events;
using UHFPS.Input;
using UHFPS.Tools;
using ThunderWire.Attributes;
using Newtonsoft.Json.Linq;

namespace UHFPS.Runtime
{
    public enum DynamicSoundType { Open, Close, Locked, Unlock }

    [Docs("https://docs.twgamesdev.com/uhfps/guides/dynamic-objects")]
    [Summary("Quản lý các vật thể có thể tương tác động bằng vật lý hoặc animation (Cửa, Ngăn kéo, Van xoay, Công tắc...). Bao gồm cả hệ thống khóa bằng chìa (Item) hoặc code.")]
    public class DynamicObject : MonoBehaviour, IInteractStartPlayer, IInteractHold, IInteractStop, ISaveable
    {
        public enum DynamicType { Openable, Pullable, Switchable, Rotable }
        public enum TransformType { Local, Global }
        public enum InteractType { Dynamic, Mouse, Animation }
        public enum DynamicStatus { Normal, Locked }
        public enum StatusChange { InventoryItem, CustomScript, None }

        // enums
        [Tooltip("Kiểu chuyển động: Openable (Cửa mở xoay), Pullable (Ngăn kéo kéo ra), Switchable (Công tắc), Rotable (Van xoay tròn).")]
        public DynamicType dynamicType = DynamicType.Openable;

        [Tooltip("Hệ tọa độ dùng để tính toán di chuyển/xoay (Thường dùng Local).")]
        public TransformType transformType = TransformType.Local;

        [Tooltip("Trạng thái bắt đầu: Normal (Mở được luôn), Locked (Bị khóa).")]
        public DynamicStatus dynamicStatus = DynamicStatus.Normal;

        [Tooltip("Cách tương tác: Dynamic (Kéo/đẩy chuột vật lý), Mouse (Click chuột), Animation (Phát animation bằng code).")]
        public InteractType interactType = InteractType.Dynamic;

        [Tooltip("Cách mở khóa (nếu bị khóa): InventoryItem (Dùng chìa trong túi đồ), CustomScript (Logic riêng bằng code), None (Không khóa).")]
        public StatusChange statusChange = StatusChange.InventoryItem;

        // general
        [Tooltip("Transform gốc của vật thể sẽ bị di chuyển/xoay.")]
        public Transform target;
        public AudioSource audioSource;
        public Animator animator;
        public HingeJoint joint;
        public new Rigidbody rigidbody;
        public Inventory inventory;
        public GameManager gameManager;

        // items
        [RequireInterface(typeof(IDynamicUnlock))]
        [Tooltip("Script xử lý logic mở khóa tự tạo (Dùng khi StatusChange = CustomScript).")]
        public MonoBehaviour unlockScript;

        [Tooltip("Giữ lại chìa khóa trong túi đồ sau khi mở (Nếu tắt, chìa sẽ bị xóa sau khi dùng).")]
        public bool keepUnlockItem;

        [Tooltip("Mã định danh (GUID) của vật phẩm (Chìa khóa) dùng để mở khóa cửa này.")]
        public ItemGuid unlockItem;

        [Tooltip("Hiển thị text thông báo khi cố mở cửa đang bị khóa.")]
        public bool showLockedText;

        [Tooltip("Nội dung text thông báo cửa bị khóa (Hỗ trợ Localization).")]
        public GString lockedText;

        [Tooltip("Danh sách các Collider sẽ bị bỏ qua va chạm với vật thể này.")]
        public Collider[] ignoreColliders;

        [Tooltip("Bỏ qua va chạm giữa vật thể này và người chơi (Dùng cho ngăn kéo tủ để tránh đẩy người chơi ra xa).")]
        public bool ignorePlayerCollider;

        [Tooltip("Tên Animator Trigger mở (Mặc định: Open).")]
        public string useTrigger1 = "Open";

        [Tooltip("Tên Animator Trigger đóng (Mặc định: Close).")]
        public string useTrigger2 = "Close";

        [Tooltip("Tên Animator Trigger mở bên (Dành cho cửa mở 2 chiều).")]
        public string useTrigger3 = "OpenSide";

        // dynamic types
        public DynamicOpenable openable = new DynamicOpenable();
        public DynamicPullable pullable = new DynamicPullable();
        public DynamicSwitchable switchable = new DynamicSwitchable();
        public DynamicRotable rotable = new DynamicRotable();

        // sounds
        [Tooltip("Âm thanh phát ra khi mở.")]
        public SoundClip useSound1;

        [Tooltip("Âm thanh phát ra khi đóng.")]
        public SoundClip useSound2;

        [Tooltip("Âm thanh phát ra khi cố mở vật bị khóa.")]
        public SoundClip lockedSound;

        [Tooltip("Âm thanh phát ra khi mở khóa (Dùng chìa thành công).")]
        public SoundClip unlockSound;

        // events
        [Tooltip("Sự kiện gọi ra khi Mở.")]
        public UnityEvent useEvent1;

        [Tooltip("Sự kiện gọi ra khi Đóng.")]
        public UnityEvent useEvent2;

        [Tooltip("Sự kiện gọi ra liên tục khi thay đổi giá trị (VD: Kéo Van xoay, truyền ra giá trị xoay hiện tại).")]
        public UnityEvent<float> onValueChange;

        [Tooltip("Sự kiện gọi ra khi cố mở cửa bị khóa.")]
        public UnityEvent lockedEvent;

        [Tooltip("Sự kiện gọi ra khi mở khóa thành công.")]
        public UnityEvent unlockedEvent;

        // hidden variables
        [Tooltip("Khóa thao tác của người chơi trong lúc đang giữ/kéo vật thể này.")]
        public bool lockPlayer;
        public bool isLocked;
        public bool isInteractLocked;

        public DynamicObjectType CurrentDynamic
        {
            get => dynamicType switch
            {
                DynamicType.Openable => openable,
                DynamicType.Pullable => pullable,
                DynamicType.Switchable => switchable,
                DynamicType.Rotable => rotable,
                _ => null,
            };
        }

        public bool IsOpened => CurrentDynamic.IsOpened;

        public bool IsHolding => CurrentDynamic.IsHolding;

        private void OnValidate()
        {
            openable.DynamicObject = this;
            pullable.DynamicObject = this;
            switchable.DynamicObject = this;
            rotable.DynamicObject = this;
        }

        private void Awake()
        {
            inventory = Inventory.Instance;
            gameManager = GameManager.Instance;

            if (dynamicStatus == DynamicStatus.Locked)
                isLocked = true;

            CurrentDynamic?.OnDynamicInit();
        }

        private void Start()
        {
            if(interactType == InteractType.Mouse)
            {
                Collider collider = GetComponent<Collider>();
                foreach (var col in ignoreColliders)
                {
                    Physics.IgnoreCollision(collider, col);
                }
            }

            if(dynamicType == DynamicType.Pullable && ignorePlayerCollider)
            {
                Collider player = gameManager.PlayerPresence.Player.GetComponent<CharacterController>();
                Collider collider = GetComponent<Collider>();
                Physics.IgnoreCollision(player, collider);
            }

            if (showLockedText)
                lockedText.SubscribeGloc();
        }

        private void Update()
        {
            if (!isLocked) CurrentDynamic?.OnDynamicUpdate();
        }

        public void InteractStartPlayer(GameObject player)
        {
            if (isInteractLocked) return;
            PlayerManager playerManager = player.GetComponent<PlayerManager>();
            CurrentDynamic?.OnDynamicStart(playerManager);
        }

        public void InteractHold(Vector3 point)
        {
            Vector2 delta = InputManager.ReadInput<Vector2>(Controls.POINTER_DELTA);
            if (!isLocked) CurrentDynamic?.OnDynamicHold(delta);
        }

        public void InteractStop()
        {
            if (!isLocked) CurrentDynamic?.OnDynamicEnd();
        }

        /// <summary>
        /// Set dynamic object locked status.
        /// </summary>
        public void SetLockedStatus(bool locked)
        {
            isLocked = locked;
            if (!locked) isInteractLocked = false;
        }

        /// <summary>
        /// Set dynamic object open state.
        /// </summary>
        /// <remarks>
        /// The dynamic object opens as if you were interacting with it. If the dynamic interaction type is mouse, nothing happens.
        /// <br>This function is good for calling from an event.</br>
        /// </remarks>
        public void SetOpenState()
        {
            if (interactType == InteractType.Mouse || isLocked)
                return;

            CurrentDynamic?.OnDynamicOpen();
        }

        /// <summary>
        /// Set dynamic object close state.
        /// </summary>
        /// <remarks>
        /// The dynamic object opens as if you were interacting with it. If the dynamic interaction type is mouse, nothing happens.
        /// <br>This function is good for calling from an event.</br>
        /// </remarks>
        public void SetCloseState()
        {
            if (interactType == InteractType.Mouse || isLocked)
                return;

            CurrentDynamic?.OnDynamicClose();
        }

        /// <summary>
        /// Play Dynamic Object Sound.
        /// </summary>
        /// </param>
        public void PlaySound(DynamicSoundType soundType)
        {
            switch (soundType)
            {
                case DynamicSoundType.Open: GameTools.PlayOneShot3D(target.position, useSound1, "Open Sound"); break;
                case DynamicSoundType.Close: GameTools.PlayOneShot3D(target.position, useSound2, "Close Sound"); break;
                case DynamicSoundType.Locked: GameTools.PlayOneShot3D(target.position, lockedSound, "Locked Sound"); break;
                case DynamicSoundType.Unlock: GameTools.PlayOneShot3D(target.position, unlockSound, "Unlock Sound"); break;
            }
        }

        /// <summary>
        /// The result of using the custom unlock script function.
        /// <br>Call this function after using the OnTryUnlock() function.</br>
        /// </summary>
        public void TryUnlockResult(bool unlocked)
        {
            if (unlocked)
            {
                unlockedEvent?.Invoke();
                PlaySound(DynamicSoundType.Unlock);
            }
            else
            {
                lockedEvent?.Invoke();
                PlaySound(DynamicSoundType.Locked);
            }

            SetLockedStatus(!unlocked);
        }

        private void OnDrawGizmosSelected()
        {
            if(CurrentDynamic.ShowGizmos)
                CurrentDynamic?.OnDrawGizmos();
        }

        public StorableCollection OnSave()
        {
            StorableCollection saveableBuffer = new StorableCollection();

            switch (dynamicType)
            {
                case DynamicType.Openable:
                    saveableBuffer = openable.OnSave();
                    break;
                case DynamicType.Pullable:
                    saveableBuffer = pullable.OnSave();
                    break;
                case DynamicType.Switchable:
                    saveableBuffer = switchable.OnSave();
                    break;
                case DynamicType.Rotable:
                    saveableBuffer = rotable.OnSave();
                    break;
                default:
                    break;
            }

            saveableBuffer.Add("isLocked", isLocked);
            return saveableBuffer;
        }

        public void OnLoad(JToken data)
        {
            switch (dynamicType)
            {
                case DynamicType.Openable:
                    openable.OnLoad(data);
                    break;
                case DynamicType.Pullable:
                    pullable.OnLoad(data);
                    break;
                case DynamicType.Switchable:
                    switchable.OnLoad(data);
                    break;
                case DynamicType.Rotable:
                    rotable.OnLoad(data);
                    break;
            }

            isLocked = (bool)data["isLocked"];
        }
    }
}