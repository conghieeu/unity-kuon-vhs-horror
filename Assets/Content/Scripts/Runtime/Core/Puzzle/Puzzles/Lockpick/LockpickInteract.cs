using UnityEngine;
using UnityEngine.Events;
using UHFPS.Tools;
using TMPro;
using ThunderWire.Attributes;

namespace UHFPS.Runtime
{
    [Docs("https://docs.twgamesdev.com/uhfps/guides/puzzles#lockpick-puzzle")]
    public class LockpickInteract : MonoBehaviour, IDynamicUnlock, IInteractStart
    {
        public GameObject LockpickModel;

        [Range(-90f, 90f)]
        public float UnlockAngle;
        public bool RandomUnlockAngle;
        public bool IsDynamicUnlockComponent;

        public Vector3 LockpickRotation;
        public float LockpickDistance;
        public GString LockpicksText;
        public ControlsContext[] ControlsContexts;

        public ItemGuid BobbyPinItem;
        public MinMax BobbyPinLimits;

        public float BobbyPinUnlockDistance = 0.1f;
        public float BobbyPinLifetime = 2;
        public bool UnbreakableBobbyPin;

        [Range(0f, 90f)]
        public float KeyholeMaxTestRange = 20;
        public float KeyholeUnlockTarget = 0.1f;

        public UnityEvent OnUnlock;

        public GameObject LockpickUI;
        public TMP_Text LockpickText;

        public PlayerPresenceManager PlayerPresence;
        public PlayerManager PlayerManager;
        public DynamicObject DynamicObject;
        public GameManager GameManager;

        private Camera MainCamera => PlayerPresence.PlayerCamera;
        private bool isUnlocked;

        private void Awake()
        {
            PlayerPresence = PlayerPresenceManager.Instance;
            PlayerManager = PlayerPresence.PlayerManager;
            GameManager = GameManager.Instance;
            if (RandomUnlockAngle) UnlockAngle = Mathf.Floor(GameTools.Random(BobbyPinLimits));
        }

        private void Start()
        {
            foreach (var control in ControlsContexts)
            {
                control.SubscribeGloc();
            }

            LockpicksText.SubscribeGloc();

            var references = GameManager.GraphicReferences.Value["Lockpick"];
            LockpickUI = references[0].gameObject;
            LockpickText = (TMP_Text)references[1];
        }

        public void InteractStart()
        {
            if (IsDynamicUnlockComponent || isUnlocked) 
                return;

            AttemptToUnlock();
        }

        public void OnTryUnlock(DynamicObject dynamicObject)
        {
            if (!IsDynamicUnlockComponent || isUnlocked) 
                return;

            DynamicObject = dynamicObject;
            AttemptToUnlock();
        }

        public void AttemptToUnlock()
        {
            Vector3 holdPosition = MainCamera.transform.position + MainCamera.transform.forward * LockpickDistance;
            Quaternion faceRotation = Quaternion.LookRotation(MainCamera.transform.forward) * Quaternion.Euler(LockpickRotation);
            GameObject lockpickObj = Instantiate(LockpickModel, holdPosition, faceRotation, PlayerManager.MainVirtualCamera.transform);
            LockpickComponent lockpickComponent = lockpickObj.GetComponent<LockpickComponent>();

            PlayerManager.PlayerItems.IsItemsUsable = false;
            GameManager.FreezePlayer(true);
            GameManager.SetBlur(true, true);
            GameManager.DisableAllGamePanels();
            GameManager.ShowControlsInfo(true, ControlsContexts);
            lockpickComponent.SetLockpick(this);
        }

        public void Unlock()
        {
            if (isUnlocked) 
                return;

            if (IsDynamicUnlockComponent && DynamicObject != null) 
                DynamicObject.TryUnlockResult(true);
            else OnUnlock?.Invoke();

            isUnlocked = true;
        }
    }
}