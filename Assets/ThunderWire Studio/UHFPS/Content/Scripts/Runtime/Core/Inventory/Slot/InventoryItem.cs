using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UHFPS.Input;
using UHFPS.Tools;
using ThunderWire.Attributes;

namespace UHFPS.Runtime
{
    public class InventoryItem : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        public struct ItemData
        {
            public string guid;
            public Item item;
            public int quantity;
            public Orientation orientation;
            public ItemCustomData customData;
            public Vector2Int slotSpace;
        }

        public enum HoverType { Normal, Hover, Move, Error }

        public Orientation orientation;
        public Image itemImage;
        public Image background;
        [Space]
        public InventoryItemPanel horizontalPanel;
        public InventoryItemPanel verticalPanel;
        public InventoryItemPanel activePanel;

        [Header("Debug")]
        [ReadOnly] public Vector2Int currentSlot;
        [ReadOnly] public Vector2Int lastSlot;

        public Inventory Inventory { get; set; }
        public string ContainerGuid { get; set; }
        public string ItemGuid { get; set; }
        public Item Item { get; set; }
        public int Quantity { get; set; }
        public ItemCustomData CustomData { get; set; }
        public Vector2Int Position => lastSlot;

        [HideInInspector] public bool isOver;
        [HideInInspector] public bool isMoving;
        [HideInInspector] public bool isRotating;
        [HideInInspector] public bool isContainerItem;

        private RectTransform rectTransform;
        private Color currentColor;

        private Orientation lastOrientation;

        private Vector2 dragOffset;
        private Vector2 dragVelocity;

        private Vector2 mousePosition;
        private Vector2 mouseDelta;

        private float targetRotation;
        private float itemRotation;
        private float rotationVelocity;

        private bool isCombining;
        private bool isCombinable;
        private bool isInitialized;

        /// <summary>
        /// [Developer Notes]
        /// Item positions were incorrectly positioned when the inventory was first opened, causing unexpected behavior,
        /// because the GridLayoutGroup doesn't update the slot positions when the panel is disabled, but I found a dumb
        /// way to fix it by setting the item position at the end of the frame.
        /// </summary>
        private IEnumerator Start()
        {
            yield return new WaitForEndOfFrame();

            rectTransform.anchoredPosition = GetItemPosition(lastSlot);
            isInitialized = true;
        }

        public void ContainerOpened(ushort columns)
        {
            currentSlot.x += columns;
            lastSlot.x += columns;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!CheckAnyItemMoving() && (!isCombining || isCombinable))
            {
                isOver = true;
                Inventory.ShowItemInfo(Item, CustomData);

                foreach (var item in Inventory.carryingItems)
                {
                    if (item.Key != this) item.Key.isOver = false;
                }

                if (Inventory.ContainerOpened)
                {
                    foreach (var item in Inventory.containerItems)
                    {
                        if (item.Key != this) item.Key.isOver = false;
                    }
                }
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            /*
            if (!CheckAnyItemMoving() && (!isCombining || isCombinable))
            {
                isOver = false;
            }
            */
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (isOver && !CheckAnyItemMoving() && (!isCombining || isCombinable))
            {
                if(!isCombinable) Inventory.ShowContextMenu(true, this);
                else Inventory.CombineWith(this);
            }
        }

        /// <summary>
        /// Set and initialize inventory item.
        /// </summary>
        public void SetItem(Inventory inventory, ItemData itemData)
        {
            Inventory = inventory;
            ItemGuid = itemData.guid;
            Item = itemData.item;
            Quantity = itemData.quantity;
            CustomData = itemData.customData;
            orientation = itemData.orientation;

            lastOrientation = orientation;
            currentSlot = itemData.slotSpace;
            lastSlot = itemData.slotSpace;

            Sprite icon = Item.Icon != null ? Item.Icon : inventory.slotSettings.missingItemSprite;
            itemImage.sprite = icon;

            rectTransform = GetComponent<RectTransform>();

            // icon orientation and scaling
            Vector2 slotSize = rectTransform.rect.size;
            slotSize -= new Vector2(20, 20);
            Vector2 iconSize = icon.rect.size;

            Vector2 newIconSize = Item.Orientation == ImageOrientation.Normal
                ?  iconSize : new Vector2(iconSize.y, iconSize.x);

            Vector2 scaleRatio = slotSize / newIconSize;
            float scaleFactor = Mathf.Min(scaleRatio.x, scaleRatio.y);

            // icon flipping
            if (Item.Orientation == ImageOrientation.Flipped)
            {
                float flipDirection = Item.FlipDirection == FlipDirection.Left ? 90 : -90;
                itemImage.rectTransform.localEulerAngles = new Vector3(0, 0, flipDirection);
            }

            itemImage.rectTransform.sizeDelta = iconSize * scaleFactor;

            ShowOrientationPanel();
            background.color = currentColor = inventory.slotSettings.itemNormalColor;
        }

        /// <summary>
        /// Set the combinability status of the item.
        /// </summary>
        public void SetCombinable(bool combine, bool combinable)
        {
            isCombining = combine;
            isCombinable = combinable;

            Color color = itemImage.color;
            color.a = !isCombining || isCombinable ? 1f : 0.3f;
            itemImage.color = color;
            isOver = false;
        }

        /// <summary>
        /// Set item quantity.
        /// </summary>
        public void SetQuantity(int quantity)
        {
            Quantity = quantity;
            UpdateItemQuantity();
        }

        /// <summary>
        /// Event when inventory closes.
        /// </summary>
        public void OnCloseInventory()
        {
            if(lastOrientation != orientation)
            {
                orientation = lastOrientation;
                float lastRotation = 0;

                if (lastOrientation == Orientation.Vertical)
                    lastRotation = -90;

                Vector3 angles = transform.localEulerAngles;
                angles.z = lastRotation;
                transform.eulerAngles = angles;
            }

            if (lastSlot != currentSlot)
            {
                currentSlot = lastSlot;
                rectTransform.position = GetItemPosition(lastSlot);
            }

            background.color = Inventory.slotSettings.itemNormalColor;
            SetCombinable(false, false);
            ShowOrientationPanel();

            isOver = false;
            isMoving = false;
            isRotating = false;
        }

        /// <summary>
        /// Get item dimensions from current orientation.
        /// </summary>
        public Vector2Int GetItemDimensions()
        {
            return orientation == Orientation.Vertical
                ? new Vector2Int(Item.Height, Item.Width)
                : new Vector2Int(Item.Width, Item.Height);
        }

        private void Update()
        {
            if (Inventory != null)
            {
                // background color changing
                if (isOver && !isMoving) UpdateBackground(HoverType.Hover);
                else if (isMoving) UpdateBackground(HoverType.Move);
                else UpdateBackground(HoverType.Normal);

                // item slot selection
                if (isMoving && !isRotating && mouseDelta.magnitude > 0)
                {
                    Vector2Int dimensions = GetItemDimensions();
                    Vector2 dragPos = mousePosition - dragOffset;
                    Vector2Int newSlotPosition = currentSlot;
                    float distance = Mathf.Infinity;

                    for (int y = 0; y < Inventory.MaxSlotXY.y; y++)
                    {
                        for (int x = 0; x < Inventory.MaxSlotXY.x; x++)
                        {
                            InventorySlot slot = Inventory[y, x];
                            if (slot == null) continue;

                            Vector2 position = new Vector2(slot.transform.position.x, slot.transform.position.y);
                            float slotDistance = Vector2.Distance(dragPos, position);

                            if (slotDistance < distance)
                            {
                                if (!Inventory.IsCoordsValid(x, y, dimensions.x, dimensions.y))
                                    continue;

                                newSlotPosition = new Vector2Int(x, y);
                                distance = slotDistance;
                            }
                        }
                    }

                    if (currentSlot != newSlotPosition)
                        Inventory.PlayInventorySound(InventorySound.ItemMove);

                    currentSlot = newSlotPosition;
                }

                if (!isRotating && isInitialized)
                {
                    // item movement
                    Vector2 position = rectTransform.localPosition;
                    Vector2 slotPosition = GetItemPosition(currentSlot);

                    position = Vector2.SmoothDamp(position, slotPosition, ref dragVelocity, Inventory.settings.dragTime);
                    rectTransform.localPosition = position;
                }
                else if(isRotating)
                {
                    // item rotation
                    if (Mathf.Abs(itemRotation - targetRotation) > 1f)
                    {
                        itemRotation = Mathf.SmoothDamp(itemRotation, targetRotation, ref rotationVelocity, Inventory.settings.rotateTime);
                    }
                    else
                    {
                        itemRotation = targetRotation;
                        rotationVelocity = 0f;
                        ShowOrientationPanel();
                        isRotating = false;
                    }

                    Vector3 angles = transform.localEulerAngles;
                    angles.z = itemRotation;
                    transform.eulerAngles = angles;
                }

                // inventory inputs
                GetInput();
            }
        }

        private Vector2 WorldToLocalPosition(RectTransform sourceRectTransform, RectTransform targetRectTransform)
        {
            Vector3 worldPosition = sourceRectTransform.position;
            Vector3 targetLocalPosition = targetRectTransform.InverseTransformPoint(worldPosition);
            return new Vector2(targetLocalPosition.x, targetLocalPosition.y);
        }

        private Vector2 GetItemPosition(Vector2Int coords)
        {
            RectTransform slot = Inventory[coords.y, coords.x].GetComponent<RectTransform>();
            RectTransform parent = transform.parent.GetComponent<RectTransform>();

            Vector2 slotPos = WorldToLocalPosition(slot, parent);
            Vector2 offset = GetOrientationOffset();
            return slotPos + offset;
        }

        public Vector2 GetOrientationOffset()
        {
            if (rectTransform)
            {
                Vector2 sizeDelta = rectTransform.sizeDelta;

                if (orientation == Orientation.Horizontal)
                {
                    return new Vector2(sizeDelta.x / 2f, -sizeDelta.y / 2f);
                }
                else
                {
                    return new Vector2(sizeDelta.y / 2f, -sizeDelta.x / 2f);
                }
            }

            return Vector2.zero;
        }

        private void UpdateBackground(HoverType hoverType)
        {
            switch (hoverType)
            {
                case HoverType.Normal:
                    currentColor = Inventory.slotSettings.itemNormalColor;
                    break;
                case HoverType.Hover:
                    currentColor = Inventory.slotSettings.itemHoverColor;
                    break;
                case HoverType.Move:
                    currentColor = Inventory.slotSettings.itemMoveColor;
                    break;
                case HoverType.Error:
                    background.color = Inventory.slotSettings.itemErrorColor;
                    break;
            }

            float defaultAlpha = currentColor.a;

            if (hoverType == HoverType.Hover)
                currentColor.a = GameTools.PingPong(0.3f, 1f);
            else currentColor.a = defaultAlpha;

            background.color = Color.Lerp(background.color, currentColor, Time.deltaTime * Inventory.slotSettings.colorChangeSpeed);
        }

        private void UpdateItemQuantity()
        {
            if(activePanel != null)
            {
                if (!Item.Settings.alwaysShowQuantity)
                {
                    if (Quantity > 1)
                    {
                        activePanel.gameObject.SetActive(true);
                        horizontalPanel.quantity.text = Quantity.ToString();
                        verticalPanel.quantity.text = Quantity.ToString();
                    }
                    else activePanel.gameObject.SetActive(false);
                }
                else
                {
                    horizontalPanel.quantity.text = Quantity.ToString();
                    verticalPanel.quantity.text = Quantity.ToString();
                    activePanel.quantity.color = Quantity >= 1
                        ? Inventory.slotSettings.normalQuantityColor
                        : Inventory.slotSettings.zeroQuantityColor;

                    if (activePanel != horizontalPanel)
                        horizontalPanel.quantity.color = Inventory.slotSettings.normalQuantityColor;
                    else if (activePanel != verticalPanel)
                        verticalPanel.quantity.color = Inventory.slotSettings.normalQuantityColor;
                }
            }
        }

        private void ShowOrientationPanel()
        {
            if (activePanel != null) activePanel.gameObject.SetActive(false);

            if (orientation == Orientation.Horizontal)
            {
                horizontalPanel.gameObject.SetActive(true);
                activePanel = horizontalPanel;
            }
            else
            {
                verticalPanel.gameObject.SetActive(true);
                activePanel = verticalPanel;
            }

            UpdateItemQuantity();
        }

        private void GetInput()
        {
            if (isOver || isMoving)
            {
                // get mouse position and mouse delta
                mousePosition = InputManager.ReadInput<Vector2>(Controls.POINTER);
                mouseDelta = InputManager.ReadInput<Vector2>(Controls.POINTER_DELTA);

                // item movement input
                if (InputManager.ReadButtonOnce(GetInstanceID(), Controls.INVENTORY_ITEM_MOVE))
                {
                    if (!isMoving)
                    {
                        if(Inventory.ContainerOpened)
                            transform.SetParent(Inventory.inventoryContainers);

                        Inventory.ShowContextMenu(false);
                        Inventory.PlayInventorySound(InventorySound.ItemSelect);
                        dragOffset = GetOrientationOffset();
                        isMoving = true;
                    }
                    else if (CheckPutSpace())
                    {
                        Inventory.PlayInventorySound(InventorySound.ItemPut);
                        Inventory.MoveItem(lastSlot, currentSlot, this);
                        lastOrientation = orientation;
                        lastSlot = currentSlot;
                        isMoving = false;

                        if (!(isOver = IsPointerOverItem()))
                            Inventory.HideItemInfo();
                    }
                    else
                    {
                        Inventory.PlayInventorySound(InventorySound.ItemError);
                        UpdateBackground(HoverType.Error);
                    }

                    transform.SetAsLastSibling();
                }
            }

            // item rotation input
            if (InputManager.ReadButtonOnce(GetInstanceID(), Controls.INVENTORY_ITEM_ROTATE) && isMoving && !isRotating && Item.Width != Item.Height)
            {
                Inventory.PlayInventorySound(InventorySound.ItemMove);

                if (orientation == Orientation.Horizontal)
                {
                    targetRotation = -90;
                    orientation = Orientation.Vertical;
                }
                else
                {
                    targetRotation = 0;
                    orientation = Orientation.Horizontal;
                }

                dragOffset = GetOrientationOffset();
                activePanel.gameObject.SetActive(false);
                isRotating = true;
            }
        }

        private bool CheckPutSpace()
        {
            Vector2Int dimensions = GetItemDimensions();
            return Inventory.CheckSpaceFromPosition(currentSlot.x, currentSlot.y, dimensions.x, dimensions.y, this);
        }

        private bool CheckAnyItemMoving()
        {
            if(Inventory != null)
            {
                foreach (var item in Inventory.carryingItems)
                {
                    if (item.Key.isMoving) return true;
                }

                if (Inventory.ContainerOpened)
                {
                    foreach (var item in Inventory.containerItems)
                    {
                        if (item.Key.isMoving) return true;
                    }
                }
            }

            return false;
        }

        private bool IsPointerOverItem()
        {
            EventSystem eventSystem = EventSystem.current;
            PointerEventData eventDataCurrentPosition = new(EventSystem.current);
            eventDataCurrentPosition.position = mousePosition;

            List<RaycastResult> results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(eventDataCurrentPosition, results);

            return results.Any(x => x.gameObject == transform.GetChild(0).gameObject);
        }
    }
}