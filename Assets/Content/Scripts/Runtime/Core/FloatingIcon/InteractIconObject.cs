using UHFPS.Tools;
using UnityEngine;

using ThunderWire.Attributes;

namespace UHFPS.Runtime
{
    [Summary("Component gắn vào vật thể để hiển thị Icon tương tác khi người chơi nhìn (Hover) hoặc giữ (Hold) vật thể.")]
    public class InteractIconObject : MonoBehaviour, IHoverStart, IHoverEnd, IInteractStart, IInteractStop
    {
        [Tooltip("Icon hiển thị khi người chơi nhìn vào vật thể.")]
        public Sprite HoverIcon;

        [Tooltip("Kích thước của Hover Icon.")]
        public Vector2 HoverSize;

        [Tooltip("Icon hiển thị khi người chơi đang giữ (Hold) tương tác.")]
        public Sprite HoldIcon;

        [Tooltip("Kích thước của Hold Icon.")]
        public Vector2 HoldSize;

        [Tooltip("Độ lệch vị trí của Icon so với tâm vật thể.")]
        public Vector3 IconOffset;

        private InteractIconModule module;
        private bool isHover;
        private bool isHovering;
        private bool isHolding;

        public Vector3 IconPosition => transform.TransformPoint(IconOffset);

        private void Start()
        {
            module = GameManager.Module<InteractIconModule>();
            if (module == null) throw new System.NullReferenceException("InteractIconModule not found in GameManager!");
        }

        public void HoverStart()
        {
            isHovering = true;

            if (isHover || isHolding)
                return;

            module?.ShowInteractIcon(this);
            isHover = true;
        }

        public void HoverEnd()
        {
            isHovering = false;

            if (!isHover || isHolding)
                return;

            module?.DestroyInteractIcon(this);
            isHover = false;
        }

        public void InteractStart()
        {
            if (!isHover)
                return;

            module?.SetIconToHold(this);
            isHolding = true;
        }

        public void InteractStop()
        {
            if (!isHover)
                return;

            if (!isHovering)
            {
                module?.DestroyInteractIcon(this);
                isHover = false;
            }
            else module?.SetIconToHover(this);

            isHolding = false;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.green.Alpha(0.5f);
            Gizmos.DrawSphere(IconPosition, 0.025f);
        }
    }
}