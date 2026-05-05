using System;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UHFPS.Input;
using UHFPS.Tools;
using ThunderWire.Attributes;
using TMText = TMPro.TMP_Text;
using static UHFPS.Runtime.SaveGameManager;

namespace UHFPS.Runtime
{
    [Summary("Quản lý quá trình tải màn chơi (Loading Screen), hiển thị thông tin màn chơi và xử lý load dữ liệu dưới nền.")]
    public class LevelManager : MonoBehaviour
    {
        [Serializable]
        public struct LevelInfo
        {
            public string SceneName;
            public GString Title;
            public GString Description;
            public Sprite Background;
        }

        [Tooltip("Danh sách thông tin các màn chơi (Tên scene, tiêu đề, mô tả, hình nền).")]
        public LevelInfo[] LevelInfos;

        [Tooltip("UI Text hiển thị tiêu đề màn chơi.")]
        public TMText Title;

        [Tooltip("UI Text hiển thị mô tả màn chơi.")]
        public TMText Description;

        [Tooltip("UI Image hiển thị hình nền của màn chơi.")]
        public Image Background;

        [Tooltip("Quản lý hiệu ứng tối màn hình (Fade In/Out).")]
        public BackgroundFader FadingBackground;

        /// <summary>
        /// Priority of background loading thread. <br><see href="https://docs.unity3d.com/ScriptReference/Application-backgroundLoadingPriority.html"></see></br>
        /// </summary>
        [Tooltip("Độ ưu tiên của luồng tải nền (Background Loading).")]
        public ThreadPriority LoadPriority = ThreadPriority.High;

        [Tooltip("Tốc độ chuyển cảnh mờ màn hình.")]
        public float FadeSpeed;

        [Tooltip("Yêu cầu bấm phím thủ công để vào game sau khi tải xong thay vì tự động chuyển.")]
        public bool SwitchManually;

        [Tooltip("Làm mờ hình nền khi bắt đầu vào game.")]
        public bool FadeBackground;

        [Tooltip("Bật chế độ debug để ghi log quá trình tải dữ liệu.")]
        public bool Debugging;

        [Tooltip("Cho phép chuyển đổi giữa các Canvas Panel khi tải xong.")]
        public bool SwitchPanels;

        [Tooltip("Tốc độ hiệu ứng mờ khi chuyển đổi Panel.")]
        public float SwitchFadeSpeed;

        [Tooltip("Panel hiện tại (thường là Panel đang hiển thị chữ Loading...).")]
        public CanvasGroup CurrentPanel;

        [Tooltip("Panel sẽ xuất hiện sau khi tải xong (thường là chữ Bấm phím bất kỳ để tiếp tục).")]
        public CanvasGroup NewPanel;

        [Tooltip("Sự kiện gọi liên tục để cập nhật thanh tiến trình tải (giá trị từ 0 đến 1).")]
        public UnityEvent<float> OnProgressUpdate;

        [Tooltip("Sự kiện gọi khi quá trình tải dữ liệu và màn chơi hoàn tất.")]
        public UnityEvent OnLoadingDone;

        private void Start()
        {
            Time.timeScale = 1f;
            Application.backgroundLoadingPriority = LoadPriority;

            string sceneName = LoadSceneName;
            if (!string.IsNullOrEmpty(sceneName))
            {
                foreach (var info in LevelInfos)
                {
                    if(info.SceneName == sceneName)
                    {
                        info.Title.SubscribeGloc();
                        info.Description.SubscribeGloc();

                        Background.sprite = info.Background;
                        Description.text = info.Description;
                        Title.text = info.Title;
                        break;
                    }
                }

                StartCoroutine(LoadLevelAsync(sceneName));
            }
        }

        private IEnumerator LoadLevelAsync(string sceneName)
        {
            yield return FadingBackground.StartBackgroundFade(true, fadeSpeed: FadeSpeed);
            yield return new WaitForEndOfFrame();

            AsyncOperation asyncOp = SceneManager.LoadSceneAsync(sceneName);
            asyncOp.allowSceneActivation = false;

            while (!asyncOp.isDone)
            {
                float progress = asyncOp.progress / 0.9f;
                OnProgressUpdate?.Invoke(progress);

                if (progress >= 1f) break;
                yield return null;
            }

            yield return DeserializeSavedGame();

            if (SwitchManually)
            {
                OnLoadingDone?.Invoke();

                if (SwitchPanels)
                {
                    yield return CanvasGroupFader.StartFade(CurrentPanel, false, SwitchFadeSpeed);
                    yield return CanvasGroupFader.StartFade(NewPanel, true, SwitchFadeSpeed);
                }

                yield return new WaitUntil(() => InputManager.AnyKeyPressed());

                if (FadeBackground)
                {
                    yield return FadingBackground.StartBackgroundFade(false, fadeSpeed: FadeSpeed);
                    yield return new WaitForEndOfFrame();
                }
            }

            asyncOp.allowSceneActivation = true;
            yield return null;
        }

        private IEnumerator DeserializeSavedGame()
        {
            if (GameLoadType == LoadType.Normal || string.IsNullOrEmpty(LoadFolderName))
                yield return null;

            // define loading states
            bool loadGameState = GameLoadType == LoadType.LoadGameState;
            bool loadWorldState = GameLoadType == LoadType.LoadWorldState;
            bool loadLastWorld = GameLoadType == LoadType.LoadWorldLast;

            bool loadWorldType = loadWorldState || loadLastWorld;
            bool canLoadWorld = loadWorldType && SerializationAsset.PreviousScenePersistency;
            string saveFolder = string.Empty;

            if(loadGameState)
            {
                saveFolder = LoadFolderName;
            }
            else if (canLoadWorld)
            {
                if (LastSceneSaves == null)
                {
                    if (Debugging) Debug.Log("[LevelManager] LastSceneSaves are empty. Trying to load the last scene saves.");
                    {
                        Task getLastScenesTask = LoadLastSceneSaves();
                        yield return new WaitToTaskComplete(getLastScenesTask);
                    }
                    if (Debugging) Debug.Log("[LevelManager] The last scene saves was successfully loaded.");
                }

                LastSceneSaves.TryGetValue(LoadSceneName, out saveFolder);
            }

            if (!string.IsNullOrEmpty(saveFolder))
            {
                if (Debugging) Debug.Log($"[LevelManager] Trying to deserialize a save with the name '{saveFolder}'.");
                {
                    Task deserializeTask = TryDeserializeGameStateAsync(saveFolder);
                    yield return new WaitToTaskComplete(deserializeTask);
                }
                if (Debugging) Debug.Log($"[LevelManager] The save was successfully deserialized. ");
            }
        }
    }
}