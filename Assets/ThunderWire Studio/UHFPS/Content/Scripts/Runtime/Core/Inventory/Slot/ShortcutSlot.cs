using UnityEngine;
using UnityEngine.UI;
using ThunderWire.Attributes;
using TMPro;

namespace UHFPS.Runtime
{
    [InspectorHeader("Shortcut Slot")]
    public class ShortcutSlot : MonoBehaviour
    {
        public Vector2 ItemRectSize;

        [Header("Panels")]
        public GameObject ItemPanel;
        public GameObject QuantityPanel;

        [Header("References")]
        public Image ItemIcon;
        public Image Background;

        [Header("Settings")]
        public bool ShowQuantity;

        [Header("Slot Colors")]
        public Color EmptySlotColor;
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