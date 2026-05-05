using System.Collections.Generic;
using UnityEngine;
using UHFPS.Input;
using Unity.Cinemachine;
using UnityEngine.Events;

using ThunderWire.Attributes;

namespace UHFPS.Runtime
{
    [Summary("Lớp cơ sở (Base Class) cho tất cả các hệ thống giải đố. Xử lý chuyển đổi Camera, đóng băng người chơi, bật/tắt collider và hiển thị con trỏ chuột.")]
    public abstract class PuzzleBase : MonoBehaviour, IInteractStart
    {
        [Tooltip("Camera riêng dùng để hiển thị góc nhìn cận cảnh khi giải đố.")]
        public CinemachineCamera PuzzleCamera;

        [Tooltip("Tốc độ hiệu ứng làm mờ màn hình đen (Fade) khi chuyển Camera.")]
        public float SwitchCameraFadeSpeed = 5;

        [Tooltip("Danh sách gợi ý phím bấm hiển thị khi đang giải đố (Click, Xoay, Thoát...).")]
        public ControlsContext[] ControlsContexts;

        [Tooltip("Layer Mask xác định những gì con trỏ chuột có thể va chạm (Raycast) khi giải đố.")]
        public LayerMask CullLayers;

        [Tooltip("Layer dành riêng cho các thành phần giải đố có thể tương tác (Nút bấm, Công tắc...).")]
        public Layer InteractLayer;

        [Tooltip("Layer được gán vào vật thể sau khi giải đố xong để ngăn người chơi tương tác lại.")]
        public Layer DisabledLayer;

        [Tooltip("Hiển thị con trỏ ảo trên màn hình để click vào các bộ phận giải đố.")]
        public bool EnablePointer;

        [Tooltip("Danh sách Collider sẽ được BẬT khi bắt đầu giải đố (Dùng cho các nút bấm nhỏ).")]
        public List<Collider> CollidersEnable = new List<Collider>();

        [Tooltip("Danh sách Collider sẽ bị TẮT khi bắt đầu giải đố (Thường là Collider tương tác chính của vật thể).")]
        public List<Collider> CollidersDisable = new List<Collider>();

        [Tooltip("Sự kiện gọi ra khi màn hình bắt đầu mờ đi để chuyển cảnh (True = Vào giải đố, False = Thoát ra).")]
        public UnityEvent<bool> OnScreenFade;

        protected PlayerPresenceManager playerPresence;
        protected PlayerManager playerManager;
        protected GameManager gameManager;
        private bool canSwitch;

        /// <summary>
        /// Specifies when the camera is switched to a puzzle or normal camera. [true = puzzle, false = normal]
        /// </summary>
        protected bool isActive;

        /// <summary>
        /// Specifies when the camera can be switched back to normal camera using the default functionality.
        /// </summary>
        protected bool canManuallySwitch;

        /// <summary>
        /// Determines whether the colliders switch to puzzle mode or normal mode.
        /// </summary>
        protected bool switchColliders;

        public virtual void Awake()
        {
            playerPresence = PlayerPresenceManager.Instance;
            playerManager = playerPresence.PlayerManager;
            gameManager = GameManager.Instance;

            foreach (var control in ControlsContexts)
            {
                control.SubscribeGloc();
            }
        }

        public virtual void Update()
        {
            if (!canSwitch || !isActive || !canManuallySwitch)
                return;

            if (InputManager.ReadButtonOnce(GetInstanceID(), Controls.EXAMINE))
                SwitchBack();
        }

        /// <summary>
        /// This function is called when you interact with the object. It freezes the player and switches the camera to the puzzle camera.
        /// </summary>
        public virtual void InteractStart()
        {
            if (!isActive)
            {
                playerPresence.FreezePlayer(true);
                playerManager.PlayerItems.IsItemsUsable = false;
                playerPresence.SwitchActiveCamera(PuzzleCamera.gameObject, SwitchCameraFadeSpeed, OnBackgroundFade, () => { canSwitch = true; });
                canManuallySwitch = true;
                switchColliders = true;
                isActive = true;
            }
        }

        /// <summary>
        /// This function is called before switching to the puzzle camera, after the screen fades to black.
        /// </summary>
        public virtual void OnBackgroundFade()
        {
            if (isActive)
            {
                gameManager.DisableAllGamePanels();
                gameManager.ShowControlsInfo(true, ControlsContexts);

                if (EnablePointer)
                {
                    gameManager.ShowPointer(CullLayers, InteractLayer, (hit, interactStart) =>
                    {
                        interactStart.InteractStart();
                    });
                }

                if (switchColliders)
                {
                    CollidersEnable.ForEach(x => x.enabled = true);
                    CollidersDisable.ForEach(x => x.enabled = false);
                }
            }
            else
            {
                playerPresence.FreezePlayer(false);
                playerManager.PlayerItems.IsItemsUsable = true;
                gameManager.ShowControlsInfo(false, null);
                gameManager.ShowPanel(GameManager.PanelType.MainPanel);

                if (switchColliders)
                {
                    CollidersEnable.ForEach(x => x.enabled = false);
                    CollidersDisable.ForEach(x => x.enabled = true);
                }
            }

            OnScreenFade?.Invoke(isActive);
        }

        /// <summary>
        /// Calling this function switches the puzzle camera to the normal camera.
        /// </summary>
        protected void SwitchBack()
        {
            if (isActive)
            {
                playerPresence.SwitchToPlayerCamera(SwitchCameraFadeSpeed, OnBackgroundFade);
                if (EnablePointer) gameManager.HidePointer();
                canSwitch = false;
                isActive = false;
            }
        }

        /// <summary>
        /// Disable the puzzle interaction functionality. The GameObject layer will be set to Disabled Layer.
        /// </summary>
        protected void DisableInteract(bool includeChild = true)
        {
            gameObject.layer = DisabledLayer;

            if (includeChild)
            {
                foreach (Transform tr in transform)
                {
                    tr.gameObject.layer = DisabledLayer;
                }
            }
        }
    }
}