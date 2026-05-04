using UnityEngine;
using UnityEngine.Events;
using UHFPS.Input;
using UHFPS.Tools;
using ThunderWire.Attributes;
using Newtonsoft.Json.Linq;

namespace UHFPS.Runtime
{
    public enum DynamicSoundType { Open, Close, Locked, Unlock }

    [Docs("https://docs.twgamesdev.com/uhfps/guides/dynamic-objects")]
    public class DynamicObject : MonoBehaviour, IInteractStartPlayer, IInteractHold, IInteractStop, ISaveable
    {
        public enum DynamicType { Openable, Pullable, Switchable, Rotable }
        public enum TransformType { Local, Global }
        public enum InteractType { Dynamic, Mouse, Animation }
        public enum DynamicStatus { Normal, Locked }
        public enum StatusChange { InventoryItem, CustomScript, None }

        // enums
        public DynamicType dynamicType = DynamicType.Openable;
        public TransformType transformType = TransformType.Local;
        public DynamicStatus dynamicStatus = DynamicStatus.Normal;
        public InteractType interactType = InteractType.Dynamic;
        public StatusChange statusChange = StatusChange.InventoryItem;

        // general
        public Transform target;
        public AudioSource audioSource;
        public Animator animator;
        public HingeJoint joint;
        public new Rigidbody rigidbody;
        public Inventory inventory;
        public GameManager gameManager;

        // items
        [RequireInterface(typeof(IDynamicUnlock))]
        public MonoBehaviour unlockScript;
        public bool keepUnlockItem;
        public ItemGuid unlockItem;
        public bool showLockedText;
        public GString lockedText;

        public Collider[] ignoreColliders;
        public bool ignorePlayerCollider;

        public string useTrigger1 = "Open";
        public string useTrigger2 = "Close";
        public string useTrigger3 = "OpenSide";

        // dynamic types
        public DynamicOpenable openable = new DynamicOpenable();
        public DynamicPullable pullable = new DynamicPullable();
        public DynamicSwitchable switchable = new DynamicSwitchable();
        public DynamicRotable rotable = new DynamicRotable();

        // sounds
        public SoundClip useSound1;
        public SoundClip useSound2;
        public SoundClip lockedSound;
        public SoundClip unlockSound;

        // events
        public UnityEvent useEvent1;
        public UnityEvent useEvent2;
        public UnityEvent<float> onValueChange;
        public UnityEvent lockedEvent;
        public UnityEvent unlockedEvent;

        // hidden variables
        [Tooltip("Lock player controls when interacting with a dynamic object.")]
        public bool lockPlayer;
        public bool isLocked;
        public bool isInteractLocked;

        public DynamicObjectType CurrentDynamic
        {
            get => dynamicType switch
            {
                DynamicType.Openable => openable,
                DynamicType.Pullable => pullable,
                DynamicType.Switchable => switchable,
                DynamicType.Rotable => rotable,
                _ => null,
            };
        }

        public bool IsOpened => CurrentDynamic.IsOpened;

        public bool IsHolding => CurrentDynamic.IsHolding;

        private void OnValidate()
        {
            openable.DynamicObject = this;
            pullable.DynamicObject = this;
            switchable.DynamicObject = this;
            rotable.DynamicObject = this;
        }

        private void Awake()
        {
            inventory = Inventory.Instance;
            gameManager = GameManager.Instance;

            if (dynamicStatus == DynamicStatus.Locked)
                isLocked = true;

            CurrentDynamic?.OnDynamicInit();
        }

        private void Start()
        {
            if(interactType == InteractType.Mouse)
            {
                Collider collider = GetComponent<Collider>();
                foreach (var col in ignoreColliders)
                {
                    Physics.IgnoreCollision(collider, col);
                }
            }

            if(dynamicType == DynamicType.Pullable && ignorePlayerCollider)
            {
                Collider player = gameManager.PlayerPresence.Player.GetComponent<CharacterController>();
                Collider collider = GetComponent<Collider>();
                Physics.IgnoreCollision(player, collider);
            }

            if (showLockedText)
                lockedText.SubscribeGloc();
        }

        private void Update()
        {
            if (!isLocked) CurrentDynamic?.OnDynamicUpdate();
        }

        public void InteractStartPlayer(GameObject player)
        {
            if (isInteractLocked) return;
            PlayerManager playerManager = player.GetComponent<PlayerManager>();
            CurrentDynamic?.OnDynamicStart(playerManager);
        }

        public void InteractHold(Vector3 point)
        {
            Vector2 delta = InputManager.ReadInput<Vector2>(Controls.POINTER_DELTA);
            if (!isLocked) CurrentDynamic?.OnDynamicHold(delta);
        }

        public void InteractStop()
        {
            if (!isLocked) CurrentDynamic?.OnDynamicEnd();
        }

        /// <summary>
        /// Set dynamic object locked status.
        /// </summary>
        public void SetLockedStatus(bool locked)
        {
            isLocked = locked;
            if (!locked) isInteractLocked = false;
        }

        /// <summary>
        /// Set dynamic object open state.
        /// </summary>
        /// <remarks>
        /// The dynamic object opens as if you were interacting with it. If the dynamic interaction type is mouse, nothing happens.
        /// <br>This function is good for calling from an event.</br>
        /// </remarks>
        public void SetOpenState()
        {
            if (interactType == InteractType.Mouse || isLocked)
                return;

            CurrentDynamic?.OnDynamicOpen();
        }

        /// <summary>
        /// Set dynamic object close state.
        /// </summary>
        /// <remarks>
        /// The dynamic object opens as if you were interacting with it. If the dynamic interaction type is mouse, nothing happens.
        /// <br>This function is good for calling from an event.</br>
        /// </remarks>
        public void SetCloseState()
        {
            if (interactType == InteractType.Mouse || isLocked)
                return;

            CurrentDynamic?.OnDynamicClose();
        }

        /// <summary>
        /// Play Dynamic Object Sound.
        /// </summary>
        /// </param>
        public void PlaySound(DynamicSoundType soundType)
        {
            switch (soundType)
            {
                case DynamicSoundType.Open: GameTools.PlayOneShot3D(target.position, useSound1, "Open Sound"); break;
                case DynamicSoundType.Close: GameTools.PlayOneShot3D(target.position, useSound2, "Close Sound"); break;
                case DynamicSoundType.Locked: GameTools.PlayOneShot3D(target.position, lockedSound, "Locked Sound"); break;
                case DynamicSoundType.Unlock: GameTools.PlayOneShot3D(target.position, unlockSound, "Unlock Sound"); break;
            }
        }

        /// <summary>
        /// The result of using the custom unlock script function.
        /// <br>Call this function after using the OnTryUnlock() function.</br>
        /// </summary>
        public void TryUnlockResult(bool unlocked)
        {
            if (unlocked)
            {
                unlockedEvent?.Invoke();
                PlaySound(DynamicSoundType.Unlock);
            }
            else
            {
                lockedEvent?.Invoke();
                PlaySound(DynamicSoundType.Locked);
            }

            SetLockedStatus(!unlocked);
        }

        private void OnDrawGizmosSelected()
        {
            if(CurrentDynamic.ShowGizmos)
                CurrentDynamic?.OnDrawGizmos();
        }

        public StorableCollection OnSave()
        {
            StorableCollection saveableBuffer = new StorableCollection();

            switch (dynamicType)
            {
                case DynamicType.Openable:
                    saveableBuffer = openable.OnSave();
                    break;
                case DynamicType.Pullable:
                    saveableBuffer = pullable.OnSave();
                    break;
                case DynamicType.Switchable:
                    saveableBuffer = switchable.OnSave();
                    break;
                case DynamicType.Rotable:
                    saveableBuffer = rotable.OnSave();
                    break;
                default:
                    break;
            }

            saveableBuffer.Add("isLocked", isLocked);
            return saveableBuffer;
        }

        public void OnLoad(JToken data)
        {
            switch (dynamicType)
            {
                case DynamicType.Openable:
                    openable.OnLoad(data);
                    break;
                case DynamicType.Pullable:
                    pullable.OnLoad(data);
                    break;
                case DynamicType.Switchable:
                    switchable.OnLoad(data);
                    break;
                case DynamicType.Rotable:
                    rotable.OnLoad(data);
                    break;
            }

            isLocked = (bool)data["isLocked"];
        }
    }
}