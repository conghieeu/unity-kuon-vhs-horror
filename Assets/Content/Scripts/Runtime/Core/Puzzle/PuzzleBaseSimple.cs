using UnityEngine;

using ThunderWire.Attributes;

namespace UHFPS.Runtime
{
    [Summary("Class cơ sở cho các Puzzle tương tác đơn giản, không cần chuyển camera (Ví dụ: Công tắc, Nút bấm ngoài màn hình chính).")]
    public abstract class PuzzleBaseSimple : MonoBehaviour, IInteractStart
    {
        [Tooltip("Layer được gán vào Collider sau khi giải xong Puzzle để chặn người chơi tương tác lại (VD: Ignore Raycast).")]
        public Layer DisabledLayer;

        public virtual void InteractStart() { }

        /// <summary>
        /// Disable the puzzle interaction functionality. The GameObject layer will be set to Disabled Layer.
        /// </summary>
        protected void DisableInteract(bool includeChild = true)
        {
            gameObject.layer = DisabledLayer;

            if (includeChild)
            {
                foreach (Transform tr in transform)
                {
                    tr.gameObject.layer = DisabledLayer;
                }
            }
        }
    }
}