using UnityEngine;
using UHFPS.Scriptable;
using Newtonsoft.Json.Linq;
using static UHFPS.Scriptable.DialogueAsset;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace UHFPS.Runtime
{
    public class DialogueTrigger : MonoBehaviour, IInteractStart, ISaveable
    {
        public enum TriggerTypeEnum { Trigger, Interact, Event }
        public enum DialogueTypeEnum { Local, Global }
        public enum DialogueContinueEnum { Sequence, Event }

        public DialogueSystem.DialogueData DialogueData { get; private set; }
        public bool IsCompleted { get; set; }

        [Tooltip("Loại kích hoạt: Trigger (Va chạm), Interact (Tương tác phím E), Event (Kích hoạt bằng code).")]
        public TriggerTypeEnum TriggerType;
        [Tooltip("Loại hội thoại: Local (Âm thanh 3D, giới hạn khoảng cách), Global (Âm thanh 2D, không giới hạn khoảng cách).")]
        public DialogueTypeEnum DialogueType;
        [Tooltip("Cách chuyển câu thoại: Sequence (Tự động chuyển), Event (Nhấn phím để chuyển).")]
        public DialogueContinueEnum DialogueContinue;
        
        public DialogueAsset Dialogue;
        public AudioSource DialogueAudio;
        public string BinderName;

        [Tooltip("Cho phép kích hoạt lại hội thoại nhiều lần.")]
        public bool Repeatable;
        public bool RangedDialogue;
        public bool ResetDialogueWhenOut;
        public float LocalDialogueRange;

        private DialogueSystem dialogueSystem;
        private Transform playerTransform;
        private bool isTriggered;

        private void Awake()
        {
            dialogueSystem = DialogueSystem.Instance;
            playerTransform = PlayerPresenceManager.Instance.Player.transform;
        }

        private void Start()
        {
            DialogueData = new();
            foreach (var dialogue in Dialogue.Dialogues)
            {
                var copy = dialogue.Copy();
                if(copy.SubtitleType == DialogueAsset.SubtitleTypeEnum.Single)
                {
                    copy.SingleSubtitle.Text.SubscribeGloc();
                }
                else
                {
                    foreach (var subtitle in copy.Subtitles)
                    {
                        if (subtitle is DialogueSubtitle sub)
                        {
                            sub.Text.SubscribeGloc();
                        }
                    }
                }

                foreach (var option in copy.Options)
                {
                    option.OptionText.SubscribeGloc();
                }

                DialogueData.Add(copy);
            }
        }

        private void Update()
        {
            if (DialogueType == DialogueTypeEnum.Global || !RangedDialogue || !isTriggered || IsCompleted)
                return;

            Vector3 playerPos = playerTransform.position;
            Vector3 targetPos = transform.position;
            float distance = Vector3.Distance(playerPos, targetPos);

            if(distance > LocalDialogueRange)
            {
                dialogueSystem.StopDialogue();
                isTriggered = !ResetDialogueWhenOut;
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (TriggerType != TriggerTypeEnum.Trigger)
                return;

            if (other.CompareTag("Player"))
                TriggerDialogue();
        }

        public void InteractStart()
        {
            if (TriggerType != TriggerTypeEnum.Interact)
                return;

            TriggerDialogue();
        }

        public void TriggerDialogue()
        {
            if (Dialogue == null || isTriggered)
                return;

            isTriggered = dialogueSystem.PlayDialogue(this);
        }

        public void ResetTrigger()
        {
            if (Repeatable)
                isTriggered = false;
        }

        public StorableCollection OnSave()
        {
            return new StorableCollection()
            {
                { nameof(IsCompleted), IsCompleted }
            };
        }

        public void OnLoad(JToken data)
        {
            IsCompleted = (bool)data[nameof(IsCompleted)];
        }

        private void OnDrawGizmosSelected()
        {
            if (DialogueType != DialogueTypeEnum.Local || !RangedDialogue)
                return;

#if UNITY_EDITOR
            Handles.color = Color.cyan;
            Handles.DrawWireDisc(transform.position, Vector3.up, LocalDialogueRange);
#endif
        }
    }
}