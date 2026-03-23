using UnityEngine;
using UnityEngine.UI;

namespace UHFPS.Runtime
{
    public class InventorySlot : MonoBehaviour
    {
        public Image frame;
        public InventoryItem itemInSlot;

        private CanvasGroup canvasGroup;
        public CanvasGroup CanvasGroup
        {
            get
            {
                if(canvasGroup == null)
                    canvasGroup = GetComponent<CanvasGroup>();

                return canvasGroup;
            }
        }
    }
}