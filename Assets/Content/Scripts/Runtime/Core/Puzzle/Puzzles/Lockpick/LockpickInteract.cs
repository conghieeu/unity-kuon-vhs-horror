using UnityEngine;
using UnityEngine.Events;
using UHFPS.Tools;
using TMPro;
using ThunderWire.Attributes;

namespace UHFPS.Runtime
{
    [Docs("https://docs.twgamesdev.com/uhfps/guides/puzzles#lockpick-puzzle")]
    [Summary("Thành phần tương tác để bắt đầu giải đố Bẻ khóa (Lockpick). Quản lý cấu hình ổ khóa, số lượng ghim và sự kiện khi bẻ khóa thành công.")]
    public class LockpickInteract : MonoBehaviour, IDynamicUnlock, IInteractStart
    {
        [Tooltip("Prefab mô hình ổ khóa 3D sẽ hiện lên trước Camera khi tương tác.")]
        public GameObject LockpickModel;

        [Tooltip("Góc xoay mục tiêu (-90 đến 90) để mở được khóa.")]
        [Range(-90f, 90f)]
        public float UnlockAngle;

        [Tooltip("Nếu bật, góc mở khóa sẽ được random mỗi lần chơi.")]
        public bool RandomUnlockAngle;

        [Tooltip("Bật nếu component này được gắn kèm với Dynamic Object (Cửa/Ngăn kéo) cần bẻ khóa.")]
        public bool IsDynamicUnlockComponent;

        [Tooltip("Góc nghiêng mặc định của mô hình ổ khóa so với Camera.")]
        public Vector3 LockpickRotation;

        [Tooltip("Khoảng cách từ Camera đến mô hình ổ khóa.")]
        public float LockpickDistance;

        [Tooltip("Định dạng text hiển thị số lượng ghim (Bobby Pin) còn lại.")]
        public GString LockpicksText;

        [Tooltip("Gợi ý các nút điều khiển hiện lên màn hình.")]
        public ControlsContext[] ControlsContexts;

        [Tooltip("Vật phẩm dùng để bẻ khóa (Bobby Pin).")]
        public ItemGuid BobbyPinItem;

        [Tooltip("Giới hạn góc khi random điểm mở khóa.")]
        public MinMax BobbyPinLimits;

        [Tooltip("Dung sai sai lệch (khoảng cách) để được tính là đúng góc bẻ khóa.")]
        public float BobbyPinUnlockDistance = 0.1f;

        [Tooltip("Sức bền của ghim trước khi bị gãy (thời gian xoay căng).")]
        public float BobbyPinLifetime = 2;

        [Tooltip("Ghim sẽ không bao giờ bị gãy.")]
        public bool UnbreakableBobbyPin;

        [Tooltip("Phạm vi góc xoay thử tối đa của ổ khóa khi nhấn quay.")]
        [Range(0f, 90f)]
        public float KeyholeMaxTestRange = 20;

        [Tooltip("Điểm mục tiêu của ổ khóa khi xoay thành công.")]
        public float KeyholeUnlockTarget = 0.1f;

        [Tooltip("Sự kiện gọi ra khi bẻ khóa thành công.")]
        public UnityEvent OnUnlock;

        [HideInInspector] public GameObject LockpickUI;
        [HideInInspector] public TMP_Text LockpickText;

        [HideInInspector] public PlayerPresenceManager PlayerPresence;
        [HideInInspector] public PlayerManager PlayerManager;
        [HideInInspector] public DynamicObject DynamicObject;
        [HideInInspector] public GameManager GameManager;

        private Camera MainCamera => PlayerPresence.PlayerCamera;
        private bool isUnlocked;

        private void Awake()
        {
            PlayerPresence = PlayerPresenceManager.Instance;
            PlayerManager = PlayerPresence.PlayerManager;
            GameManager = GameManager.Instance;
            if (RandomUnlockAngle) UnlockAngle = Mathf.Floor(GameTools.Random(BobbyPinLimits));
        }

        private void Start()
        {
            foreach (var control in ControlsContexts)
            {
                control.SubscribeGloc();
            }

            LockpicksText.SubscribeGloc();

            var references = GameManager.GraphicReferences.Value["Lockpick"];
            LockpickUI = references[0].gameObject;
            LockpickText = (TMP_Text)references[1];
        }

        public void InteractStart()
        {
            if (IsDynamicUnlockComponent || isUnlocked) 
                return;

            AttemptToUnlock();
        }

        public void OnTryUnlock(DynamicObject dynamicObject)
        {
            if (!IsDynamicUnlockComponent || isUnlocked) 
                return;

            DynamicObject = dynamicObject;
            AttemptToUnlock();
        }

        public void AttemptToUnlock()
        {
            Vector3 holdPosition = MainCamera.transform.position + MainCamera.transform.forward * LockpickDistance;
            Quaternion faceRotation = Quaternion.LookRotation(MainCamera.transform.forward) * Quaternion.Euler(LockpickRotation);
            GameObject lockpickObj = Instantiate(LockpickModel, holdPosition, faceRotation, PlayerManager.MainVirtualCamera.transform);
            LockpickComponent lockpickComponent = lockpickObj.GetComponent<LockpickComponent>();

            PlayerManager.PlayerItems.IsItemsUsable = false;
            GameManager.FreezePlayer(true);
            GameManager.SetBlur(true, true);
            GameManager.DisableAllGamePanels();
            GameManager.ShowControlsInfo(true, ControlsContexts);
            lockpickComponent.SetLockpick(this);
        }

        public void Unlock()
        {
            if (isUnlocked) 
                return;

            if (IsDynamicUnlockComponent && DynamicObject != null) 
                DynamicObject.TryUnlockResult(true);
            else OnUnlock?.Invoke();

            isUnlocked = true;
        }
    }
}