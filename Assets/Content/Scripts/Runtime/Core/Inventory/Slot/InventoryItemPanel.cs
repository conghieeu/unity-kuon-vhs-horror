using UnityEngine;
using TMPro;
using ThunderWire.Attributes;

namespace UHFPS.Runtime
{
    [Summary("Panel hiển thị số lượng (Quantity) của vật phẩm trong túi đồ.")]
    public class InventoryItemPanel : MonoBehaviour
    {
        [Tooltip("Text hiển thị số lượng của vật phẩm hiện tại.")]
        public TMP_Text quantity;
    }
}