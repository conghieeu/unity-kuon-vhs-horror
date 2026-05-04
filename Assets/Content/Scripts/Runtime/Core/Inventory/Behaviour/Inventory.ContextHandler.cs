using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UHFPS.Input;
using UHFPS.Tools;

namespace UHFPS.Runtime
{
    public partial class Inventory
    {
        public struct Shortcut
        {
            public ShortcutSlot slot;
            public InventoryItem item;
        }

        private InventoryItem activeItem;
        private ExamineController examine;
        private PlayerItemsManager playerItems;

        private readonly Shortcut[] shortcuts = new Shortcut[4];
        private readonly List<string> combinePartners = new();

        private bool bindShortcut;
        private bool itemSelector;

        public void InitializeContextHandler()
        {
            contextMenu.contextUse.onClick.AddListener(UseItem);
            contextMenu.contextExamine.onClick.AddListener(ExamineItem);
            contextMenu.contextCombine.onClick.AddListener(CombineItem);
            contextMenu.contextShortcut.onClick.AddListener(ShortcutItem);
            contextMenu.contextDrop.onClick.AddListener(DropItem);
            contextMenu.contextDiscard.onClick.AddListener(DiscardItem);
            examine = playerPresence.Component<ExamineController>();
            playerItems = playerPresence.PlayerManager.PlayerItems;

            // initialize shortcuts
            shortcuts[0].slot = shortcutSettings.Slot01;
            shortcuts[1].slot = shortcutSettings.Slot02;
            shortcuts[2].slot = shortcutSettings.Slot03;
            shortcuts[3].slot = shortcutSettings.Slot04;
        }

        public void ContextUpdate()
        {
            if (examine.IsExamining || gameManager.IsPaused) 
                return;

            for (int i = 0; i < shortcuts.Length; i++)
            {
                if(InputManager.ReadButtonOnce("Shortcut" + (1 + i), Controls.SHORTCUT_PREFIX + (1 + i)))
                {
                    if (bindShortcut && activeItem != null)
                    {
                        SetShortcut(i);
                        bindShortcut = false;
                    }
                    else if (shortcuts[i].item != null && !gameManager.IsInventoryShown && PlayerItems.CanInteract)
                    {
                        UseItem(shortcuts[i].item);
                    }
                    break;
                }
            }
        }

        public int AutoShortcut(InventoryItem inventoryItem, bool replace = false, int replaceId = 0)
        {
            for (int i = 0; i < shortcuts.Length; i++)
            {
                if (shortcuts[i].item == null)
                {
                    SetShortcut(i, inventoryItem);
                    return i;
                }
            }

            if (replace)
            {
                SetShortcut(replaceId, inventoryItem);
                return replaceId;
            }

            return -1;
        }

        public void UseItem()
        {
            UseItem(activeItem);
            ShowContextMenu(false);
            activeItem = null;
        }

        public void UseItem(InventoryItem item, bool fromInventory = true)
        {
            if (itemSelector && fromInventory)
            {
                inventorySelector.OnInventoryItemSelect(this, item);
                inventorySelector = null;
                itemSelector = false;
            }
            else
            {
                var usableType = item.Item.UsableSettings.usableType;
                if (usableType == UsableType.PlayerItem)
                {
                    int playerItemIndex = item.Item.UsableSettings.playerItemIndex;
                    if(playerItemIndex >= 0) PlayerItems.SwitchPlayerItem(playerItemIndex);
                }
                else if(usableType == UsableType.HealthItem)
                {
                    PlayerHealth playerHealth = playerPresence.PlayerManager.PlayerHealth;
                    int healAmount = (int)item.Item.UsableSettings.healthPoints;
                    int currentHealth = playerHealth.EntityHealth;

                    if(currentHealth < playerHealth.MaxEntityHealth)
                    {
                        playerHealth.OnApplyHeal(healAmount);
                        RemoveItem(item, 1);
                    }
                }
                else if(usableType == UsableType.CustomEvent)
                {
                    CallUseEvent(item);
                    if(item.Item.UsableSettings.removeOnUse)
                        RemoveItem(item, 1);
                }
            }

            if(fromInventory)
                gameManager.ShowInventoryPanel(false);
        }

        public void CombineItem()
        {
            foreach (var item in carryingItems)
            {
                bool isCombinable = combinePartners.Contains(item.Key.ItemGuid);
                item.Key.SetCombinable(true, isCombinable);
            }

            ShowInventoryPrompt(true, promptSettings.combinePrompt);
            ShowContextMenu(false);
            combinePartners.Clear();
        }

        public void CombineWith(InventoryItem secondItem)
        {
            // reset the combinability status of items
            foreach (var item in carryingItems)
                item.Key.SetCombinable(false, false);

            // active = the item in which the combination was called
            var activeCombination = activeItem.Item.CombineSettings.FirstOrDefault(x => x.combineWithID == secondItem.ItemGuid);
            // second = the item that was used after selecting combine
            var secondCombination = secondItem.Item.CombineSettings.FirstOrDefault(x => x.combineWithID == activeItem.ItemGuid);

            // cache active and second custom data
            var activeCustomData = activeItem.CustomData;
            var secondCustomData = secondItem.CustomData;

            // active combination events
            if (!string.IsNullOrEmpty(activeCombination.combineWithID))
            {
                // call active inventory item, player item combination events
                if (!activeCombination.isCrafting)
                {
                    if (activeCombination.eventAfterCombine && secondItem.Item.UsableSettings.usableType == UsableType.PlayerItem)
                    {
                        int playerItemIndex = secondItem.Item.UsableSettings.playerItemIndex;
                        var playerItem = playerItems.PlayerItems[playerItemIndex];

                        // check if it is possible to combine a player item (e.g. reload) with an active item
                        if (playerItem.CanCombine()) playerItem.OnItemCombine(activeItem);
                    }

                    if (!activeCombination.keepAfterCombine)
                    {
                        // remove the active item if keepAfterCombine is false
                        RemoveShortcut(activeItem);
                        RemoveItem(activeItem, 1);
                    }
                }
                else
                {
                    RemoveShortcut(activeItem);
                    RemoveItem(activeItem, activeCombination.requiredCurrentAmount);
                }
            }

            // second combination events
            if (activeCombination.isCrafting)
            {
                RemoveShortcut(secondItem);
                RemoveItem(secondItem, activeCombination.requiredSecondAmount);
            }
            else if (activeCombination.removeSecondItem)
            {
                // remove the second item if removeSecondItem is true
                RemoveShortcut(secondItem);
                RemoveItem(secondItem, 1);
            }
            else if (!string.IsNullOrEmpty(secondCombination.combineWithID))
            {
                if (!secondCombination.keepAfterCombine)
                {
                    // remove the second item if keepAfterCombine is false
                    RemoveShortcut(secondItem);
                    RemoveItem(secondItem, 1);
                }
            }

            if (!activeCombination.isCrafting)
            {
                // select player item after combine
                if (activeCombination.selectAfterCombine)
                {
                    int playerItemIndex = activeCombination.playerItemIndex;
                    if (playerItemIndex >= 0) playerPresence.PlayerManager.PlayerItems.SwitchPlayerItem(playerItemIndex);
                }
            }

            if (!string.IsNullOrEmpty(activeCombination.resultCombineID))
            {
                int quantity = activeCombination.isCrafting ? activeCombination.resultItemAmount : 1;

                if (activeCombination.haveCustomData)
                {
                    ItemCustomData customData = activeCombination.customData;
                    if (activeCombination.inheritCustomData)
                    {
                        string inheritKey = activeCombination.inheritKey;

                        if (!activeCombination.inheritFromSecond)
                        {
                            customData = InheritCustomData(activeCustomData, inheritKey);
                        }
                        else
                        {
                            customData = InheritCustomData(secondCustomData, inheritKey);
                        }
                    }

                    AddItem(activeCombination.resultCombineID, (ushort)quantity, customData);
                }
                else
                {
                    AddItem(activeCombination.resultCombineID, (ushort)quantity, new());
                }
            }

            activeItem = null;
            ShowInventoryPrompt(false, null);
            combinePartners.Clear();
        }

        private ItemCustomData InheritCustomData(ItemCustomData customData, string inheritKey)
        {
            ItemCustomData newCustomData = new();

            if (inheritKey.IsEmpty())
            {
                newCustomData.JsonData = customData.JsonData;
            }
            else
            {
                var json = customData.GetJson();
                if (json.ContainsKey(inheritKey))
                {
                    string newJson = json[inheritKey].ToString();
                    newCustomData.JsonData = newJson;
                }
                else
                {
                    newCustomData.JsonData = customData.JsonData;
                }
            }

            return newCustomData;
        }

        public void ShortcutItem()
        {
            bindShortcut = true;
            ShowContextMenu(false);
            ShowInventoryPrompt(true, promptSettings.shortcutPrompt);
            contextMenu.blockerPanel.SetActive(true);
            combinePartners.Clear();
        }

        private void SetShortcut(int index)
        {
            if(shortcuts[index].item == activeItem)
            {
                shortcuts[index].item = null;
                shortcuts[index].slot.SetItem(null);
            }
            else
            {
                // unbind from other slot
                RemoveShortcut(activeItem);

                // bind to a new slot
                shortcuts[index].item = activeItem;
                shortcuts[index].slot.SetItem(activeItem);
            }

            activeItem = null;
            bindShortcut = false;
            contextMenu.blockerPanel.SetActive(false);
            ShowInventoryPrompt(false, null);
        }

        private void SetShortcut(int index, InventoryItem item)
        {
            shortcuts[index].item = item;
            shortcuts[index].slot.SetItem(item);
        }

        private void RemoveShortcut(InventoryItem item)
        {
            for (int i = 0; i < shortcuts.Length; i++)
            {
                if (shortcuts[i].item == item)
                {
                    shortcuts[i].item = null;
                    shortcuts[i].slot.SetItem(null);
                    break;
                }
            }
        }

        public void ExamineItem()
        {
            Vector3 examinePosition = examine.InventoryPosition;
            Item item = activeItem.Item;

            OnCloseInventory();

            if (item.ItemObject != null)
            {
                GameObject examineObj = Instantiate(item.ItemObject.Object, examinePosition, Quaternion.identity);
                examineObj.name = "Examine " + item.Title;
                examine.ExamineFromInventory(examineObj);
            }
            else
            {
                Debug.LogError("[Inventory] Could not examine an item because the item does not contain an item drop object!");
            }

            combinePartners.Clear();
            activeItem = null;
        }

        public void DropItem()
        {
            Vector3 dropPosition = examine.DropPosition;
            Item item = activeItem.Item;

            if(item.ItemObject != null)
            {
                GameObject dropObj = SaveGameManager.InstantiateSaveable(item.ItemObject, dropPosition, Vector3.zero, "Drop of " + item.Title);

                if(dropObj.TryGetComponent(out Rigidbody rigidbody))
                {
                    rigidbody.useGravity = true;
                    rigidbody.isKinematic = false;
                    rigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
                    rigidbody.AddForce(playerPresence.PlayerCamera.transform.forward * settings.dropStrength);
                }
                else
                {
                    Debug.LogError("[Inventory] Drop item must have a Rigidbody component to apply drop force!");
                    return;
                }

                if(dropObj.TryGetComponent(out InteractableItem interactable))
                {
                    interactable.DisableType = InteractableItem.DisableTypeEnum.Destroy;
                    interactable.Quantity = (ushort)activeItem.Quantity;
                }

                RemoveItem(activeItem);
            }
            else
            {
                Debug.LogError("[Inventory] Could not drop an item because the item does not contain an item drop object!");
            }

            RemoveShortcut(activeItem);
            ShowContextMenu(false);
            combinePartners.Clear();
            activeItem = null;
        }

        public void DiscardItem()
        {
            RemoveShortcut(activeItem);
            RemoveItem(activeItem);
            ShowContextMenu(false);
            combinePartners.Clear();
            activeItem = null;
        }
    }
}