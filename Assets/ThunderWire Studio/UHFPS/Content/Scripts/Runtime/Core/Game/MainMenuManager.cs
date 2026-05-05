using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using ThunderWire.Attributes;

namespace UHFPS.Runtime
{
    [InspectorHeader("Main Menu Manager")]
    public class MainMenuManager : MonoBehaviour
    {
        public BackgroundFader BackgroundFader;
        public string NewGameSceneName;
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