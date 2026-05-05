using UnityEngine.Events;
using UnityEngine;
using TMPro;
using Newtonsoft.Json.Linq;
using UHFPS.Tools;

using ThunderWire.Attributes;

namespace UHFPS.Runtime
{
    [Summary("Hệ thống giải đố Két sắt xoay (Safe). Tạo ra bánh xe quay trên màn hình để người chơi vặn trái/phải nhập mã.")]
    public class SafePuzzle : MonoBehaviour, IInteractStart, ISaveable
    {
        [Tooltip("Mã số mở khóa, gồm các cặp 2 chữ số nối tiếp nhau (VD: 153045).")]
        public string UnlockCode = "000000";

        [Tooltip("Prefab của vòng xoay (Dial) sẽ được tạo ra trên màn hình khi giải đố.")]
        public GameObject WheelObject;

        [Tooltip("Góc xoay của bánh xe khi hiển thị trước Camera.")]
        public Vector3 WheelRotation;

        [Tooltip("Khoảng cách từ Camera tới bánh xe.")]
        public float WheelDistance;

        [Tooltip("Độ sáng của đèn chiếu (Examine Light) hắt vào bánh xe.")]
        public float FocusLightIntensity = 1f;

        [Tooltip("Animator điều khiển việc mở cửa két sắt.")]
        public Animator Animator;

        [Tooltip("Trigger kích hoạt hoạt ảnh mở cửa (Unlock).")]
        public string UnlockTrigger = "Unlock";

        [Tooltip("Trigger thiết lập trạng thái két sắt đã mở (Dùng khi Load game).")]
        public string ResetTrigger = "Reset";

        [Tooltip("Màu chữ mặc định của các cặp số chưa nhập/đã nhập xong.")]
        public Color SolutionNormalColor = Color.black;

        [Tooltip("Màu chữ của cặp số mà người chơi ĐANG xoay để nhập.")]
        public Color SolutionCurrentColor = Color.white;

        [Tooltip("Layer của két sắt sau khi đã mở khóa (Để tránh tương tác lại).")]
        public Layer UnlockedLayer;

        [Tooltip("Danh sách gợi ý phím hiển thị khi giải đố (Xoay trái, Xoay phải, Thoát).")]
        public ControlsContext[] ControlsContexts;

        [Tooltip("Gọi lại sự kiện OnUnlock khi Load Game nếu két sắt đã từng được mở.")]
        public bool LoadCallEvent;

        [Tooltip("Sự kiện gọi ra khi mở két thành công (Mở cửa, rơi đồ...).")]
        public UnityEvent OnUnlock;

        public GameManager GameManager;
        public PlayerManager PlayerManager;
        private PlayerPresenceManager PlayerPresence;
        private ExamineController ExamineController;

        public GameObject SafeLockPanel;
        public TMP_Text Number1;
        public TMP_Text Number2;
        public TMP_Text Number3;

        private Camera MainCamera => PlayerPresence.PlayerCamera;
        private bool isUnlocked;

        private void Reset()
        {
            UnlockCode = "000000";
        }

        private void Awake()
        {
            GameManager = GameManager.Instance;
            PlayerPresence = PlayerPresenceManager.Instance;
            PlayerManager = PlayerPresence.PlayerManager;
            ExamineController = PlayerManager.GetComponentInChildren<ExamineController>();

            foreach (var control in ControlsContexts)
            {
                control.SubscribeGloc();
            }
        }

        private void Start()
        {
            var references = GameManager.GraphicReferences.Value["SafeLock"];
            SafeLockPanel = references[0].gameObject;
            Number1 = (TMP_Text)references[1];
            Number2 = (TMP_Text)references[2];
            Number3 = (TMP_Text)references[3];
        }

        public void InteractStart()
        {
            Vector3 holdPosition = MainCamera.transform.position + MainCamera.transform.forward * WheelDistance;
            Quaternion faceRotation = Quaternion.LookRotation(MainCamera.transform.forward) * Quaternion.Euler(WheelRotation);

            GameObject wheelObj = Instantiate(WheelObject, holdPosition, faceRotation, PlayerManager.MainVirtualCamera.transform);
            SafeWheel safeWheel = wheelObj.GetComponent<SafeWheel>();
            ExamineController.SetExamineLight(FocusLightIntensity);

            PlayerManager.PlayerItems.IsItemsUsable = false;
            GameManager.FreezePlayer(true);
            GameManager.SetBlur(true, true);
            GameManager.DisableAllGamePanels();
            GameManager.ShowControlsInfo(true, ControlsContexts);
            safeWheel.SetSafe(this);
        }

        public void OnPuzzleQuit()
        {
            SafeLockPanel.SetActive(false);
            ExamineController.ResetExamineLight();
        }

        public void SetUnlocked()
        {
            gameObject.layer = UnlockedLayer;
            OnUnlock?.Invoke();

            if (Animator != null)
                Animator.SetTrigger(UnlockTrigger);
        }

        public StorableCollection OnSave()
        {
            return new StorableCollection()
            {
                { nameof(isUnlocked), isUnlocked }
            };
        }

        public void OnLoad(JToken data)
        {
            isUnlocked = (bool)data[nameof(isUnlocked)];

            if (isUnlocked)
            {
                gameObject.layer = UnlockedLayer;
                if (Animator != null) Animator.SetTrigger(ResetTrigger);
                if (LoadCallEvent) OnUnlock?.Invoke();
            }
        }
    }
}