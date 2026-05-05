using System;
using System.Reactive.Disposables;
using UnityEngine;
using UnityEngine.Events;
using ThunderWire.Attributes;
using UHFPS.Tools;

namespace UHFPS.Runtime
{
    [InspectorHeader("Dialogue Events")]
    [Summary("Component cung cấp các UnityEvent cơ bản khi hội thoại Bắt đầu và Kết thúc để gọi các hàm bên ngoài (Ví dụ: Ẩn/Hiện Player UI).")]
    public class DialogueEvents : MonoBehaviour
    {
        [Tooltip("Sự kiện gọi khi bất kỳ hội thoại nào bắt đầu.")]
        public UnityEvent OnDialogueStart;

        [Tooltip("Sự kiện gọi khi hội thoại kết thúc.")]
        public UnityEvent OnDialogueEnd;

        private readonly CompositeDisposable disposables = new();
        private DialogueSystem dialogueSystem;

        private void Awake()
        {
            dialogueSystem = DialogueSystem.Instance;
        }

        private void OnEnable()
        {
            dialogueSystem.OnDialogueStart.Subscribe(_ => OnDialogueStart?.Invoke()).AddTo(disposables);
            dialogueSystem.OnDialogueEnd.Subscribe(_ => OnDialogueEnd?.Invoke()).AddTo(disposables);
        }

        private void OnDisable()
        {
            disposables.Dispose();
        }
    }
}