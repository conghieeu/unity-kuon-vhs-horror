using System;
using System.Collections;
using UnityEngine;
using Unity.Cinemachine;
using UHFPS.Tools;
using ThunderWire.Attributes;

namespace UHFPS.Runtime
{
    [Docs("https://docs.twgamesdev.com/uhfps/getting-started")]
    [Summary("Quản lý sự hiện diện, trạng thái khóa/mở khóa và các thao tác chuyển đổi camera, dịch chuyển của người chơi trong scene.")]
    public class PlayerPresenceManager : Singleton<PlayerPresenceManager>
    {
        public enum UnlockType 
        {
            /// <summary>
            /// Player will be unlocked at the start or after the game state is loaded.
            /// </summary>
            Automatically,

            /// <summary>
            /// Player will be unlocked after calling the <b>UnlockPlayer()</b> function.
            /// </summary>
            Manually
        }

        [Tooltip("Cách thức mở khóa người chơi khi bắt đầu scene (Tự động hoặc Thủ công).")]
        public UnlockType PlayerUnlockType = UnlockType.Automatically;

        [Tooltip("GameObject chính của người chơi.")]
        public GameObject Player;

        [Tooltip("Thời gian chờ trước khi bắt đầu hiệu ứng tối màn hình khi mở khóa.")]
        public float WaitFadeOutTime = 0.5f;

        [Tooltip("Tốc độ hiệu ứng tối/sáng màn hình (Fade Out/In).")]
        public float FadeOutSpeed = 3f;

        private PlayerComponent[] playerComponents;
        private GameManager gameManager;
        private GameObject activeCamera;

        public bool PlayerIsUnlocked;
        public bool GameStateIsLoaded;
        public bool IsCameraSwitched;

        private bool isBackgroundFadedOut;

        private PlayerManager playerManager;
        public PlayerManager PlayerManager
        {
            get
            {
                if (playerManager == null)
                    playerManager = Player.GetComponent<PlayerManager>();

                return playerManager;
            }
        }

        public PlayerStateMachine StateMachine => PlayerManager.PlayerStateMachine;
        public LookController LookController => PlayerManager.LookController;

        /// <summary>
        /// Check if player is unlocked and the active camera is player camera.
        /// </summary>
        public bool IsUnlockedAndCamera => PlayerIsUnlocked && !IsCameraSwitched;

        public Camera PlayerCamera => PlayerManager.MainCamera;

        public CinemachineCamera PlayerVirtualCamera => PlayerManager.MainVirtualCamera;

        private void OnEnable()
        {
            SaveGameManager.Instance.OnGameLoaded += () =>
            {
                UnlockPlayer();
                GameStateIsLoaded = true;
            };
        }

        private void Awake()
        {
            gameManager = GetComponent<GameManager>();
            playerComponents = Player.GetComponentsInChildren<PlayerComponent>(true);

            // keep player frozen at start
            FreezePlayer(true);
        }

        private void Start()
        {
            if (!SaveGameManager.GameWillLoad || !SaveGameManager.GameStateExist)
            {
                if(PlayerUnlockType == UnlockType.Automatically)
                    UnlockPlayer();
            }
        }

        public T Component<T>()
        {
            return Player.GetComponentInChildren<T>(true);
        }

        public T[] Components<T>()
        {
            return Player.GetComponentsInChildren<T>(true);
        }

        public void FreezeMovement(bool freeze)
        {
            StateMachine.SetEnabled(!freeze);
        }

        public void FreezeLook(bool freeze, bool showCursor = false)
        {
            GameTools.ShowCursor(!showCursor, showCursor);
            LookController.SetEnabled(!freeze);
        }

        public void FreezePlayer(bool freeze, bool showCursor = false)
        {
            GameTools.ShowCursor(!showCursor, showCursor);

            foreach (var component in playerComponents)
            {
                component.SetEnabled(!freeze);
            }
        }

        public void FadeBackground(bool fadeOut, Action onBackgroundFade)
        {
            StartCoroutine(StartFadeBackground(fadeOut, onBackgroundFade));
        }

        IEnumerator StartFadeBackground(bool fadeOut, Action onBackgroundFade)
        {
            yield return gameManager.StartBackgroundFade(fadeOut, WaitFadeOutTime, FadeOutSpeed);
            isBackgroundFadedOut = fadeOut;
            onBackgroundFade?.Invoke();
        }

        public void UnlockPlayer()
        {
            StartCoroutine(DoUnlockPlayer());
        }

        private IEnumerator DoUnlockPlayer()
        {
            if(!isBackgroundFadedOut)
                yield return gameManager.StartBackgroundFade(true, WaitFadeOutTime, FadeOutSpeed);

            FreezePlayer(false);
            PlayerIsUnlocked = true;
        }

        public (Vector3 position, Vector2 rotation) GetPlayerTransform()
        {
            return (Player.transform.position, LookController.LookRotation);
        }

        public void SetPlayerTransform(Vector3 position, Vector2 rotation)
        {
            Player.transform.SetPositionAndRotation(position, Quaternion.identity);
            LookController.LookRotation = rotation;
            Physics.SyncTransforms(); // sync position to character controller
        }

        public void SetPlayerPositionAndLook(Vector3 position, Vector2 eulerLook)
        {
            Player.transform.SetPositionAndRotation(position, Quaternion.identity);
            LookController.ApplyEulerLook(eulerLook);
            Physics.SyncTransforms(); // sync position to character controller
        }

        public void Teleport(Vector3 position)
        {
            Player.transform.SetPositionAndRotation(position, Quaternion.identity);
            Physics.SyncTransforms();
        }

        public void Teleport(Vector3 position, bool colliderState = true)
        {
            Player.transform.position = position;
            StateMachine.PlayerCollider.enabled = colliderState;

            // sync position to character controller
            if (colliderState) Physics.SyncTransforms();
        }

        public void Teleport(Vector3 position, Vector2 eulerLook, bool colliderState = true)
        {
            Player.transform.SetPositionAndRotation(position, Quaternion.identity);
            StateMachine.PlayerCollider.enabled = colliderState;
            LookController.ApplyEulerLook(eulerLook);

            // sync position to character controller
            if (colliderState) Physics.SyncTransforms();
        }

        public void SwitchActiveCamera(GameObject virtualCameraObj, float fadeSpeed, Action onBackgroundFade)
        {
            StartCoroutine(SwitchCamera(virtualCameraObj, fadeSpeed, onBackgroundFade, null));
            IsCameraSwitched = true;
        }

        public void SwitchActiveCamera(GameObject virtualCameraObj, float fadeSpeed, Action onBackgroundFade, Action onFadeComplete)
        {
            StartCoroutine(SwitchCamera(virtualCameraObj, fadeSpeed, onBackgroundFade, onFadeComplete));
            IsCameraSwitched = true;
        }

        public void SwitchToPlayerCamera(float fadeSpeed, Action onBackgroundFade)
        {
            StartCoroutine(SwitchCamera(null, fadeSpeed, onBackgroundFade, null));
        }

        public void SwitchToPlayerCamera(float fadeSpeed, Action onBackgroundFade, Action onFadeComplete)
        {
            StartCoroutine(SwitchCamera(null, fadeSpeed, onBackgroundFade, onFadeComplete));
        }

        public IEnumerator SwitchCamera(GameObject cameraObj, float fadeSpeed)
        {
            yield return gameManager.StartBackgroundFade(false, fadeSpeed: fadeSpeed);
            playerManager.MainVirtualCamera.gameObject.SetActive(cameraObj == null);
            
            if(cameraObj != null) playerManager.PlayerItems.DeactivateCurrentItem();
            else playerManager.PlayerItems.ActivatePreviouslyDeactivatedItem();

            if (activeCamera != null) activeCamera.SetActive(false);
            if (cameraObj != null) cameraObj.SetActive(cameraObj != null);
            activeCamera = cameraObj;

            yield return new WaitForEndOfFrame();
            yield return gameManager.StartBackgroundFade(true, fadeSpeed: fadeSpeed);

            IsCameraSwitched = cameraObj != null; // check if camera switched to player camera
        }

        private IEnumerator SwitchCamera(GameObject cameraObj, float fadeSpeed, Action onBackgroundFade, Action onFadeComplete)
        {
            yield return gameManager.StartBackgroundFade(false, fadeSpeed: fadeSpeed);
            playerManager.MainVirtualCamera.gameObject.SetActive(cameraObj == null);

            if (cameraObj != null) playerManager.PlayerItems.DeactivateCurrentItem();
            else playerManager.PlayerItems.ActivatePreviouslyDeactivatedItem();

            if (activeCamera != null) activeCamera.SetActive(false);
            if(cameraObj != null) cameraObj.SetActive(cameraObj != null);
            activeCamera = cameraObj;

            onBackgroundFade?.Invoke();

            yield return new WaitForEndOfFrame();
            yield return gameManager.StartBackgroundFade(true, fadeSpeed: fadeSpeed);

            IsCameraSwitched = cameraObj != null; // check if camera switched to player camera

            yield return new WaitForSeconds(0.1f);
            onFadeComplete?.Invoke();
        }
    }
}