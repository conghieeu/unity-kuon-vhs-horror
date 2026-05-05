using System;
using System.Reactive.Linq;
using UnityEngine;
using UHFPS.Scriptable;
using UHFPS.Runtime;
using Newtonsoft.Json.Linq;
using ThunderWire.Attributes;

namespace UHFPS.Runtime
{
    [RequireComponent(typeof(SaveableObject))]
    [Summary("Bộ điều khiển NPC giao tiếp với người chơi và yêu cầu vật phẩm để hoàn thành nhiệm vụ.")]
    public class NPCQuestController : MonoBehaviour, IInteractStart, ISaveable
    {
        public enum NPCState { Initial, WaitingForItem, Completed }

        [Header("Dialogue Triggers")]
        [Tooltip("Trigger hội thoại cho lần gặp đầu tiên (Giao nhiệm vụ). Hãy chắc chắn 'TriggerType' được đặt là 'Event'.")]
        public DialogueTrigger initialDialogueTrigger;
        
        [Tooltip("Trigger hội thoại khi NPC đang đợi người chơi mang vật phẩm về. Hãy chắc chắn 'TriggerType' được đặt là 'Event'.")]
        public DialogueTrigger waitingDialogueTrigger;
        
        [Tooltip("Trigger hội thoại khi nhiệm vụ đã hoàn thành. Hãy chắc chắn 'TriggerType' được đặt là 'Event'.")]
        public DialogueTrigger completedDialogueTrigger;

        [Header("Quest Settings")]
        [Tooltip("Mã GUID của vật phẩm NPC yêu cầu.")]
        public string requiredItemGUID;
        [Tooltip("Số lượng vật phẩm NPC yêu cầu.")]
        public ushort requiredItemQuantity = 1;
        [Tooltip("Nhiệm vụ (Objective) sẽ được hệ thống đánh dấu hoàn thành sau khi giao đủ vật phẩm.")]
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
            // If a dialogue is currently playing, treat interaction as advancing the dialogue (for Event mode)
            if (DialogueSystem.Instance.IsPlaying)
            {
                DialogueSystem.Instance.NextDialogue();
                return;
            }

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
