using System.Collections;
using UnityEngine.Events;
using UnityEngine;
using Unity.Cinemachine;
using UHFPS.Input;
using UHFPS.Tools;
using UHFPS.Runtime.States;
using static UHFPS.Runtime.States.HidingStateAsset;

namespace UHFPS.Runtime
{
    public class HideInteract : MonoBehaviour, IInteractStart
    {
        public enum HideStyleEnum { Trigger, Interact }

        public HideStyleEnum HideStyle = HideStyleEnum.Trigger;
        public bool DrawGizmos = true;

        public Transform PlayerHidePosition;
        public Transform PlayerUnhidePosition;

        public CinemachineCamera VirtualCamera;
        public Animator Animator;
        public GString UnhideText;

        public CinemachineBlendDefinition BlendDefinition;
        [Range(0f, 1f)] public float BlendInOffset = 1f;
        [Range(0f, 1f)] public float BlendOutOffset = 1f;

        public string HideParameter = "IsHiding";
        public string DefaultStateName = "Default";
        public string HideStateName = "Hide";
        public string UnhideStateName = "Unhide";

        public UnityEvent OnHideStart;
        public UnityEvent OnHidden;
        public UnityEvent OnUnhide;

        public bool IsHidden;
        private bool isHiding;

        private PlayerPresenceManager presenceManager;
        private GameManager gameManager;

        private PlayerManager playerManager;
        private PlayerStateMachine stateMachine;
        private PlayerItemsManager playerItems;
        private MotionController motionController;
        private InteractController interactController;

        private CinemachineBrain cinemachineBrain;
        private CinemachineBlendDefinition defaultBlend;

        private HidingPlayerState _hideState;
        private HidingPlayerState HideState
        {
            get => _hideState ??= (HidingPlayerState)stateMachine.GetState<HidingStateAsset>();
        }

        private void Awake()
        {
            presenceManager = PlayerPresenceManager.Instance;
            gameManager = GameManager.Instance;

            playerManager = presenceManager.PlayerManager;
            stateMachine = presenceManager.StateMachine;
            playerItems = playerManager.PlayerItems;
            motionController = playerManager.MotionController;
            interactController = playerManager.InteractController;

            cinemachineBrain = playerManager.MainCamera.GetComponent<CinemachineBrain>();
        }

        private void Start()
        {
            UnhideText.SubscribeGloc();
        }

        private void OnTriggerEnter(Collider other)
        {
            if(!isHiding && HideStyle == HideStyleEnum.Trigger && other.CompareTag("Player"))
            {
                Hide();
                isHiding = true;
            }
        }

        public void InteractStart()
        {
            if (!isHiding && HideStyle == HideStyleEnum.Interact)
            {
                Hide();
                isHiding = true;
            }
        }

        public void Hide()
        {
            if (IsHidden)
                return;

            InputManager.ResetToggledButtons();
            playerManager.MainVirtualCamera.enabled = false;
            SetPlayerUsable(false);

            // set blend definition
            defaultBlend = cinemachineBrain.DefaultBlend;
            cinemachineBrain.DefaultBlend = BlendDefinition;

            // activate hide camera
            VirtualCamera.gameObject.SetActive(true);

            // start hide animation
            StartCoroutine(StartHideAnimation());

            // call hide start event
            OnHideStart?.Invoke();

            // change state to hiding state
            stateMachine.ChangeState(PlayerStateMachine.HIDING_STATE, new StorableCollection()
            {
                { "hideTrigger", this }
            });
        }

        public void Unhide(bool fromAI = false)
        {
            if (!IsHidden && !fromAI)
                return;

            if (fromAI) StartCoroutine(UnhideFromAI());
            else
            {
                Vector3 eulerAngles = PlayerUnhidePosition.eulerAngles;
                Vector2 newLook = new(eulerAngles.y, 0f);
                presenceManager.Teleport(PlayerUnhidePosition.position, newLook, false);

                gameManager.ShowControlsInfo(false);
                StartCoroutine(StartUnhideAnimation());
            }
        }

        public void SetPlayerHidden(bool state)
        {
            if (!isHiding)
                return;

            HideState.IsFullyHidden = state;
        }

        private void SetPlayerUsable(bool state)
        {
            if (playerManager.PlayerHealth.IsDead)
                return;

            presenceManager.FreezePlayer(!state);
            presenceManager.FreezeMovement(false);
            presenceManager.PlayerIsUnlocked = state;

            if (!state)
            {
                playerItems.DeactivateCurrentItem();
                gameManager.DisableAllGamePanels();
            }
            else
            {
                gameManager.ShowPanel(GameManager.PanelType.MainPanel);
            }
        }

        IEnumerator UnhideFromAI()
        {
            yield return new WaitUntil(() => IsHidden);
            Unhide(false);
        }

        IEnumerator StartHideAnimation()
        {
            yield return new WaitUntil(() => cinemachineBrain.IsBlending);
            CinemachineBlend blend = cinemachineBrain.ActiveBlend;

            while (blend != null && !blend.IsComplete)
            {
                if (blend.BlendWeight >= BlendInOffset)
                    break;

                yield return null;
            }

            Animator.SetBool(HideParameter, true);
            yield return new WaitForEndOfFrame();
            yield return new WaitForAnimatorStateEnd(Animator, HideStateName);

            // call hidden event
            OnHidden?.Invoke();

            // show unhide controls
            gameManager.ShowControlsInfo(true, new ControlsContext()
            {
                InputAction = interactController.UseAction,
                InteractName = UnhideText
            });

            Vector3 currentLook = VirtualCamera.transform.eulerAngles;
            Vector2 newLook = new(currentLook.y, 0f);
            presenceManager.Teleport(PlayerHidePosition.position, newLook, false);

            HideState.IsFullyHidden = true;
            interactController.ResetInteract();
            motionController.ResetMotions();
            stateMachine.ToStandingPose();
            IsHidden = true;
        }

        IEnumerator StartUnhideAnimation()
        {
            Animator.SetBool(HideParameter, false);
            yield return new WaitForEndOfFrame();

            HideState.IsFullyHidden = false;
            yield return new WaitForAnimatorClip(Animator, UnhideStateName, 1f - BlendOutOffset);

            playerManager.MainVirtualCamera.enabled = true;
            VirtualCamera.gameObject.SetActive(false);

            yield return new WaitUntil(() => cinemachineBrain.IsBlending);

            CinemachineBlend blend = cinemachineBrain.ActiveBlend;
            yield return new WaitUntil(() => blend.IsComplete);

            // call unhide event
            OnUnhide?.Invoke();

            ResetParameters();
        }

        private void ResetParameters()
        {
            Animator.Play(DefaultStateName);
            cinemachineBrain.DefaultBlend = defaultBlend;

            presenceManager.StateMachine.PlayerCollider.enabled = true;
            presenceManager.StateMachine.ChangeToIdle();

            SetPlayerUsable(true);
            IsHidden = false;
            isHiding = false;
        }

        private void OnDrawGizmos()
        {
            if (!DrawGizmos || PlayerUnhidePosition == null || !PlayerPresenceManager.HasReference)
                return;

            PlayerPresenceManager pp = PlayerPresenceManager.Instance;
            PlayerStateMachine stateMachine = pp.StateMachine;

            Vector3 position = PlayerUnhidePosition.position;
            Vector3 forward = PlayerUnhidePosition.forward;
            float radius = 0.25f;

            GizmosE.DrawDisc(position, radius, Color.green, Color.green.Alpha(0.01f));
            GizmosE.DrawGizmosArrow(position, forward * radius, 0.1f);

            float playerRadius = stateMachine.Controller.radius;
            float playerHeight = stateMachine.StandingState.ControllerHeight;
            Vector3 playerPos = position + Vector3.up * playerHeight / 2f;

            Gizmos.color = Color.green;
            GizmosE.DrawWireCapsule(playerPos, Quaternion.identity, playerRadius, playerHeight);

            Vector3 cameraOffset = stateMachine.StandingState.CameraOffset;
            Vector3 cameraPos = position + Vector3.up * playerHeight + cameraOffset;

            Gizmos.color = Color.red;
            Gizmos.DrawSphere(cameraPos, 0.025f);
        }
    }
}