using ThunderWire.Attributes;
using TMPro;
using UnityEngine;

namespace UHFPS.Runtime
{
    [InspectorHeader("Dialogue Speech Text")]
    public class DialogueSpeechText : MonoBehaviour
    {
        public string BinderName;
        public TMP_Text TextMesh;
        public bool HideBetweenSubtitles;

        private bool isPlaying;

        public void OnDialogueStart(AudioSource _, string binderName)
        {
            if(isPlaying = binderName == BinderName)
                TextMesh.gameObject.SetActive(true);
        }

        public void OnSubtitle(AudioClip _, string subtitleText)
        {
            if (!isPlaying)
                return;

            TextMesh.text = subtitleText;
        }

        public void OnSubtitleFinish()
        {
            if (!isPlaying || !HideBetweenSubtitles)
                return;

            TextMesh.text = "";
        }

        public void OnDialogueEnd()
        {
            if (!isPlaying)
                return;

            TextMesh.gameObject.SetActive(false);
            TextMesh.text = "";
            isPlaying = false;
        }
    }
}