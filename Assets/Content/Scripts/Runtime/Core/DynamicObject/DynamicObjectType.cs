using System;
using UnityEngine;
using Newtonsoft.Json.Linq;

namespace UHFPS.Runtime
{
    [Serializable]
    public abstract class DynamicObjectType
    {
        [field: SerializeField]
        public DynamicObject DynamicObject { get; internal set; }

        public bool IsHolding { get; protected set; }

        public virtual bool IsOpened { get; } = false;

        protected DynamicObject.TransformType TransformType => DynamicObject.transformType;
        protected DynamicObject.DynamicStatus DynamicStatus => DynamicObject.dynamicStatus;
        protected DynamicObject.InteractType InteractType => DynamicObject.interactType;
        protected DynamicObject.StatusChange StatusChange => DynamicObject.statusChange;

        protected Transform Target => DynamicObject.target;
        protected Animator Animator => DynamicObject.animator;
        protected HingeJoint Joint => DynamicObject.joint;
        protected Rigidbody Rigidbody => DynamicObject.rigidbody;
        protected AudioSource AudioSource => DynamicObject.audioSource;

        /// <summary>
        /// Override this parameter to set whether you want to show or hide gizmos on the dynamic object.
        /// </summary>
        public virtual bool ShowGizmos { get; } = false;

        /// <summary>
        /// Override this method to set custom parameters at Awake.
        /// </summary>
        public virtual void OnDynamicInit() { }

        /// <summary>
        /// Override this method to set custom parameters when you interact with the dynamic object.
        /// </summary>
        public virtual void OnDynamicStart(PlayerManager player) { }

        /// <summary>
        /// Override this method to set the behavior when a open event is called.
        /// </summary>
        public virtual void OnDynamicOpen() { }

        /// <summary>
        /// Override this method to set the behavior when a close event is called.
        /// </summary>
        public virtual void OnDynamicClose() { }

        /// <summary>
        /// Override this method to define custom actions when the dynamic object is locked.
        /// </summary>
        public virtual void OnDynamicLocked() { }

        /// <summary>
        /// Override this method to define your own behavior at Update.
        /// </summary>
        public virtual void OnDynamicUpdate() { }

        /// <summary>
        /// Override this method to define a custom behavior when you hold the interact button on a dynamic object.
        /// </summary>
        public virtual void OnDynamicHold(Vector2 mouseDelta) { }

        /// <summary>
        /// Override this method to clear the parameters.
        /// </summary>
        public virtual void OnDynamicEnd() { }

        /// <summary>
        /// Override this method if you want to define your own gizmos drawing.
        /// </summary>
        public virtual void OnDrawGizmos() { }

        /// <summary>
        /// Try to unlock the dynamic object.
        /// </summary>
        public virtual void TryUnlock()
        {
            if (StatusChange == DynamicObject.StatusChange.InventoryItem)
            {
                if (DynamicObject.unlockItem.InInventory)
                {
                    if (!DynamicObject.keepUnlockItem)
                        DynamicObject.inventory.RemoveItem(DynamicObject.unlockItem);

                    DynamicObject.SetLockedStatus(false);
                    DynamicObject.unlockedEvent?.Invoke();
                    DynamicObject.PlaySound(DynamicSoundType.Unlock);
                }
                else
                {
                    DynamicObject.lockedEvent?.Invoke();
                    DynamicObject.PlaySound(DynamicSoundType.Locked);
                    OnDynamicLocked();

                    if (DynamicObject.showLockedText)
                        DynamicObject.gameManager.ShowHintMessage(DynamicObject.lockedText, 3f);
                }
            }
            else if (StatusChange == DynamicObject.StatusChange.CustomScript && DynamicObject.unlockScript != null)
            {
                IDynamicUnlock dynamicUnlock = (IDynamicUnlock)DynamicObject.unlockScript;
                dynamicUnlock.OnTryUnlock(DynamicObject);
            }
            else
            {
                DynamicObject.lockedEvent?.Invoke();
                DynamicObject.PlaySound(DynamicSoundType.Locked);
                OnDynamicLocked();

                if (DynamicObject.showLockedText)
                    DynamicObject.gameManager.ShowHintMessage(DynamicObject.lockedText, 3f);
            }
        }

        /// <summary>
        /// This method collects the data that is to be saved.
        /// </summary>
        public abstract StorableCollection OnSave();

        /// <summary>
        /// This method is called when the loading process is executed.
        /// </summary>
        public abstract void OnLoad(JToken token);
    }
}