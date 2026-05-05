using UnityEngine;
using UHFPS.Tools;
using UnityEngine.Events;

using ThunderWire.Attributes;

namespace UHFPS.Runtime
{
    [Summary("Thùng/Rương chứa đồ (Kế thừa InventoryContainer). Hỗ trợ tương tác trực tiếp mở lên giao diện Rương đồ, có thêm âm thanh và hoạt ảnh nắp rương.")]
    public class ItemsContainer : InventoryContainer, IInteractStart
    {
        [Tooltip("Animator điều khiển việc đóng/mở vật chứa (VD: Mở nắp rương).")]
        public Animator Animator;

        [Tooltip("Tên tham số Boolean trong Animator để mở nắp rương.")]
        public string OpenParameter = "Open";

        [Tooltip("Âm thanh phát ra khi mở rương.")]
        public SoundClip OpenSound;

        [Tooltip("Âm thanh phát ra khi đóng rương.")]
        public SoundClip CloseSound;

        [Tooltip("Chỉ phát âm thanh đóng rương khi dùng kèm Animator.")]
        public bool CloseWithAnimation;

        [Tooltip("Sự kiện gọi ra khi rương được mở.")]
        public UnityEvent OnOpenContainer;

        [Tooltip("Sự kiện gọi ra khi rương bị đóng lại.")]
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