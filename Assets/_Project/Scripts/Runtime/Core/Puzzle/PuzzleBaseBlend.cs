using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using Unity.Cinemachine;
using UHFPS.Input;

namespace UHFPS.Runtime
{
    public abstract class PuzzleBaseBlend : MonoBehaviour, IInteractStart
    {
        public CinemachineCamera VirtualCamera;
        public CinemachineBlendDefinition BlendDefinition;
        public ControlsContext[] ControlsContexts;

        public List<Collider> CollidersEnable = new();
        public List<Collider> CollidersDisable = new();

        protected PlayerPresenceManager playerPresence;
        protected PlayerManager playerManager;
        protected GameManager gameManager;
        protected PlayerItemsManager playerItems;

        private CinemachineBrain cinemachineBrain;
        private CinemachineBlendDefinition defaultBlend;
        private bool canSwitch;

        /// <summary>
        /// Specifies whether the camera is currently switching.
        /// </summary>
        protected bool isBlending;

        /// <summary>
        /// Specifies when the camera is switched to a puzzle or normal camera. [true = puzzle, false = normal]
        /// </summary>
        protected bool isActive;

        /// <summary>
        /// Specifies when the camera can be switched back to normal camera using the default functionality.
        /// </summary>
        protected bool canManuallySwitch;

        /// <summary>
        /// Determines whether the colliders switch to puzzle mode or normal mode.
        /// </summary>
        protected bool switchColliders = true;

        public virtual void OnBlendedIn() { }
        public virtual void OnBlendedOut() { }
        public virtual void OnBlendStart(bool blendIn) { }

        public virtual void Awake()
        {
            playerPresence = PlayerPresenceManager.Instance;
            gameManager = GameManager.Instance;

            playerManager = playerPresence.PlayerManager;
            playerItems = playerManager.PlayerItems;

            cinemachineBrain = playerManager.MainCamera.GetComponent<CinemachineBrain>();
            defaultBlend = cinemachineBrain.DefaultBlend;

            foreach (var control in ControlsContexts)
            {
                control.SubscribeGloc();
            }
        }

        public virtual void Update()
        {
            if (isBlending || !isActive || !canManuallySwitch)
                return;

            if (InputManager.ReadButtonOnce(GetInstanceID(), Controls.EXAMINE))
                SwitchBack();
        }

        public virtual void InteractStart()
        {
            if (isActive)
                return;

            // set blend definition
            cinemachineBrain.DefaultBlend = BlendDefinition;

            OnBlendStart(true);
            InputManager.ResetToggledButtons();
            playerManager.MainVirtualCamera.enabled = false;
            VirtualCamera.gameObject.SetActive(true);

            // freeze player
            SetPlayerUsable(false);

            StartCoroutine(SwitchCamera(false));
            canManuallySwitch = true;
            isActive = true;
            canSwitch = false;
        }

        /// <summary>
        /// Calling this function switches the puzzle camera to the normal camera.
        /// </summary>
        protected void SwitchBack()
        {
            if (!canSwitch || !isActive)
                return;

            OnBlendStart(false);
            playerManager.MainVirtualCamera.enabled = true;
            VirtualCamera.gameObject.SetActive(false);

            gameManager.ShowControlsInfo(false, null);
            StartCoroutine(SwitchCamera(true));

            isActive = false;
            canSwitch = false;
        }

        private void SwitchedIn()
        {
            if (switchColliders)
            {
                CollidersEnable.ForEach(x => x.enabled = true);
                CollidersDisable.ForEach(x => x.enabled = false);
            }

            gameManager.ShowControlsInfo(true, ControlsContexts);
            canSwitch = true;
            OnBlendedIn();
        }

        private void SwitchedBack()
        {
            if (switchColliders)
            {
                CollidersEnable.ForEach(x => x.enabled = false);
                CollidersDisable.ForEach(x => x.enabled = true);
            }

            cinemachineBrain.DefaultBlend = defaultBlend;
            SetPlayerUsable(true);
            OnBlendedOut();
        }

        private void SetPlayerUsable(bool state)
        {
            if (playerManager.PlayerHealth.IsDead)
                return;

            playerPresence.FreezePlayer(!state);
            playerPresence.PlayerIsUnlocked = state;

            if (!state)
            {
                playerItems.IsItemsUsable = false;
                playerItems.DeactivateCurrentItem();
                gameManager.DisableAllGamePanels();
            }
            else
            {
                playerItems.IsItemsUsable = true;
                gameManager.ShowPanel(GameManager.PanelType.MainPanel);
            }
        }

        IEnumerator SwitchCamera(bool switchBack)
        {
            isBlending = true;

            // wait until the cameras are blending
            yield return new WaitForEndOfFrame();
            yield return new WaitUntil(() => cinemachineBrain.IsBlending);

            // wait for blend to complete
            CinemachineBlend blend = cinemachineBrain.ActiveBlend;
            yield return new WaitUntil(() => blend.IsComplete);
              
            if (switchBack) SwitchedBack();
            else SwitchedIn();

            isBlending = false;
        }
    }
}