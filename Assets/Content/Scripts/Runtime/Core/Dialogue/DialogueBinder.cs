using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using static UHFPS.Scriptable.DialogueAsset;

using ThunderWire.Attributes;

namespace UHFPS.Runtime
{
    [Summary("Component trung gian dùng để kết nối và chuyển tiếp các sự kiện từ DialogueSystem tới các UI hoặc script khác.")]
    public class DialogueBinder : MonoBehaviour
    {
        [Tooltip("Sự kiện gọi ra khi hội thoại bắt đầu (Truyền ra AudioSource và tên Binder).")]
        public UnityEvent<AudioSource, string> OnDialogueStart;
        [Tooltip("Sự kiện gọi ra mỗi khi chuyển sang câu phụ đề mới (Truyền ra âm thanh và Text).")]
        public UnityEvent<AudioClip, string> OnSubtitle;

        [Tooltip("Sự kiện gọi ra khi một câu phụ đề kết thúc chờ.")]
        public UnityEvent OnSubtitleFinish;

        [Tooltip("Sự kiện gọi ra khi toàn bộ sequence hội thoại kết thúc.")]
        public UnityEvent OnDialogueEnd;

        [Tooltip("Sự kiện gọi ra khi hiển thị các lựa chọn (Truyền ra danh sách DialogueOption).")]
        public UnityEvent<List<DialogueOption>> OnShowOptions;

        private DialogueSystem dialogueSystem;

        private void Awake()
        {
            dialogueSystem = DialogueSystem.Instance;
        }

        public void NextDialogue()
        {
            dialogueSystem.NextDialogue();
        }

        public void StopDialogue()
        {
            dialogueSystem.StopDialogue();
        }
    }
}