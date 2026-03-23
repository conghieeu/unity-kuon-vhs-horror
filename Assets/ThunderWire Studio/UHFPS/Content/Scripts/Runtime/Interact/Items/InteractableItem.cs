using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Newtonsoft.Json.Linq;
using UHFPS.Tools;
using ThunderWire.Attributes;

namespace UHFPS.Runtime
{
    [Docs("https://docs.twgamesdev.com/uhfps/guides/interactions")]
    public class InteractableItem : SaveableBehaviour
    {
        [Serializable]
        public sealed class Hotspot
        {
            public Transform HotspotTransform;
            [Tooltip("If you want the hotspot to be shown, keep this value true.")]
            public bool Enabled = true;
            [Tooltip("If this option is enabled, the hotspot action will be called when you put back the examined item.")]
            public bool ResetHotspot = false;
            [Space] public UnityEvent HotspotAction;
        }

        public enum InteractableTypeEnum { GenericItem, InventoryItem, ExamineItem, InventoryExpand }
        public enum MessageTypeEnum { None, Hint, Alert }
        public enum ExamineTypeEnum { None, GenericObject, CustomObject }
        public enum ExamineRotateEnum { Static, Horizontal, Vertical, Both }
        public enum DisableTypeEnum { None, Deactivate, Destroy }

        public InteractableTypeEnum InteractableType = InteractableTypeEnum.GenericItem;
        public MessageTypeEnum MessageType = MessageTypeEnum.None;
        public ExamineTypeEnum ExamineType = ExamineTypeEnum.None;
        public ExamineRotateEnum ExamineRotate = ExamineRotateEnum.Static;
        public DisableTypeEnum DisableType = DisableTypeEnum.None;

        public ItemProperty PickupItem;
        public ItemCustomData ItemCustomData;

        public GString InteractTitle;
        public GString ExamineTitle;

        public GString PaperText;
        public GString HintMessage;

        public float MessageTime = 3f;

        public ushort Quantity = 1;
        public ushort SlotsToExpand = 1;
        public bool ExpandRows;

        public bool UseInventoryTitle = true;
        public bool ExamineInventoryTitle = true;
        public bool ShowExamineTitle = true;

        public bool ShowFloatingIcon = false;
        public bool TakeFromExamine = false;
        public bool AllowCursorExamine = false;
        public bool IsPaper = false;

        public bool AutoShortcut = false;
        public bool AutoEquip = false;

        public bool UseExamineZooming = true;
        public MinMax ExamineZoomLimits = new(0.3f, 0.4f);
        public float ExamineDistance = 0.4f;

        public bool UseFaceRotation;
        public Vector3 FaceRotation;

        public bool UseControlPoint;
        public Vector3 ControlPoint = new(0, 0.1f, 0);

        public List<Collider> CollidersEnable = new();
        public List<Collider> CollidersDisable = new();
        public Hotspot ExamineHotspot = new();

        public SoundClip PickupSound;
        public SoundClip ExamineSound;
        public SoundClip ExamineHintSound;

        public UnityEvent OnTakeEvent;
        public UnityEvent OnExamineStartEvent;
        public UnityEvent OnExamineEndEvent;

        public bool IsExamined;

        public bool IsCustomExamine => InteractableType != InteractableTypeEnum.GenericItem && ExamineType == ExamineTypeEnum.CustomObject;

        /// <summary>
        /// Name of the item.
        /// </summary>
        public string ItemName
        {
            get
            {
                string title = InteractTitle;

                if (InteractableType == InteractableTypeEnum.InventoryItem && UseInventoryTitle)
                {
                    title = PickupItem.GetItem().Title;
                }

                return title;
            }
        }

        private void Awake()
        {
            if(IsCustomExamine)
            {
                foreach (var col in CollidersEnable)
                {
                    col.enabled = false;
                }

                foreach (var col in CollidersDisable)
                {
                    col.enabled = true;
                }
            }
        }

        private void Start()
        {
            if (InteractableType != InteractableTypeEnum.InventoryItem || !UseInventoryTitle)
                InteractTitle.SubscribeGloc();

            if(ExamineType != ExamineTypeEnum.None)
                ExamineTitle.SubscribeGloc();

            if(ExamineType != ExamineTypeEnum.None && IsPaper)
                PaperText.SubscribeGloc();

            if(MessageType == MessageTypeEnum.Hint)
                HintMessage.SubscribeGloc();
        }

        public void OnInteract()
        {
            if (InteractableType == InteractableTypeEnum.ExamineItem)
                return;

            GameTools.PlayOneShot2D(transform.position, PickupSound, "PickupSound");
            OnTakeEvent?.Invoke();

            if (DisableType != DisableTypeEnum.None)
                EnabledState(false);
        }

        /// <summary>
        /// Use this method to interact with the object as if you were pressing the USE button on it.
        /// </summary>
        public void InteractWithObject()
        {
            if (InteractableType == InteractableTypeEnum.ExamineItem)
                return;

            PlayerPresenceManager.Instance.PlayerManager.InteractController.Interact(gameObject);
        }

        public void EnabledState(bool enabled)
        {
            if (!enabled && SaveGameManager.HasReference) 
                SaveGameManager.RemoveSaveable(gameObject);

            if(DisableType == DisableTypeEnum.Deactivate)
                gameObject.SetActive(enabled);
            else if(DisableType == DisableTypeEnum.Destroy && !enabled)
                Destroy(gameObject);
        }

        public override StorableCollection OnSave()
        {
            return new StorableCollection()
            {
                { "position", transform.localPosition.ToSaveable() },
                { "rotation", transform.localEulerAngles.ToSaveable() },
                { "quantity", Quantity },
                { "enabledState", gameObject.activeSelf },
                { "hotspotEnabled", ExamineHotspot.Enabled },
                { "customData", ItemCustomData.GetJson() }
            };
        }

        public override void OnLoad(JToken data)
        {
            transform.localPosition = data["position"].ToObject<Vector3>();
            transform.localEulerAngles = data["rotation"].ToObject<Vector3>();

            Quantity = (ushort)data["quantity"];
            EnabledState((bool)data["enabledState"]);
            ExamineHotspot.Enabled = (bool)data["hotspotEnabled"];

            ItemCustomData.JsonData = data["customData"].ToString();
        }
    }
}