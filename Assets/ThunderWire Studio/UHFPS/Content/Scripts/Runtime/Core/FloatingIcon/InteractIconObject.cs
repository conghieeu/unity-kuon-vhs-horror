using UHFPS.Tools;
using UnityEngine;

namespace UHFPS.Runtime
{
    public class InteractIconObject : MonoBehaviour, IHoverStart, IHoverEnd, IInteractStart, IInteractStop
    {
        public Sprite HoverIcon;
        public Vector2 HoverSize;

        public Sprite HoldIcon;
        public Vector2 HoldSize;

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