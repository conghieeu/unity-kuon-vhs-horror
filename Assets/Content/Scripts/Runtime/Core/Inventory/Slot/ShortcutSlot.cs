using UnityEngine;
using UnityEngine.UI;
using ThunderWire.Attributes;
using TMPro;

namespace UHFPS.Runtime
{
    [InspectorHeader("Shortcut Slot")]
    [Summary("Quản lý giao diện và dữ liệu của một ô phím tắt (Shortcut) trên màn hình HUD.")]
    public class ShortcutSlot : MonoBehaviour
    {
        [Tooltip("Kích thước tối đa của khung chứa icon vật phẩm.")]
        public Vector2 ItemRectSize;

        [Header("Panels")]
        [Tooltip("GameObject cha chứa icon vật phẩm.")]
        public GameObject ItemPanel;

        [Tooltip("GameObject cha chứa Text hiển thị số lượng vật phẩm.")]
        public GameObject QuantityPanel;

        [Header("References")]
        [Tooltip("Component Image dùng để render icon của vật phẩm.")]
        public Image ItemIcon;

        [Tooltip("Component Image làm màu nền cho ô phím tắt.")]
        public Image Background;

        [Header("Settings")]
        [Tooltip("Có hiển thị số lượng vật phẩm hay không.")]
        public bool ShowQuantity;

        [Header("Slot Colors")]
        [Tooltip("Màu nền khi ô phím tắt trống.")]
        public Color EmptySlotColor;

        [Tooltip("Màu nền khi có vật phẩm được gán vào ô phím tắt.")]
        public Color NormalSlotColor;

        private InventoryItem inventoryItem;
        private Inventory inventory;
        private TMP_Text quantity;

        private void Awake()
        {
            quantity = QuantityPanel.GetComponentInChildren<TMP_Text>();
        }

        public void SetItem(InventoryItem inventoryItem)
        {
            this.inventoryItem = inventoryItem;

            if(inventoryItem != null)
            {
                inventory = inventoryItem.Inventory;
                Item item = inventoryItem.Item;

                // icon orientation and scaling
                Vector2 slotSize = ItemRectSize;
                Vector2 iconSize = item.Icon.rect.size;

                Vector2 scaleRatio = slotSize / iconSize;
                float scaleFactor = Mathf.Min(scaleRatio.x, scaleRatio.y);

                ItemIcon.sprite = item.Icon;
                ItemIcon.rectTransform.sizeDelta = iconSize * scaleFactor;

                Background.color = NormalSlotColor;
                ItemPanel.SetActive(true);
            }
            else
            {
                ItemIcon.sprite = null;
                quantity.text = string.Empty;
                QuantityPanel.SetActive(false);

                Background.color = EmptySlotColor;
                ItemPanel.SetActive(false);
            }
        }

        private void Update()
        {
            UpdateItemQuantity();
        }

        private void UpdateItemQuantity()
        {
            if (inventoryItem == null)
                return;

            int itemQuantity = inventoryItem.Quantity;

            if (!inventoryItem.Item.Settings.alwaysShowQuantity)
            {
                if (itemQuantity > 1)
                    quantity.text = inventoryItem.Quantity.ToString();
                else
                {
                    QuantityPanel.SetActive(false);
                    quantity.text = string.Empty;
                }
            }
            else
            {
                QuantityPanel.SetActive(true);
                quantity.text = itemQuantity.ToString();
                quantity.color = itemQuantity >= 1
                    ? inventory.slotSettings.normalQuantityColor
                    : inventory.slotSettings.zeroQuantityColor;
            }
        }
    }
}