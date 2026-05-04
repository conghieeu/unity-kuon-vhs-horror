using UnityEngine;
using UHFPS.Tools;
using UHFPS.Scriptable;
using Newtonsoft.Json.Linq;

namespace UHFPS.Runtime
{
    public abstract class PlayerItemBehaviour : MonoBehaviour, ISaveableCustom
    {
        private Animator animator;
        private PlayerManager playerManager;
        private PlayerStateMachine playerStateMachine;
        private LookController lookController;
        private ExamineController examineController;
        private MotionController motionController;
        private Vector3 wallHitVel;

        private Transform motionTransform;
        private Quaternion defaultMotionRot;
        private Vector3 defaultMotionPos;

        public bool EnableWallDetection = true;
        public bool EnableMotionPreset = true;
        public bool EnableExternalMotion = true;

        // wall detection
        public Transform WallHitTransform;
        public LayerMask WallHitMask;
        public float WallHitRayDistance = 0.5f;
        public float WallHitRayRadius = 0.3f;
        public float WallHitAmount = 1f;
        public float WallHitTime = 0.2f;
        public Vector3 WallHitRayOffset;
        public bool ShowRayGizmos = true;

        // item motion
        public MotionBlender MotionBlender = new();
        public MotionPreset MotionPreset;

        // external motions
        public ExternalMotions ExternalMotions = new();

        /// <summary>
        /// The pivot point of the item object that will be used for the motion preset effects.
        /// </summary>
        [field: SerializeField]
        public Transform MotionPivot { get; set; }

        /// <summary>
        /// The object of the item which will be enabled or disabled, usually a child object.
        /// </summary>
        [field: SerializeField]
        public GameObject ItemObject { get; set; }

        /// <summary>
        /// Root player transform.
        /// </summary>
        public Transform PlayerRoot => PlayerManager.transform;

        public Vector3 LookForward2D => LookController.LookForward2D;
        public Vector3 LookForward => LookController.LookForward;
        public Vector3 LookCross => LookController.LookCross;

        /// <summary>
        /// Ray going from main camera to forward.
        /// </summary>
        public Ray CameraRay
        {
            get
            {
                Transform mainCamera = PlayerManager.MainCamera.transform;
                return new Ray(mainCamera.position, mainCamera.forward);
            }
        }

        /// <summary>
        /// Animator component of the Item Object.
        /// </summary>
        public Animator Animator
        {
            get
            {
                if(animator == null)
                    animator = ItemObject.GetComponentInChildren<Animator>();

                return animator;
            }
        }

        /// <summary>
        /// PlayerManager component.
        /// </summary>
        public PlayerManager PlayerManager
        {
            get
            {
                if (playerManager == null)
                    playerManager = GetComponentInParent<PlayerManager>();

                return playerManager;
            }
        }

        /// <summary>
        /// PlayerStateMachine component.
        /// </summary>
        public PlayerStateMachine PlayerStateMachine
        {
            get
            {
                if (playerStateMachine == null)
                    playerStateMachine = PlayerManager.GetComponent<PlayerStateMachine>();

                return playerStateMachine;
            }
        }

        /// <summary>
        /// LookController component.
        /// </summary>
        public LookController LookController
        {
            get
            {
                if (lookController == null)
                    lookController = PlayerManager.GetComponentInChildren<LookController>();

                return lookController;
            }
        }

        /// <summary>
        /// ExamineController component.
        /// </summary>
        public ExamineController ExamineController
        {
            get
            {
                if (examineController == null)
                    examineController = PlayerManager.GetComponentInChildren<ExamineController>();

                return examineController;
            }
        }

        /// <summary>
        /// MotionController component.
        /// </summary>
        public MotionController CameraMotions
        {
            get
            {
                if (motionController == null)
                    motionController = PlayerManager.GetComponentInChildren<MotionController>();

                return motionController;
            }
        }

        /// <summary>
        /// PlayerItemsManager component.
        /// </summary>
        public PlayerItemsManager PlayerItems
        {
            get => PlayerManager.PlayerItems;
        }

        /// <summary>
        /// Check if the item is interactive. False, for example when the inventory is opened, object is dragged etc.
        /// </summary>
        public bool CanInteract => PlayerItems.CanInteract;

        /// <summary>
        /// The name of the item that will be displayed in the list.
        /// </summary>
        public virtual string Name => "Item";

        /// <summary>
        /// Check whether the item can be switched.
        /// </summary>
        public virtual bool IsBusy() => false;

        /// <summary>
        /// Check whether the item is equipped.
        /// </summary>
        public virtual bool IsEquipped() => ItemObject.activeSelf;

        /// <summary>
        /// Check whether the item can be combined in inventory.
        /// </summary>
        public virtual bool CanCombine() => false;

        public virtual void Reset()
        {
            if (WallHitTransform == null)
                WallHitTransform = transform;
        }

        public virtual void Start()
        {
            if (WallHitTransform == null)
                WallHitTransform = transform;

            if (EnableMotionPreset && MotionPreset != null)
            {
                motionTransform = MotionPivot != null ? MotionPivot : CameraMotions.HandsMotionTransform;
                defaultMotionRot = motionTransform.localRotation;
                defaultMotionPos = motionTransform.localPosition;
                MotionBlender.Init(MotionPreset, motionTransform, PlayerStateMachine);
            }

            if (EnableExternalMotion) 
                ExternalMotions.Init(CameraMotions);
        }

        public virtual void OnDestroy()
        {
            if (EnableMotionPreset && MotionPreset != null && MotionBlender != null)
                MotionBlender.Dispose();
        }

        private void Update()
        {
            if (IsEquipped())
            {
                if (EnableWallDetection)
                {
                    Vector3 forward = PlayerItems.transform.forward;
                    Vector3 origin = PlayerItems.transform.TransformPoint(WallHitRayOffset);

                    if (Physics.SphereCast(origin, WallHitRayRadius, forward, out RaycastHit hit, WallHitRayDistance, WallHitMask))
                        OnItemBlocked(hit.distance, true);
                    else
                        OnItemBlocked(0f, false);
                }

                if (EnableMotionPreset && MotionPreset != null && motionTransform != null)
                {
                    MotionBlender.BlendMotions(Time.deltaTime, out var position, out var rotation);
                    Vector3 newPosition = defaultMotionPos + position;
                    Quaternion newRotation = defaultMotionRot * rotation;
                    motionTransform.SetLocalPositionAndRotation(newPosition, newRotation);
                }
            }

            OnUpdate();
        }

        /// <summary>
        /// Apply an external camera motion effect, such as an wobble or impact.
        /// </summary>
        public void ApplyEffect(string eventID)
        {
            if (!EnableExternalMotion || ExternalMotions == null)
                return;

            ExternalMotions.ApplyEffect(eventID);
        }

        /// <summary>
        /// Will be called when the ray going from the camera hits the wall to prevent the player item from being clipped.
        /// </summary>
        public virtual void OnItemBlocked(float hitDistance, bool blocked)
        {
            float value = GameTools.Remap(0f, WallHitRayDistance, 0f, 1f, hitDistance);
            Vector3 backward = Vector3.back * WallHitAmount;
            Vector3 result = Vector3.Lerp(backward, Vector3.zero, blocked ? value : 1f);
            WallHitTransform.localPosition = Vector3.SmoothDamp(WallHitTransform.localPosition, result, ref wallHitVel, WallHitTime);
        }

        /// <summary>
        /// Will be called every frame like the classic Update() function. 
        /// </summary>
        public virtual void OnUpdate() { }

        /// <summary>
        /// Will be called when a combinable item is combined with this inventory item.
        /// </summary>
        public virtual void OnItemCombine(InventoryItem combineItem) { }

        /// <summary>
        /// Will be called when PlayerItemsManager selects an item.
        /// </summary>
        public abstract void OnItemSelect();

        /// <summary>
        /// Will be called when PlayerItemsManager deselects an item.
        /// </summary>
        public abstract void OnItemDeselect();

        /// <summary>
        /// Will be called when PlayerItemsManager activates an item.
        /// </summary>
        public abstract void OnItemActivate();

        /// <summary>
        /// Will be called when PlayerItemsManager deactivates an item.
        /// </summary>
        public abstract void OnItemDeactivate();

        public virtual void OnDrawGizmosSelected()
        {
            bool selected = false;

#if UNITY_EDITOR
            selected = UnityEditor.Selection.activeGameObject == gameObject;
#endif

            if (ShowRayGizmos && EnableWallDetection && selected)
            {
                Vector3 forward = PlayerItems.transform.forward;
                Vector3 origin = PlayerItems.transform.TransformPoint(WallHitRayOffset);
                Vector3 p2 = origin + forward * WallHitRayDistance;

                Gizmos.color = Color.yellow;
                GizmosE.DrawWireCapsule(origin, p2, WallHitRayRadius);
            }
        }

        public virtual StorableCollection OnCustomSave()
        {
            return new();
        }

        public virtual void OnCustomLoad(JToken data) { }
    }
}