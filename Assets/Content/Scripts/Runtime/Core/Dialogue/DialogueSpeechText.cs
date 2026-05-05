using ThunderWire.Attributes;
using TMPro;
using UnityEngine;

namespace UHFPS.Runtime
{
    [InspectorHeader("Dialogue Speech Text")]
    [Summary("Component quản lý UI Text hiển thị phụ đề hội thoại (Subtitle) ở trên đầu hoặc góc màn hình.")]
    public class DialogueSpeechText : MonoBehaviour
    {
        [Tooltip("Tên Binder để kiểm tra đúng luồng hội thoại.")]
        public string BinderName;

        [Tooltip("UI Text hiển thị phụ đề hội thoại.")]
        public TMP_Text TextMesh;

        [Tooltip("Nếu bật, Text sẽ bị ẩn đi trong khoảng thời gian nghỉ giữa các câu thoại.")]
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