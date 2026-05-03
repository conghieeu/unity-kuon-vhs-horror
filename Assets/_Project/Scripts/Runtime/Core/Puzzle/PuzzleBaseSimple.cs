using UnityEngine;

namespace UHFPS.Runtime
{
    public abstract class PuzzleBaseSimple : MonoBehaviour, IInteractStart
    {
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