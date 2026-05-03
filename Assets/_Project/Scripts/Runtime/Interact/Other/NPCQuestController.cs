using System;
using System.Reactive.Linq;
using UnityEngine;
using UHFPS.Scriptable;
using UHFPS.Runtime;
using Newtonsoft.Json.Linq;

namespace UHFPS.Runtime
{
    [RequireComponent(typeof(SaveableObject))]
    public class NPCQuestController : MonoBehaviour, IInteractStart, ISaveable
    {
        public enum NPCState { Initial, WaitingForItem, Completed }

        [Header("Dialogue Triggers")]
        [Tooltip("The DialogueTrigger for the first encounter (Quest Giving). Make sure its TriggerType is set to 'Event'.")]
        public DialogueTrigger initialDialogueTrigger;
        
        [Tooltip("The DialogueTrigger for when the NPC is waiting for the player to bring the item. Make sure its TriggerType is set to 'Event'.")]
        public DialogueTrigger waitingDialogueTrigger;
        
        [Tooltip("The DialogueTrigger for when the quest is completed. Make sure its TriggerType is set to 'Event'.")]
        public DialogueTrigger completedDialogueTrigger;

        [Header("Quest Settings")]
        public string requiredItemGUID;
        public ushort requiredItemQuantity = 1;
        public ObjectiveSelect objectiveToComplete;

        private NPCState currentState = NPCState.Initial;
        private bool isMyDialoguePlaying = false;
        private IDisposable dialogueEndSub;

        private void Start()
        {
            // Subscribe to the global dialogue end event
            if (DialogueSystem.Instance != null)
            {
                dialogueEndSub = DialogueSystem.Instance.OnDialogueEnd.Subscribe(_ => OnDialogueEnded());
            }
        }

        private void OnDestroy()
        {
            // Clean up subscription to prevent memory leaks
            dialogueEndSub?.Dispose();
        }

        public void InteractStart()
        {
            // Do not interact if a dialogue is already playing
            if (DialogueSystem.Instance.IsPlaying)
                return;

            // Pre-interaction logic based on state
            if (currentState == NPCState.WaitingForItem)
            {
                CheckQuestCompletion();
            }

            // Play the appropriate dialogue based on current state
            PlayCurrentStateDialogue();
        }

        private void CheckQuestCompletion()
        {
            if (Inventory.Instance != null && Inventory.Instance.ContainsItem(requiredItemGUID, out _))
            {
                // 1. Remove the item from inventory
                Inventory.Instance.RemoveItem(requiredItemGUID, requiredItemQuantity);

                // 2. Complete the objective in the system
                if (ObjectiveManager.Instance != null && !string.IsNullOrEmpty(objectiveToComplete.ObjectiveKey))
                {
                    ObjectiveManager.Instance.CompleteObjective(objectiveToComplete.ObjectiveKey, objectiveToComplete.SubObjectives);
                }

                // 3. Move to completed state
                currentState = NPCState.Completed;
            }
        }

        private void PlayCurrentStateDialogue()
        {
            DialogueTrigger triggerToPlay = null;

            switch (currentState)
            {
                case NPCState.Initial:
                    triggerToPlay = initialDialogueTrigger;
                    break;
                case NPCState.WaitingForItem:
                    triggerToPlay = waitingDialogueTrigger;
                    break;
                case NPCState.Completed:
                    triggerToPlay = completedDialogueTrigger;
                    break;
            }

            if (triggerToPlay != null)
            {
                // Bypass TriggerDialogue() checking 'isTriggered' by directly calling DialogueSystem
                bool success = DialogueSystem.Instance.PlayDialogue(triggerToPlay);
                if (success)
                {
                    isMyDialoguePlaying = true;
                }
            }
            else
            {
                Debug.LogWarning($"[NPCQuestController] Missing DialogueTrigger for state: {currentState}");
            }
        }

        private void OnDialogueEnded()
        {
            // Check if it was our dialogue that just ended
            if (isMyDialoguePlaying)
            {
                isMyDialoguePlaying = false;

                // State transitions after dialogue finishes
                if (currentState == NPCState.Initial)
                {
                    // After the initial greeting/quest giving dialogue, the NPC waits for the item
                    currentState = NPCState.WaitingForItem;
                }
            }
        }

        #region ISaveable Implementation

        public StorableCollection OnSave()
        {
            return new StorableCollection()
            {
                { "npcQuestState", (int)currentState }
            };
        }

        public void OnLoad(JToken data)
        {
            if (data["npcQuestState"] != null)
            {
                currentState = (NPCState)(int)data["npcQuestState"];
            }
        }

        #endregion
    }
}
