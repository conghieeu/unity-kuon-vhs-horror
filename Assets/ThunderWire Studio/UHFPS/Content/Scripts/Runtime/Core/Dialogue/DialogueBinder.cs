using UnityEngine;
using UnityEngine.Events;

namespace UHFPS.Runtime
{
    public class DialogueBinder : MonoBehaviour
    {
        public UnityEvent<AudioSource, string> OnDialogueStart;
        public UnityEvent<AudioClip, string> OnSubtitle;
        public UnityEvent OnSubtitleFinish;
        public UnityEvent OnDialogueEnd;

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