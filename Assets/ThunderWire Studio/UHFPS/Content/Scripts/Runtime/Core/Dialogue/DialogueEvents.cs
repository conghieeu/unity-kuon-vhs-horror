using System;
using System.Reactive.Disposables;
using UnityEngine;
using UnityEngine.Events;
using ThunderWire.Attributes;
using UHFPS.Tools;

namespace UHFPS.Runtime
{
    [InspectorHeader("Dialogue Events")]
    public class DialogueEvents : MonoBehaviour
    {
        public UnityEvent OnDialogueStart;
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