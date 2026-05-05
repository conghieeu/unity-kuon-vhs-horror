using UnityEngine;
using UnityEngine.UI;
using ThunderWire.Attributes;

namespace UHFPS.Runtime
{
    [Summary("Thành phần đại diện cho một ô trống (Slot) trong lưới (Grid) của túi đồ. Mỗi InventoryItem sẽ chiếm một hoặc nhiều InventorySlot.")]
    public class InventorySlot : MonoBehaviour
    {
        [Tooltip("Hình ảnh khung viền của ô.")]
        public Image frame;

        [Tooltip("Vật phẩm hiện đang chiếm dụng ô này (null nếu ô trống).")]
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