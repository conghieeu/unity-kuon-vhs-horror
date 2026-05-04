using UnityEngine;
using UHFPS.Tools;
using UnityEngine.Events;

namespace UHFPS.Runtime
{
    public class ItemsContainer : InventoryContainer, IInteractStart
    {
        public Animator Animator;
        public string OpenParameter = "Open";

        public SoundClip OpenSound;
        public SoundClip CloseSound;
        public bool CloseWithAnimation;

        public UnityEvent OnOpenContainer;
        public UnityEvent OnCloseContainer;

        public void InteractStart()
        {
            inventory.OpenContainer(this);
            OnOpenContainer?.Invoke();

            GameTools.PlayOneShot3D(transform.position, OpenSound);
            if (Animator != null) Animator.SetBool(OpenParameter, true);
        }

        public override void OnStorageClose()
        {
            if (CloseWithAnimation) GameTools.PlayOneShot3D(transform.position, CloseSound);
            if (Animator != null) Animator.SetBool(OpenParameter, false);
            OnCloseContainer?.Invoke();
        }

        public void PlayCloseSound()
        {
            GameTools.PlayOneShot3D(transform.position, CloseSound);
        }
    }
}