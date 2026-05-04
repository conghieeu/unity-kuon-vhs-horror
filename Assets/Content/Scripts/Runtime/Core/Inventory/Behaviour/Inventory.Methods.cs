using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace UHFPS.Runtime
{
    public partial class Inventory
    {
        private readonly Dictionary<string, List<Action<ItemUseEvent>>> useEvents = new();

        public void RegisterUseEvent(string itemGuid, Action<ItemUseEvent> evt)
        {
            if (useEvents.TryGetValue(itemGuid, out var actions))
                actions.Add(evt);
            else
                useEvents[itemGuid] = new() { evt };
        }

        private void CallUseEvent(InventoryItem item)
        {
            if (useEvents.TryGetValue(item.ItemGuid, out var actions))
            {
                foreach (var action in actions)
                {
                    action.Invoke(new ItemUseEvent()
                    {
                        Item = item,
                        UseData = item.Item.UsableSettings.customData
                    });
                }
            }
        }

        /// <summary>
        /// Get <see cref="InventoryItem"/> reference from Inventory.
        /// </summary>
        public InventoryItem GetInventoryItem(string guid)
        {
            if (ContainsItem(guid, out OccupyData occupyData))
                return occupyData.inventoryItem;

            return null;
        }

        /// <summary>
        /// Get <see cref="OccupyData"/> reference from Inventory.
        /// </summary>
        public OccupyData GetOccupyData(string guid)
        {
            if (ContainsItem(guid, out OccupyData occupyData))
                return occupyData;

            return default;
        }

        /// <summary>
        /// Get the quantity of specified item from the inevntory.
        /// </summary>
        public int GetItemQuantity(string guid)
        {
            if (ContainsItem(guid, out OccupyData occupyData))
                return occupyData.inventoryItem.Quantity;

            return 0;
        }

        /// <summary>
        /// Get the quantity of all specified items from the inevntory.
        /// </summary>
        public int GetAllItemsQuantity(string guid)
        {
            int quantity = 0;

            foreach (var item in carryingItems)
            {
                if (item.Key.ItemGuid == guid)
                    quantity += item.Key.Quantity;
            }

            return quantity;
        }

        /// <summary>
        /// Set the quantity of the single item from the inevntory.
        /// </summary>
        public void SetItemQuantity(string guid, ushort quantity, bool removeWhenZero = true)
        {
            if (ContainsItem(guid, out OccupyData occupyData))
            {
                if (quantity >= 1 || !removeWhenZero)
                {
                    occupyData.inventoryItem.SetQuantity(quantity);
                    OnStackChanged.OnNext(new(guid, quantity));
                }
                else if (removeWhenZero)
                {
                    RemoveItem(occupyData.inventoryItem);
                }
            }
        }

        /// <summary>
        /// Remove the quantity of an items with the same guid. The item with the lowest quantity comes first.
        /// </summary>
        public void RemoveItemQuantityMany(string guid, ushort quantity)
        {
            int subtractQ = quantity;

            if (ContainsItemMany(guid, out OccupyData[] itemDatas))
            {
                // sort items by the closest quantity to zero
                itemDatas = itemDatas.OrderBy(x => x.inventoryItem.Quantity).ToArray();

                // iterate over each item with the same guid
                foreach (var itemData in itemDatas)
                {
                    var inventoryItem = itemData.inventoryItem;
                    int currQuantity = itemData.inventoryItem.Quantity;
                    int newQuantity = currQuantity - subtractQ;

                    if(newQuantity <= 0)
                    {
                        RemoveItem(inventoryItem);
                    }
                    else
                    {
                        inventoryItem.SetQuantity(newQuantity);
                        OnStackChanged.OnNext(new(guid, newQuantity));
                    }

                    if ((subtractQ -= currQuantity) <= 0)
                        break;
                }
            }
        }

        /// <summary>
        /// Check if there is a free space from the desired position.
        /// </summary>
        /// <param name="x">Slot X position.</param>
        /// <param name="y">Slot Y position.</param>
        /// <returns>Status whether there is free space in desired position.</returns>
        public bool CheckSpaceFromPosition(int x, int y, int width, int height, InventoryItem item = null)
        {
            for (int yy = y; yy < y + height; yy++)
            {
                for (int xx = x; xx < x + width; xx++)
                {
                    if (yy < MaxSlotXY.y && xx < MaxSlotXY.x)
                    {
                        if (slotArray[yy, xx] == SlotType.Locked)
                            return false;

                        InventorySlot slot = this[yy, xx];
                        if (slot == null) return false;

                        if (slot.itemInSlot != null)
                        {
                            if (item != null && slot.itemInSlot == item)
                                continue;
                            return false;
                        }
                    }
                    else return false;
                }
            }

            // check if width of the item has not overflowed from the container into the inventory
            if (ContainerOpened)
            {
                return (x <= currentContainer.Columns && x + width <= currentContainer.Columns) ||
                    (x >= currentContainer.Columns && x + width >= currentContainer.Columns);
            }

            return true;
        }

        /// <summary>
        /// Check if the item is in inventory.
        /// </summary>
        /// <param name="guid">Unique ID of the item.</param>
        /// <returns>Status whether the item is in inventory.</returns>
        public bool ContainsItem(string guid, out OccupyData occupyData)
        {
            if (carryingItems.Count > 0)
            {
                foreach (var item in carryingItems)
                {
                    if (item.Key.ItemGuid == guid)
                    {
                        occupyData = new OccupyData()
                        {
                            inventoryItem = item.Key,
                            occupiedSlots = item.Value
                        };
                        return true;
                    }
                }
            }

            occupyData = new OccupyData();
            return false;
        }

        /// <summary>
        /// Check if an item guid is in inventory and get a list of that items.
        /// </summary>
        /// <param name="guid">Unique ID of the item.</param>
        /// <returns>Status whether the item is in inventory.</returns>
        public bool ContainsItemMany(string guid, out OccupyData[] occupyData)
        {
            IList<OccupyData> occupyDatas = new List<OccupyData>();

            if (carryingItems.Count > 0)
            {
                foreach (var item in carryingItems)
                {
                    if (item.Key.ItemGuid == guid)
                    {
                        occupyDatas.Add(new()
                        {
                            inventoryItem = item.Key,
                            occupiedSlots = item.Value
                        });
                    }
                }
            }

            occupyData = occupyDatas.ToArray();
            return occupyDatas.Count > 0;
        }

        /// <summary>
        /// Check if the item is in inventory.
        /// </summary>
        /// <param name="guid">Unique ID of the item.</param>
        /// <returns>Status whether the item is in inventory.</returns>
        public bool ContainsItem(string guid)
        {
            if (carryingItems.Count > 0)
            {
                foreach (var item in carryingItems)
                {
                    if (item.Key.ItemGuid == guid)
                        return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Check if the item coordinates are in the container view.
        /// </summary>
        /// <param name="isContainer">Result if the coordinates of the item are in the container view.</param>
        /// <returns>Result if the coordinates are not overflowned.</returns>
        public bool IsContainerCoords(int x, int y)
        {
            if (y >= 0 && x >= 0 && x < MaxSlotXY.x && y < MaxSlotXY.y)
                return slotArray[y, x] == SlotType.Container;

            return false;
        }

        /// <summary>
        /// Check if the item coordinates are valid.
        /// </summary>
        public bool IsCoordsValid(int x, int y, int width, int height)
        {
            if (x < 0 || y < 0 ||
                x >= MaxSlotXY.x || y >= MaxSlotXY.y ||
                x + (width - 1) >= MaxSlotXY.x || y + (height - 1) >= MaxSlotXY.y)
                return false;

            SlotType prevType = slotArray[y, x];
            for (int yy = y; yy < y + height; yy++)
            {
                for (int xx = x; xx < x + width; xx++)
                {
                    SlotType currType = slotArray[yy, xx];

                    if (prevType != currType ||
                        prevType == SlotType.Locked ||
                        currType == SlotType.Locked)
                        return false;

                    prevType = currType;
                }
            }

            return true;
        }

        /// <summary>
        /// Occupy slots with the item in the new coordinates.
        /// </summary>
        private void OccupySlots(bool isContainerSpace, Vector2Int newCoords, InventoryItem inventoryItem)
        {
            Item item = inventoryItem.Item;
            int maxY = item.Height, maxX = item.Width;

            // rotate the item if the orientation is vertical
            if (inventoryItem.orientation == Orientation.Vertical)
            {
                maxY = item.Width;
                maxX = item.Height;
            }

            InventorySlot[] slotsToOccupy = new InventorySlot[maxY * maxX];

            int slotIndex = 0;
            for (int yy = newCoords.y; yy < newCoords.y + maxY; yy++)
            {
                for (int xx = newCoords.x; xx < newCoords.x + maxX; xx++)
                {
                    InventorySlot slot = this[yy, xx];
                    slot.itemInSlot = inventoryItem;
                    slotsToOccupy[slotIndex++] = slot;
                }
            }

            if (!isContainerSpace)
            {
                carryingItems[inventoryItem] = slotsToOccupy;
            }
            else
            {
                containerItems[inventoryItem] = slotsToOccupy;
            }
        }

        private void SetInventorySlots(InventoryContainer container, bool add)
        {
            int newRows = Mathf.Max(SlotXY.y, add ? container.Rows : 0);
            int newColumns = SlotXY.x + (add ? container.Columns : 0);
            SlotType[,] newSlotArray = new SlotType[newRows, newColumns];

            if (add)
            {
                for (int y = 0; y < newRows; y++)
                {
                    for (int x = 0; x < newColumns; x++)
                    {
                        if (x < SlotXY.x)
                        {
                            newSlotArray[y, x] = slotArray[y, x];
                        }
                        else if (y < container.Rows)
                        {
                            newSlotArray[y, x] = SlotType.Container;
                        }
                    }
                }
            }
            else
            {
                for (int y = 0; y < newRows; y++)
                {
                    for (int x = 0; x < newColumns; x++)
                    {
                        newSlotArray[y, x] = slotArray[y, x];
                    }
                }
            }

            slotArray = newSlotArray;
        }

        private bool CheckAndRegisterCombinePartners(InventoryItem selected)
        {
            IList<string> possiblePartners = new List<string>();

            // check player item combination
            if (playerItems.IsAnyEquipped)
            {
                int currentItem = playerItems.CurrentItemIndex;
                var playerItem = playerItems.PlayerItems[currentItem];

                if (playerItem.CanCombine())
                {
                    foreach (var itemCombine in selected.Item.CombineSettings)
                    {
                        var inventoryItem = items[itemCombine.combineWithID];
                        int playerItemIndex = inventoryItem.UsableSettings.playerItemIndex;

                        if (playerItems.CurrentItemIndex == playerItemIndex)
                            possiblePartners.Add(itemCombine.combineWithID);
                    }
                }
            }

            // check combination settings
            foreach (var item in carryingItems)
            {
                foreach (var combine in selected.Item.CombineSettings)
                {
                    if (item.Key.ItemGuid != combine.combineWithID)
                        continue;

                    // if crafting is enabled, check the required amount of items
                    if (combine.isCrafting)
                    {
                        int reqCurrent = combine.requiredCurrentAmount;
                        int reqSecond = combine.requiredSecondAmount;

                        if (selected.Quantity < reqCurrent)
                            continue;

                        if (item.Key.Quantity >= reqSecond)
                            possiblePartners.Add(item.Key.ItemGuid);
                    }
                    else
                    {
                        // otherwise just add a possible combination partner
                        possiblePartners.Add(item.Key.ItemGuid);
                    }
                }
            }

            combinePartners.AddRange(possiblePartners);
            return possiblePartners.Count > 0;
        }

        private void AddItemToFreeSpace(FreeSpace space, InventoryItem inventoryItem)
        {
            Item item = inventoryItem.Item;
            int maxY = item.Height, maxX = item.Width;

            if (space.orientation == Orientation.Vertical)
            {
                maxY = item.Width;
                maxX = item.Height;
            }

            InventorySlot[] occupiedSlots = new InventorySlot[maxY * maxX];
            int slotIndex = 0;

            for (int y = space.y; y < space.y + maxY; y++)
            {
                for (int x = space.x; x < space.x + maxX; x++)
                {
                    InventorySlot slot = slots[y, x];
                    slot.itemInSlot = inventoryItem;
                    occupiedSlots[slotIndex++] = slot;
                }
            }

            carryingItems.Add(inventoryItem, occupiedSlots);
        }

        private bool CheckSpace(ushort width, ushort height, out FreeSpace slotSpace)
        {
            for (int y = 0; y < SlotXY.y; y++)
            {
                for (int x = 0; x < SlotXY.x; x++)
                {
                    if (width == height)
                    {
                        if (CheckSpaceFromPosition(x, y, width, height))
                        {
                            slotSpace = new FreeSpace(x, y, Orientation.Horizontal);
                            return true;
                        }
                    }
                    else
                    {
                        if (CheckSpaceFromPosition(x, y, width, height))
                        {
                            slotSpace = new FreeSpace(x, y, Orientation.Horizontal);
                            return true;
                        }
                        else if (CheckSpaceFromPosition(x, y, height, width))
                        {
                            slotSpace = new FreeSpace(x, y, Orientation.Vertical);
                            return true;
                        }
                    }
                }
            }

            slotSpace = new FreeSpace();
            return false;
        }
    }
}