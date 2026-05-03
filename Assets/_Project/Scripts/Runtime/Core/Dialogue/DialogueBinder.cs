using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using static UHFPS.Scriptable.DialogueAsset;

namespace UHFPS.Runtime
{
    public class DialogueBinder : MonoBehaviour
    {
        public UnityEvent<AudioSource, string> OnDialogueStart;
        public UnityEvent<AudioClip, string> OnSubtitle;
        public UnityEvent OnSubtitleFinish;
        public UnityEvent OnDialogueEnd;
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