using System;
using System.Reactive.Subjects;
using System.Reactive.Disposables;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UHFPS.Rendering;
using UHFPS.Scriptable;
using UHFPS.Input;
using UHFPS.Tools;
using ThunderWire.Attributes;
using TMText = TMPro.TMP_Text;

namespace UHFPS.Runtime
{
    [Docs("https://docs.twgamesdev.com/uhfps/guides/game-manager")]
    public class GameManager : Singleton<GameManager>
    {
        public enum PanelType { GamePanel, PausePanel, DeadPanel, MainPanel, InventoryPanel, MapPanel }

        [Serializable]
        public struct GraphicReference
        {
            public string Name;
            public Behaviour[] Graphics;
        }

        [Serializable]
        public struct ExaminePointer
        {
            public Sprite NormalSprite;
            public Sprite HoldSprite;
            public Vector2 PointerSize;
        }

        [Serializable]
        public struct DefaultPointer
        {
            public Sprite NormalSprite;
            public Vector2 PointerSize;
        }

        public ManagerModulesAsset Modules;
        public Volume GlobalPPVolume;
        public Volume HealthPPVolume;
        public BackgroundFader BackgroundFade;

        #region Panels
        // Main Panels
        public CanvasGroup GamePanel;
        public CanvasGroup PausePanel;
        public CanvasGroup DeadPanel;

        // Sub Panels
        public CanvasGroup HUDPanel;
        public CanvasGroup TabPanel;

        // Game Panels
        public CanvasGroup InventoryPanel;
        public CanvasGroup AlertsPanel;
        public Transform FloatingIcons;
        #endregion

        #region Pause
        public Button SaveGameButton;
        public Button LoadGameButton;
        #endregion

        #region UserInterface
        // Reticle
        public Image ReticleImage;
        public Image InteractProgress;
        public Slider StaminaSlider;

        // Interaction
        public InteractInfoPanel InteractInfoPanel;
        public ControlsInfoPanel ControlsInfoPanel;

        // Interact Pointer
        public Image PointerImage;
        public DefaultPointer NormalPointer;
        public DefaultPointer HoverPointer;
        public ExaminePointer ClickPointer;
        public ExaminePointer DragVerticalPointer;
        public ExaminePointer DragHorizontalPointer;

        // Item Pickup
        public Transform ItemPickupLayout;
        public GameObject ItemPickup;
        public float PickupMessageTime = 2f;

        // Hint Message
        public CanvasGroup HintMessageGroup;
        public float HintMessageFadeSpeed = 2f;

        // Health
        public Slider HealthBar;
        public Image Hearthbeat;
        public TMText HealthPercent;

        // Paper
        public CanvasGroup PaperPanel;
        public TMText PaperText;
        public float PaperFadeSpeed;

        // Examine
        public CanvasGroup ExamineInfoPanel;
        public Transform ExamineHotspots;
        public TMText ExamineText;
        public float ExamineFadeSpeed;

        // Overlays
        public GameObject OverlaysParent;
        #endregion

        public bool EnableBlur = true;
        public float BlurRadius = 5f;
        public float BlurDuration = 0.15f;

        public GraphicReference[] GraphicReferencesRaw;
        public CompositeDisposable Disposables = new();

        private bool isInputLocked;
        private bool showStaminaSlider;

        private bool isClicked;
        private bool isHolding;
        private bool isPointerShown;

        private int pointerCullLayers;
        private float defaultBlurRadius;

        private Layer pointerInteractLayer;
        private Action<RaycastHit, IInteractStart> pointerInteractAction;

        private CoroutineRunner blurCoroutine;
        private DialogueSystem dialogueSystem;

        private readonly BehaviorSubject<bool> IsPausedSubject = new(false);
        private readonly BehaviorSubject<bool> InventoryShownSubject = new(false);

        public bool IsPaused 
        {
            get => IsPausedSubject.Value;
            private set => IsPausedSubject.OnNext(value);
        }

        public bool IsInventoryShown 
        {
            get => InventoryShownSubject.Value;
            private set => InventoryShownSubject.OnNext(value);
        }

        public bool IsPointerHolding { get; private set; }

        private Inventory inventory;
        public Inventory Inventory
        {
            get
            {
                if (inventory == null)
                    inventory = GetComponent<Inventory>();

                return inventory;
            }
        }

        private PlayerPresenceManager playerPresence;
        public PlayerPresenceManager PlayerPresence
        {
            get
            {
                if (playerPresence == null)
                    playerPresence = GetComponent<PlayerPresenceManager>();

                return playerPresence;
            }
        }

        public bool PlayerDied => PlayerPresence.PlayerManager.PlayerHealth.IsDead;

        /// <summary>
        /// Get Custom Graphic References
        /// </summary>
        public Lazy<IDictionary<string, Behaviour[]>> GraphicReferences { get; } = new(() => 
        {
            Dictionary<string, Behaviour[]> referencesDict = new();

            foreach (var reference in Instance.GraphicReferencesRaw)
            {
                if (string.IsNullOrEmpty(reference.Name) || referencesDict.ContainsKey(reference.Name)) 
                    continue;

                referencesDict.Add(reference.Name, reference.Graphics);
            }

            return referencesDict;
        });

        private void Awake()
        {
            InputManager.Performed(Controls.PAUSE, OnPause);
            InputManager.Performed(Controls.INVENTORY, OnInventory);

            // update stamina slider value
            if (PlayerPresence.StateMachine.PlayerFeatures.EnableStamina)
            {
                PlayerPresence.StateMachine.Stamina.Subscribe(value =>
                {
                    StaminaSlider.value = value;
                    showStaminaSlider = value < 1f;
                })
                .AddTo(Disposables);
            }

            foreach (var module in Modules.ManagerModules)
            {
                if (module == null) continue;
                module.GameManager = this;
                module.OnAwake();
            }

            dialogueSystem = GetComponent<DialogueSystem>();
            DualKawaseBlur blur = GetStack<DualKawaseBlur>();
            if (blur != null) defaultBlurRadius = blur.BlurRadius.value;
        }

        private void Start()
        {
            foreach (var module in Modules.ManagerModules)
            {
                if (module == null) continue;
                module.OnStart();
            }
        }

        private void OnDestroy()
        {
            Disposables?.Dispose();
        }

        private void Update()
        {
            if (isPointerShown)
            {
                Vector2 pointerDelta = InputManager.ReadInput<Vector2>(Controls.POINTER_DELTA);
                Vector3 pointerPos = PointerImage.transform.position;

                if (!IsPointerHolding)
                {
                    pointerPos.x = Mathf.Clamp(pointerPos.x + pointerDelta.x, 0, Screen.width);
                    pointerPos.y = Mathf.Clamp(pointerPos.y + pointerDelta.y, 0, Screen.height);
                    PointerImage.transform.position = pointerPos;
                }

                Ray pointerRay = PlayerPresence.PlayerCamera.ScreenPointToRay(pointerPos);
                bool isRaycast = false;

                if (GameTools.Raycast(pointerRay, out RaycastHit hit, 5, pointerCullLayers, pointerInteractLayer))
                {
                    GameObject obj = hit.collider.gameObject;
                    Sprite pointerSprite = NormalPointer.NormalSprite;
                    Vector2 pointerSize = NormalPointer.PointerSize;

                    if (!isHolding)
                    {
                        if (obj.TryGetComponent(out IExamineClick click))
                        {
                            isRaycast = true;
                            pointerSprite = ClickPointer.NormalSprite;
                            pointerSize = ClickPointer.PointerSize;

                            if (InputManager.ReadButton(Controls.LEFT_BUTTON))
                            {
                                pointerSprite = ClickPointer.HoldSprite;
                                IsPointerHolding = true;

                                if (!isClicked)
                                {
                                    click.OnExamineClick();
                                    isClicked = true;
                                }
                            }
                            else
                            {
                                isClicked = false;
                                IsPointerHolding = false;
                            }
                        }
                        else if (obj.TryGetComponent(out IExamineDragVertical dragVertical))
                        {
                            isRaycast = true;
                            pointerSprite = DragVerticalPointer.NormalSprite;
                            pointerSize = DragVerticalPointer.PointerSize;

                            if (InputManager.ReadButton(Controls.LEFT_BUTTON))
                            {
                                pointerSprite = DragVerticalPointer.HoldSprite;
                                dragVertical.OnExamineDragVertical(pointerDelta.y);
                                IsPointerHolding = true;
                            }
                            else
                            {
                                IsPointerHolding = false;
                            }
                        }
                        else if (obj.TryGetComponent(out IExamineDragHorizontal dragHorizontal))
                        {
                            isRaycast = true;
                            pointerSprite = DragHorizontalPointer.NormalSprite;
                            pointerSize = DragHorizontalPointer.PointerSize;

                            if (InputManager.ReadButton(Controls.LEFT_BUTTON))
                            {
                                pointerSprite = DragHorizontalPointer.HoldSprite;
                                dragHorizontal.OnExamineDragHorizontal(pointerDelta.x);
                                IsPointerHolding = true;
                            }
                            else
                            {
                                IsPointerHolding = false;
                            }
                        }
                        else if (obj.TryGetComponent(out IInteractStart interactStart) && pointerInteractAction != null)
                        {
                            isRaycast = true;
                            pointerSprite = HoverPointer.NormalSprite;
                            pointerSize = HoverPointer.PointerSize;

                            if (InputManager.ReadButtonOnce(GetInstanceID(), Controls.LEFT_BUTTON))
                            {
                                pointerInteractAction?.Invoke(hit, interactStart);
                            }
                        }
                    }

                    PointerImage.sprite = pointerSprite;
                    PointerImage.rectTransform.sizeDelta = pointerSize;
                }
                else
                {
                    PointerImage.sprite = NormalPointer.NormalSprite;
                    PointerImage.rectTransform.sizeDelta = NormalPointer.PointerSize;
                    IsPointerHolding = false;
                    isClicked = false;
                }

                isHolding = !isRaycast && !IsPointerHolding && InputManager.ReadButton(Controls.LEFT_BUTTON);
            }
            else
            {
                isClicked = false;
                isHolding = false;
                IsPointerHolding = false;
            }

            // update manager modules
            foreach (var module in Modules.ManagerModules)
            {
                if (module == null) 
                    continue;

                module.OnUpdate();
            }

            // update stamina slider alpha
            if (PlayerPresence.StateMachine.PlayerFeatures.EnableStamina)
            {
                CanvasGroup staminaGroup = StaminaSlider.GetComponent<CanvasGroup>();
                staminaGroup.alpha = Mathf.MoveTowards(staminaGroup.alpha, showStaminaSlider ? 1f : 0f, Time.deltaTime * 3f);
            }
        }

        /// <summary>
        /// Subscribe listening to On Paused event.
        /// </summary>
        /// <param name="onPaused">Status, whether the game is paused.</param>
        public static void SubscribePauseEvent(Action<bool> onPaused)
        {
            var disposable = Instance.IsPausedSubject.Subscribe(onPaused);
            Instance.Disposables.Add(disposable);
        }

        /// <summary>
        /// Subscribe listening to On Inventory Shown event.
        /// </summary>
        /// <param name="onInventoryShown">Status, whether the inventory is shown.</param>
        public static void SubscribeInventoryEvent(Action<bool> onInventoryShown)
        {
            var disposable = Instance.InventoryShownSubject.Subscribe(onInventoryShown);
            Instance.Disposables.Add(disposable);
        }

        /// <summary>
        /// Start background fade.
        /// </summary>
        public IEnumerator StartBackgroundFade(bool fadeOut, float waitTime = 0, float fadeSpeed = 3) 
            => BackgroundFade.StartBackgroundFade(fadeOut, waitTime, fadeSpeed);

        /// <summary>
        /// Get GameManager Module.
        /// </summary>
        public static T Module<T>() where T : ManagerModule
        {
            foreach (var module in Instance.Modules.ManagerModules)
            {
                if (module.GetType() == typeof(T))
                    return (T)module;
            }

            return default;
        }

        /// <summary>
        /// When the next level is loaded, only the player's data is loaded.
        /// </summary>
        public void LoadNextLevel(string sceneName)
        {
            StopAllCoroutines();
            StartCoroutine(LoadNext(sceneName, false));
        }

        /// <summary>
        /// When the next level is loaded, the player's data and the world state are loaded.
        /// </summary>
        public void LoadNextWorld(string sceneName)
        {
            StopAllCoroutines();
            StartCoroutine(LoadNext(sceneName, true));
        }

        /// <summary>
        /// Load last saved world state of the current scene.
        /// </summary>
        public void LoadWorldLastState()
        {
            StopAllCoroutines();
            StartCoroutine(LoadLast());
        }

        /// <summary>
        /// When the next level is loaded, the player's data and the world state are loaded from the game state.
        /// </summary>
        /// <remarks>If sceneName is null, the current scene is used.</remarks>
        public void LoadGameState(string sceneName, string folderName)
        {
            if (string.IsNullOrEmpty(sceneName))
                sceneName = SceneManager.GetActiveScene().name;

            StartCoroutine(LoadGame(sceneName, folderName));
        }

        private IEnumerator LoadLast()
        {
            yield return BackgroundFade.StartBackgroundFade(false);
            yield return new WaitForEndOfFrame();

            SaveGameManager.SetLoadWorldLast();
            SceneManager.LoadScene(SaveGameManager.LMS);
        }

        private IEnumerator LoadNext(string sceneName, bool worldState)
        {
            yield return BackgroundFade.StartBackgroundFade(false);
            yield return new WaitForEndOfFrame();

            if (worldState) SaveGameManager.SetLoadWorldState(sceneName);
            else SaveGameManager.SetLoadPlayerData(sceneName);
            SceneManager.LoadScene(SaveGameManager.LMS);
        }

        private IEnumerator LoadGame(string sceneName, string folderName)
        {
            yield return BackgroundFade.StartBackgroundFade(false);
            yield return new WaitForEndOfFrame();

            SaveGameManager.SetLoadGameState(sceneName, folderName);
            SceneManager.LoadScene(SaveGameManager.LMS);
        }

        /// <summary>
        /// Freeze Player Controls.
        /// </summary>
        public void FreezePlayer(bool state, bool showCursor = false, bool lockInput = true)
        {
            PlayerPresence.FreezePlayer(state, showCursor);
            isInputLocked = lockInput && state;
        }

        /// <summary>
        /// Set the active state of the PostProcessing Volume.
        /// </summary>
        public void SetStack<T>(bool active) where T : VolumeComponent
        {
            if (GlobalPPVolume.profile.TryGet(out T component))
            {
                component.active = active;
            }
        }

        /// <summary>
        /// Get PostProcessing Volume component.
        /// </summary>
        public T GetStack<T>() where T : VolumeComponent
        {
            if (GlobalPPVolume.profile.TryGet(out T component))
                return component;

            return default;
        }

        /// <summary>
        /// Set the blur PostProcessing Volume active state.
        /// </summary>
        public void SetBlur(bool active, bool interpolate = false)
        {
            if (!EnableBlur) 
                return;

            if (!interpolate)
            {
                DualKawaseBlur blur = GetStack<DualKawaseBlur>();
                if (blur == null) return;

                blur.BlurRadius.value = defaultBlurRadius;
                blur.active = active;
            }
            else if (active)
            {
                InterpolateBlur(BlurRadius, BlurDuration);
            }
            else
            {
                InterpolateBlur(0f, BlurDuration);
            }
        }

        /// <summary>
        /// Interpolate blur radius over time.
        /// </summary>
        public void InterpolateBlur(float blurRadius, float duration)
        {
            if (!EnableBlur) 
                return;

            DualKawaseBlur blur = GetStack<DualKawaseBlur>();
            if (blur == null) return;

            if (blurRadius > 0) blur.BlurRadius.value = 0f;
            blur.active = true;

            if (blurCoroutine != null) blurCoroutine.Stop();
            blurCoroutine = CoroutineRunner.Run(gameObject, InterpolateBlur(blur, blurRadius, duration));
        }

        IEnumerator InterpolateBlur(DualKawaseBlur blur, float targetRadius, float duration)
        {
            float startBlurRadius = blur.BlurRadius.value;
            float elapsedTime = 0;

            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                blur.BlurRadius.value = Mathf.Lerp(startBlurRadius, targetRadius, elapsedTime / duration);
                yield return null;
            }

            blur.BlurRadius.value = targetRadius;
            if(targetRadius <= 0) blur.active = false;
        }

        /// <summary>
        /// Set the interaction state of the save and load button.
        /// </summary>
        /// <remarks>
        /// Useful if you need to disable the save and load functionality when you are in a certain zone. (e.g. inside elevator, cutscene)
        /// </remarks>
        public void SetSaveInteractable(bool saveBtnState, bool loadBtnState)
        {
            SaveGameButton.interactable = saveBtnState;
            LoadGameButton.interactable = loadBtnState;
        }

        /// <summary>
        /// Show timed interact progress.
        /// </summary>
        public void ShowInteractProgress(bool show, float progress)
        {
            if (show)
            {
                InteractProgress.enabled = true;
                InteractProgress.fillAmount = progress;
            }
            else
            {
                InteractProgress.fillAmount = 0f;
                InteractProgress.enabled = false;
            }
        }

        /// <summary>
        /// Block input to prevent the Inventory or Pause menu from opening when using other functions.
        /// </summary>
        public void LockInput(bool state)
        {
            isInputLocked = state;
        }

        /// <summary>
        /// Show the helper controls panel at the bottom of the screen.
        /// </summary>
        public void ShowControlsInfo(bool show, params ControlsContext[] contexts)
        {
            if (show)
            {
                ControlsInfoPanel.ShowInfo(contexts);
            }
            else
            {
                ControlsInfoPanel.HideInfo();
            }
        }

        /// <summary>
        /// Show the panel to examine the text of the paper document.
        /// </summary>
        public void ShowPaperInfo(bool show, bool noFade, string paperText = "")
        {
            if (!noFade)
            {
                PaperText.text = paperText;
                StartCoroutine(CanvasGroupFader.StartFade(PaperPanel, show, PaperFadeSpeed, () =>
                {
                    if(!show) PaperText.text = string.Empty;
                }));
                return;
            }

            if (show)
            {
                PaperPanel.gameObject.SetActive(true);
                PaperText.text = paperText;
                PaperPanel.alpha = 1;
            }
            else
            {
                PaperPanel.gameObject.SetActive(false);
                PaperText.text = string.Empty;
                PaperPanel.alpha = 0;
            }
        }

        /// <summary>
        /// Show info about the item being examined.
        /// </summary>
        public void ShowExamineInfo(bool show, bool noFade, string examineText = "")
        {
            StopAllCoroutines();

            if (!noFade)
            {
                ExamineText.text = examineText;
                StartCoroutine(CanvasGroupFader.StartFade(ExamineInfoPanel, show, ExamineFadeSpeed, () =>
                {
                    if (!show) ExamineText.text = string.Empty;
                }));
                return;
            }

            if (show)
            {
                ExamineText.text = examineText;
                ExamineInfoPanel.alpha = 1;
            }
            else
            {
                ExamineText.text = string.Empty;
                ExamineInfoPanel.alpha = 0;
            }
        }

        /// <summary>
        /// Show the game panel.
        /// </summary>
        /// <param name="panel"></param>
        public void ShowPanel(PanelType panel)
        {
            switch (panel)
            {
                case PanelType.PausePanel:
                    SetPanelInteractable(panel);
                    GamePanel.alpha = 0;
                    PausePanel.alpha = 1;
                    DeadPanel.alpha = 0;
                    break;
                case PanelType.GamePanel:
                    SetPanelInteractable(panel);
                    GamePanel.alpha = 1;
                    PausePanel.alpha = 0;
                    DeadPanel.alpha = 0;
                    break;
                case PanelType.DeadPanel:
                    SetPanelInteractable(panel);
                    GamePanel.alpha = 0;
                    PausePanel.alpha = 0;
                    DeadPanel.alpha = 1;
                    break;
                case PanelType.MainPanel:
                    SetPanelInteractable(PanelType.GamePanel);
                    GamePanel.alpha = 1;
                    PausePanel.alpha = 0;
                    DeadPanel.alpha = 0;
                    DisableAllGamePanels();
                    HUDPanel.alpha = 1;
                    AlertsPanel.alpha = 1;
                    break;
                case PanelType.InventoryPanel:
                    SetPanelInteractable(PanelType.GamePanel);
                    GamePanel.alpha = 1;
                    PausePanel.alpha = 0;
                    DeadPanel.alpha = 0;
                    DisableAllGamePanels();
                    AlertsPanel.alpha = 0;
                    InventoryPanel.alpha = 1;
                    TabPanel.alpha = 1;
                    IsInventoryShown = true;
                    break;
            }
        }

        /// <summary>
        /// Set the panel as interactable.
        /// </summary>
        public void SetPanelInteractable(PanelType panel)
        {
            GamePanel.interactable = panel == PanelType.GamePanel;
            GamePanel.blocksRaycasts = panel == PanelType.GamePanel;

            PausePanel.interactable = panel == PanelType.PausePanel;
            PausePanel.blocksRaycasts = panel == PanelType.PausePanel;

            DeadPanel.interactable = panel == PanelType.DeadPanel;
            DeadPanel.blocksRaycasts = panel == PanelType.DeadPanel;
        }

        /// <summary>
        /// Disable all game panels. (HUD, Tab, Inventory etc.)
        /// </summary>
        public void DisableAllGamePanels()
        {
            HUDPanel.alpha = 0;
            TabPanel.alpha = 0;
            InventoryPanel.alpha = 0;
            IsInventoryShown = false;
        }

        /// <summary>
        /// Disable all feature panels. (Inventory, Alerts etc.)
        /// </summary>
        public void DisableAllFeaturePanels()
        {
            InventoryPanel.alpha = 0;
            AlertsPanel.alpha = 0;
        }

        /// <summary>
        /// Show game inventory panel.
        /// </summary>
        public void ShowInventoryPanel(bool state)
        {
            if (PlayerPresence.IsUnlockedAndCamera && !PlayerDied && !isInputLocked && !IsPaused)
            {
                // set inventory status
                IsInventoryShown = state;

                // freeze player functions
                PlayerPresence.FreezePlayer(IsInventoryShown, IsInventoryShown);

                // show blur
                if (IsInventoryShown) InterpolateBlur(BlurRadius, BlurDuration);
                else InterpolateBlur(0, BlurDuration);

                // set panel visibility
                if (IsInventoryShown)
                {
                    ShowPanel(PanelType.InventoryPanel);
                    ShowControlsInfo(true, Inventory.ControlsContexts);
                    OverlaysParent.SetActive(false);
                    if(dialogueSystem != null) dialogueSystem.ForceShow(false);
                }
                else
                {
                    Inventory.OnCloseInventory();
                    ShowPanel(PanelType.MainPanel);
                    ShowControlsInfo(false, null);
                    OverlaysParent.SetActive(true);
                    if (dialogueSystem != null) dialogueSystem.ForceShow(true);
                }
            }
        }

        /// <summary>
        /// Show interaction/mouse pointer.
        /// </summary>
        public void ShowPointer(int cullLayers, Layer interactLayer, Action<RaycastHit, IInteractStart> interactAction)
        {
            isPointerShown = true;
            pointerCullLayers = cullLayers;
            pointerInteractLayer = interactLayer;
            pointerInteractAction = interactAction;
            PointerImage.gameObject.SetActive(true);
        }

        /// <summary>
        /// Show interaction/mouse pointer.
        /// </summary>
        public void ShowPointer(int cullLayers, Layer interactLayer)
        {
            isPointerShown = true;
            pointerCullLayers = cullLayers;
            pointerInteractLayer = interactLayer;
            PointerImage.gameObject.SetActive(true);
        }

        /// <summary>
        /// Set interaction/mouse pointer enabled state.
        /// </summary>
        public void EnablePointer(bool enable)
        {
            isPointerShown = enable;
            PointerImage.gameObject.SetActive(enable);
        }

        /// <summary>
        /// Hide interaction/mouse pointer.
        /// </summary>
        public void HidePointer()
        {
            isPointerShown = false;
            pointerInteractAction = null;
            pointerCullLayers = -1;
            pointerInteractLayer = -1;
            PointerImage.gameObject.SetActive(false);
            PointerImage.rectTransform.anchoredPosition = Vector2.zero;
        }

        /// <summary>
        /// Reset interaction/mouse pointer sprite and size.
        /// </summary>
        public void ResetPointer()
        {
            PointerImage.sprite = NormalPointer.NormalSprite;
            PointerImage.rectTransform.sizeDelta = NormalPointer.PointerSize;
        }

        /// <summary>
        /// Show a message when you pickup something.
        /// </summary>
        public void ShowItemPickupMessage(string text, Sprite icon, float time)
        {
            GameObject pickupElement = Instantiate(ItemPickup, ItemPickupLayout);
            ItemPickupElement element = pickupElement.GetComponent<ItemPickupElement>();
            element.ShowItemPickup(text, icon, time);
        }

        /// <summary>
        /// Show the hint at the top of the screen.
        /// </summary>
        public void ShowHintMessage(string text, float time)
        {
            TMText tmpText = HintMessageGroup.GetComponentInChildren<TMText>();
            tmpText.text = text;

            StopAllCoroutines();
            StartCoroutine(ShowHintMessage(time));
        }

        IEnumerator ShowHintMessage(float time)
        {
            yield return CanvasGroupFader.StartFade(HintMessageGroup, true, HintMessageFadeSpeed);

            yield return new WaitForSeconds(time);

            yield return CanvasGroupFader.StartFade(HintMessageGroup, false, HintMessageFadeSpeed, () =>
            {
                HintMessageGroup.gameObject.SetActive(false);
            });
        }

        /// <summary>
        /// Restart the game from the last saved game.
        /// </summary>
        public void RestartGame()
        {
            string sceneName = SaveGameManager.LoadSceneName;
            string saveName = SaveGameManager.LoadFolderName;
            LoadGameState(sceneName, saveName);
        }

        public void ResumeGame()
        {
            IsPaused = false;
            ShowPanel(PanelType.GamePanel);

            // reset panels
            SetPanelInteractable(PanelType.GamePanel);

            // hide blur
            InterpolateBlur(0, BlurDuration);

            // un-freeze player functions
            if (PlayerPresence.PlayerIsUnlocked)
                PlayerPresence.FreezePlayer(false, false);
        }

        public void MainMenu()
        {
            StartCoroutine(LoadMainMenu());
        }

        private IEnumerator LoadMainMenu()
        {
            yield return BackgroundFade.StartBackgroundFade(false);
            yield return new WaitForEndOfFrame();
            SceneManager.LoadScene(SaveGameManager.MM);
        }

        private void OnPause(InputAction.CallbackContext obj)
        {
            if (obj.ReadValueAsButton() && PlayerPresence.IsUnlockedAndCamera && !PlayerDied && !isInputLocked && !IsInventoryShown)
            {
                // set panel visibility
                IsPaused = !IsPaused;
                GamePanel.alpha = IsPaused ? 0 : 1;
                PausePanel.alpha = IsPaused ? 1 : 0;
                SetPanelInteractable(IsPaused ? PanelType.PausePanel : PanelType.GamePanel);

                // show blur
                if (IsPaused) InterpolateBlur(BlurRadius, BlurDuration);
                else InterpolateBlur(0, BlurDuration);

                // freeze player functions
                if (PlayerPresence.PlayerIsUnlocked)
                    PlayerPresence.FreezePlayer(IsPaused, IsPaused);
            }
        }

        private void OnInventory(InputAction.CallbackContext obj)
        {
            if (obj.ReadValueAsButton()) ShowInventoryPanel(!IsInventoryShown);
        }
    }
}