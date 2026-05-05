using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Playables;
using Unity.Cinemachine;
using Newtonsoft.Json.Linq;
using UHFPS.Tools;
using ThunderWire.Attributes;

namespace UHFPS.Runtime
{
    [Summary("Kích hoạt và quản lý logic của một đoạn cutscene khi người chơi tương tác hoặc đi vào vùng trigger.")]
    public class CutsceneTrigger : MonoBehaviour, IInteractStart, ISaveable
    {
        public enum TriggerTypeEnum { Trigger, Interact, Event }
        public enum CutsceneTypeEnum { CameraCutscene, PlayerCutscene }

        [Tooltip("Loại kích hoạt cutscene.")]
        public TriggerTypeEnum TriggerType;

        [Tooltip("Loại cutscene (Camera hoặc Player).")]
        public CutsceneTypeEnum CutsceneType;

        [Tooltip("Playable Director chứa timeline của cutscene.")]
        public PlayableDirector Cutscene;

        [Tooltip("Script điều khiển người chơi trong cutscene (chỉ dành cho Player Cutscene).")]
        public CutscenePlayer CutscenePlayer;

        [Tooltip("Camera ảo được sử dụng cho Camera Cutscene.")]
        public CinemachineCamera CutsceneCamera;

        [Tooltip("Tốc độ hiệu ứng mờ (fade) khi bắt đầu/kết thúc Camera Cutscene.")]
        public float CutsceneFadeSpeed;

        [Tooltip("Cấu hình pha trộn (blend) camera.")]
        public CinemachineBlendDefinition BlendDefinition;

        [Tooltip("Asset chứa các cài đặt tùy chỉnh để pha trộn giữa các camera ảo cụ thể trong scene.")]
        public CinemachineBlenderSettings CustomBlendAsset;

        [Tooltip("Đợi hội thoại kết thúc trước khi bắt đầu cutscene.")]
        public bool WaitForDialogue = true;

        [Tooltip("Đợi camera pha trộn xong vào camera cutscene trước khi bắt đầu cutscene.")]
        public bool WaitForBlendIn = true;

        [Tooltip("Độ lệch thời gian (0-1) mà cutscene bắt đầu trong quá trình pha trộn camera.")]
        [Range(0f, 1f)] public float BlendInOffset = 1f;

        [Tooltip("Thời gian pha trộn từ camera cutscene quay lại camera người chơi.")]
        public float BlendOutTime = 1f;

        [Tooltip("Vị trí kết thúc của người chơi nếu sử dụng kiểu pha trộn 'Cut'.")]
        public Transform CutEndTransform;

        [Tooltip("Tốc độ hiện hình (fade in) khi dùng kiểu pha trộn 'Cut'.")]
        public float CutFadeInSpeed = 3f;

        [Tooltip("Tốc độ ẩn hình (fade out) khi dùng kiểu pha trộn 'Cut'.")]
        public float CutFadeOutSpeed = 3f;

        [Tooltip("Hiển thị Gizmos vị trí kết thúc của người chơi.")]
        public bool DrawCutEndGizmos;

        [Header("Events")]
        [Tooltip("Sự kiện kích hoạt khi cutscene bắt đầu.")]
        public UnityEvent OnCutsceneStart;

        [Tooltip("Sự kiện kích hoạt khi cutscene kết thúc.")]
        public UnityEvent OnCutsceneEnd;

        private DialogueSystem dialogueSystem;
        private CutsceneModule cutscene;
        private bool isPlayed;

        private void Awake()
        {
            cutscene = GameManager.Module<CutsceneModule>();
            dialogueSystem = DialogueSystem.Instance;
        }

        private void Start()
        {
            // rebuild playable graph to ensure seamless transition
            if (Cutscene != null) Cutscene.RebuildGraph();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (TriggerType != TriggerTypeEnum.Trigger)
                return;

            if (other.CompareTag("Player"))
                TriggerCutscene();
        }

        public void InteractStart()
        {
            if (TriggerType != TriggerTypeEnum.Interact)
                return;

            TriggerCutscene();
        }

        public void TriggerCutscene()
        {
            if (Cutscene == null || isPlayed || (WaitForDialogue && dialogueSystem.IsPlaying))
                return;

            cutscene.PlayCutscene(this);
            OnCutsceneStart?.Invoke();
            isPlayed = true;
        }

        private void OnDrawGizmos()
        {
            if (!DrawCutEndGizmos || BlendDefinition.Style != CinemachineBlendDefinition.Styles.Cut || CutEndTransform == null || !PlayerPresenceManager.HasReference)
                return;

            CharacterController controller = PlayerPresenceManager.Instance.StateMachine.PlayerCollider;

            float offset = 0.6f;
            float height = (controller.height + offset) / 2f;
            float radius = controller.radius;

            Vector3 origin = CutEndTransform.position + Vector3.up * (offset / 2f);
            Vector3 p2 = origin + Vector3.up * height;
            Vector3 p1 = origin;

            Gizmos.color = Color.red;
            GizmosE.DrawWireCapsule(p1, p2, radius);

            Gizmos.color = Color.green;
            GizmosE.DrawGizmosArrow(CutEndTransform.position, CutEndTransform.forward * 0.5f);
        }

        public StorableCollection OnSave()
        {
            return new StorableCollection()
            {
                { nameof(isPlayed), isPlayed }
            };
        }

        public void OnLoad(JToken data)
        {
            isPlayed = (bool)data[nameof(isPlayed)];
        }
    }
}