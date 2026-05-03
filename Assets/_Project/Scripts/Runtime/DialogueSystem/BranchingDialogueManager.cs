using UnityEngine;
using UHFPS.Runtime;

namespace Custom.DialogueSystem
{
    public class BranchingDialogueManager : Singleton<BranchingDialogueManager>
    {
        [Header("References")]
        public BranchingDialogueUI DialogueUI;

        private BranchingDialogueInteract currentInteract;
        private AudioSource audioSource;
        
        public bool IsDialogueActive { get; private set; }

        private void Awake()
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        public void StartDialogue(BranchingDialogueInteract interact, int startIndex)
        {
            if (IsDialogueActive) return;

            currentInteract = interact;
            IsDialogueActive = true;
            
            // Lock player and hide UI
            if (GameManager.HasReference)
            {
                GameManager.Instance.DisableAllGamePanels();
                GameManager.Instance.FreezePlayer(true, true);
            }

            DialogueUI.ShowDialoguePanel(true);
            ShowNode(startIndex);
        }

        public void ShowNode(int nodeIndex)
        {
            if (nodeIndex < 0 || nodeIndex >= currentInteract.DialogueNodes.Count)
            {
                EndDialogue();
                return;
            }

            var node = currentInteract.DialogueNodes[nodeIndex];

            if (node.VoiceLine != null)
            {
                audioSource.clip = node.VoiceLine;
                audioSource.Play();
            }
            else
            {
                audioSource.Stop();
            }

            DialogueUI.DisplayNode(node);
        }

        public void OptionSelected(int nextNodeIndex, UnityEngine.Events.UnityEvent onSelectEvent)
        {
            onSelectEvent?.Invoke();

            if (nextNodeIndex < 0)
            {
                EndDialogue();
            }
            else
            {
                ShowNode(nextNodeIndex);
            }
        }

        public void EndDialogue()
        {
            IsDialogueActive = false;
            audioSource.Stop();
            DialogueUI.ShowDialoguePanel(false);

            if (GameManager.HasReference)
            {
                GameManager.Instance.ShowPanel(GameManager.PanelType.MainPanel);
                GameManager.Instance.FreezePlayer(false, false);
            }
        }
    }
}
