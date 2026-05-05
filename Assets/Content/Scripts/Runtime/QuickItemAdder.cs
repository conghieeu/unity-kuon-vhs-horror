using UnityEngine;
using UHFPS.Runtime;
using ThunderWire.Attributes;
using QFSW.QC;

namespace Kuon
{
    [Summary("Tiện ích hỗ trợ thêm nhanh vật phẩm (Item) vào kho đồ (Inventory) và tùy chọn cầm lên tay.")]
    public class QuickItemAdder : MonoBehaviour
    {
        [Header("Item Settings")]
        [Tooltip("Kéo thả trực tiếp Prefab hoặc Object chứa Component InteractableItem vào đây (Ví dụ: Camcorder)")]
        public InteractableItem ItemPrefab;
        
        [Tooltip("Số lượng item muốn thêm. Nếu để là 0 sẽ tự lấy số lượng từ cấu hình phần item kia")]
        public ushort QuantityOverride = 0;

        [Tooltip("Slot Shortcut muốn ép vào (1, 2, 3, 4). Ví dụ slot 1 là nút số 1.")]
        [Range(1, 4)]
        public int TargetShortcutSlot = 1;
        
        [Tooltip("Nếu tích vào, Item sẽ tự động cầm luôn trên tay character")]
        public bool AutoEquipOnHands = true;

        void Start()
        {
            // AddAndEquipItem();
        }

        /// <summary>
        /// Hàm này có thể được gọi từ Unity Events hoặc tự động chạy lúc Start()
        /// </summary>
        [Command("additem")]
        public void AddAndEquipItem()
        {
            if (ItemPrefab == null)
            {
                Debug.LogWarning("[QuickItemAdder] Bạn chưa kéo thả một InteractableItem Prefab vào nhé!");
                return;
            }

            if (ItemPrefab.InteractableType != InteractableItem.InteractableTypeEnum.InventoryItem)
            {
                Debug.LogWarning("[QuickItemAdder] Object bạn kéo vào Inspector không phải dạng Inventory Item.");
                return;
            }

            if (Inventory.Instance != null && ItemPrefab.PickupItem != null)
            {
                string guid = ItemPrefab.PickupItem.GUID;
                ushort quantity = QuantityOverride > 0 ? QuantityOverride : ItemPrefab.Quantity;

                // Gọi API của UHFPS để thêm vào túi đồ dựa trên GUID của InteractableItem đo
                if (Inventory.Instance.AddItem(guid, quantity, ItemPrefab.ItemCustomData, out InventoryItem addedItem))
                {
                    // Tự động gán vào Slot được chỉ định, tham số true = ép đè nếu slot đã có đồ, -1 vì mảng đếm từ 0
                    Inventory.Instance.AutoShortcut(addedItem, true, TargetShortcutSlot - 1);

                    // Lấy ra cầm
                    if (AutoEquipOnHands)
                    {
                        Inventory.Instance.UseItem(addedItem, false);
                    }
                }
            }
            else
            {
                Debug.LogError("[QuickItemAdder] Lỗi không tìm thấy Inventory Instance hoặc Item bị rỗng.");
            }
        }
    }
}
