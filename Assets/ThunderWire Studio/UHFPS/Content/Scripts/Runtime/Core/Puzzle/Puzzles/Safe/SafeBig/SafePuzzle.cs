using UnityEngine.Events;
using UnityEngine;
using TMPro;
using Newtonsoft.Json.Linq;
using UHFPS.Tools;

namespace UHFPS.Runtime
{
    public class SafePuzzle : MonoBehaviour, IInteractStart, ISaveable
    {
        public string UnlockCode = "000000";

        public GameObject WheelObject;
        public Vector3 WheelRotation;
        public float WheelDistance;
        public float FocusLightIntensity = 1f;

        public Animator Animator;
        public string UnlockTrigger = "Unlock";
        public string ResetTrigger = "Reset";

        public Color SolutionNormalColor = Color.black;
        public Color SolutionCurrentColor = Color.white;

        public Layer UnlockedLayer;
        public ControlsContext[] ControlsContexts;

        public bool LoadCallEvent;
        public UnityEvent OnUnlock;

        public GameManager GameManager;
        public PlayerManager PlayerManager;
        private PlayerPresenceManager PlayerPresence;
        private ExamineController ExamineController;

        public GameObject SafeLockPanel;
        public TMP_Text Number1;
        public TMP_Text Number2;
        public TMP_Text Number3;

        private Camera MainCamera => PlayerPresence.PlayerCamera;
        private bool isUnlocked;

        private void Reset()
        {
            UnlockCode = "000000";
        }

        private void Awake()
        {
            GameManager = GameManager.Instance;
            PlayerPresence = PlayerPresenceManager.Instance;
            PlayerManager = PlayerPresence.PlayerManager;
            ExamineController = PlayerManager.GetComponentInChildren<ExamineController>();

            foreach (var control in ControlsContexts)
            {
                control.SubscribeGloc();
            }
        }

        private void Start()
        {
            var references = GameManager.GraphicReferences.Value["SafeLock"];
            SafeLockPanel = references[0].gameObject;
            Number1 = (TMP_Text)references[1];
            Number2 = (TMP_Text)references[2];
            Number3 = (TMP_Text)references[3];
        }

        public void InteractStart()
        {
            Vector3 holdPosition = MainCamera.transform.position + MainCamera.transform.forward * WheelDistance;
            Quaternion faceRotation = Quaternion.LookRotation(MainCamera.transform.forward) * Quaternion.Euler(WheelRotation);

            GameObject wheelObj = Instantiate(WheelObject, holdPosition, faceRotation, PlayerManager.MainVirtualCamera.transform);
            SafeWheel safeWheel = wheelObj.GetComponent<SafeWheel>();
            ExamineController.SetExamineLight(FocusLightIntensity);

            PlayerManager.PlayerItems.IsItemsUsable = false;
            GameManager.FreezePlayer(true);
            GameManager.SetBlur(true, true);
            GameManager.DisableAllGamePanels();
            GameManager.ShowControlsInfo(true, ControlsContexts);
            safeWheel.SetSafe(this);
        }

        public void OnPuzzleQuit()
        {
            SafeLockPanel.SetActive(false);
            ExamineController.ResetExamineLight();
        }

        public void SetUnlocked()
        {
            gameObject.layer = UnlockedLayer;
            OnUnlock?.Invoke();

            if (Animator != null)
                Animator.SetTrigger(UnlockTrigger);
        }

        public StorableCollection OnSave()
        {
            return new StorableCollection()
            {
                { nameof(isUnlocked), isUnlocked }
            };
        }

        public void OnLoad(JToken data)
        {
            isUnlocked = (bool)data[nameof(isUnlocked)];

            if (isUnlocked)
            {
                gameObject.layer = UnlockedLayer;
                if (Animator != null) Animator.SetTrigger(ResetTrigger);
                if (LoadCallEvent) OnUnlock?.Invoke();
            }
        }
    }
}