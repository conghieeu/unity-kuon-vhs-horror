using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UHFPS.Runtime;

namespace Custom.DialogueSystem
{
    [Serializable]
    public class BranchingDialogueOption
    {
        public string OptionText;
        [Tooltip("Index of the next node. -1 to end dialogue.")]
        public int NextNodeIndex = -1;
        public UnityEvent OnOptionSelected;
    }

    [Serializable]
    public class BranchingDialogueNode
    {
        public string CharacterName;
        [TextArea(3, 5)]
        public string DialogueText;
        public AudioClip VoiceLine;
        public List<BranchingDialogueOption> Options = new List<BranchingDialogueOption>();
    }

    public class BranchingDialogueInteract : MonoBehaviour, IInteractStart, IInteractTitle
    {
        public string InteractTitleName = "Talk";
        
        [Header("Dialogue Data")]
        public List<BranchingDialogueNode> DialogueNodes = new List<BranchingDialogueNode>();
        
        [Header("State")]
        public int StartNodeIndex = 0;

        public TitleParams InteractTitle()
        {
            return new TitleParams { title = InteractTitleName, button1 = "Interact" };
        }

        public void InteractStart()
        {
            if (DialogueNodes.Count > 0 && StartNodeIndex >= 0 && StartNodeIndex < DialogueNodes.Count)
            {
                if (BranchingDialogueManager.HasReference)
                {
                    BranchingDialogueManager.Instance.StartDialogue(this, StartNodeIndex);
                }
                else
                {
                    Debug.LogError("BranchingDialogueManager is missing in the scene!");
                }
            }
        }

        public void ChangeStartNodeIndex(int newIndex)
        {
            StartNodeIndex = newIndex;
        }
    }
}
