using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using ThunderWire.Attributes;

namespace UHFPS.Runtime
{
    [InspectorHeader("Main Menu Manager")]
    [Summary("Quản lý logic màn hình Main Menu, bao gồm chuyển cảnh (Load Scene) sang New Game và Quit Game.")]
    public class MainMenuManager : MonoBehaviour
    {
        [Tooltip("Tham chiếu đến Component làm mờ nền (Background Fader).")]
        public BackgroundFader BackgroundFader;

        [Tooltip("Tên Scene sẽ được Load khi người chơi chọn New Game.")]
        public string NewGameSceneName;

        [Tooltip("Có xoá toàn bộ dữ liệu Save cũ khi bắt đầu New Game không?")]
        public bool NewGameRemoveSaves;

        public void NewGame()
        {
            if (string.IsNullOrEmpty(NewGameSceneName))
                throw new System.NullReferenceException("The new game scene name field is empty!");

            SaveGameManager.ClearLoadType();
            StartCoroutine(LoadNewGame());
        }

        IEnumerator LoadNewGame()
        {
            yield return BackgroundFader.StartBackgroundFade(false);
            if(NewGameRemoveSaves) yield return new WaitToTaskComplete(SaveGameManager.RemoveAllSaves());

            SaveGameManager.LoadSceneName = NewGameSceneName;
            SceneManager.LoadScene(SaveGameManager.LMS);
        }

        public void QuitGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}