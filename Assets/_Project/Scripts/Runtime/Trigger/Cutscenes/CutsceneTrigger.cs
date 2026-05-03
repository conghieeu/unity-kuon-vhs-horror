using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Playables;
using Unity.Cinemachine;
using Newtonsoft.Json.Linq;
using UHFPS.Tools;

namespace UHFPS.Runtime
{
    public class CutsceneTrigger : MonoBehaviour, IInteractStart, ISaveable
    {
        public enum TriggerTypeEnum { Trigger, Interact, Event }
        public enum CutsceneTypeEnum { CameraCutscene, PlayerCutscene }

        public TriggerTypeEnum TriggerType;
        public CutsceneTypeEnum CutsceneType;
        public PlayableDirector Cutscene;
        public CutscenePlayer CutscenePlayer;

        public CinemachineCamera CutsceneCamera;
        public float CutsceneFadeSpeed;

        public CinemachineBlendDefinition BlendDefinition;
        [Tooltip("This is the asset that contains custom settings for blends between specific virtual cameras in your scene.")]
        public CinemachineBlenderSettings CustomBlendAsset;

        [Tooltip("Wait for the dialogue to finish before starting the cutscene.")]
        public bool WaitForDialogue = true;
        [Tooltip("Wait for the camera to blend into cutscene camera before starting the cutscene.")]
        public bool WaitForBlendIn = true;

        [Tooltip("The time offset at which the cutscene starts during the camera blend.")]
        [Range(0f, 1f)] public float BlendInOffset = 1f;
        [Tooltip("The time at which the cutscene camera blends in to player camera.")]
        public float BlendOutTime = 1f;

        public Transform CutEndTransform;
        public float CutFadeInSpeed = 3f;
        public float CutFadeOutSpeed = 3f;
        public bool DrawCutEndGizmos;

        public UnityEvent OnCutsceneStart;
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