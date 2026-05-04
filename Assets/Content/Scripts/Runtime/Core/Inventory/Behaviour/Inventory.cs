using System;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reactive.Disposables;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json.Linq;
using ThunderWire.Attributes;
using UHFPS.Scriptable;
using UHFPS.Tools;
using TMPro;

namespace UHFPS.Runtime
{
    public enum Orientation { Horizontal, Vertical };
    public enum InventorySound { ItemSelect, ItemMove, ItemPut, ItemError }

    public struct ItemUpdate
    {
        public string guid;
        public int quantity;

        public ItemUpdate(string guid, int quantity)
        {
            this.guid = guid;
            this.quantity = quantity;
        }
    }

    [Docs("https://docs.twgamesdev.com/uhfps/guides/inventory")]
    public partial class Inventory : Singleton<Inventory>, ISaveableCustom
    {
        #region Structures
        [Serializable]
        public sealed class Settings
        {
            public ushort rows = 5;
            public ushort columns = 5;
            public float cellSize = 100f;
            public float spacing = 10f;
            public float dragTime = 0.05f;
            public float rotateTime = 0.05f;
            public float dropStrength = 10f;
        }

        [Serializable]
        public sealed class SlotSettings
        {
            public GameObject slotPrefab;
            public GameObject slotItemPrefab;

            [Header("Slot Textures")]
            public Sprite normalSlotFrame;
            public Sprite lockedSlotFrame;
            public Sprite missingItemSprite;

            [Header("Slot Colors")]
            public Color itemNormalColor = Color.white;
            public Color itemHoverColor = Color.white;
            public Color itemMoveColor = Color.white;
            public Color itemErrorColor = Color.white;
            public float colorChangeSpeed = 20f;

            [Header("Slot Quantity")]
            public Color normalQuantityColor = Color.white;
            public Color zeroQuantityColor = Color.red;
        }

        [Serializable]
        public sealed class ContainerSettings
        {
            public RectTransform containerObject;
            public RectTransform containerItems;
            public GridLayoutGroup containerSlots;
            public TMP_Text containerName;
        }

        [Serializable]
        public sealed class ItemInfo
        {
            public GameObject infoPanel;
            public TMP_Text itemTitle;
            public TMP_Text itemDescription;
        }

        [Serializable]
        public sealed class PromptSettings
        {
            public CanvasGroup promptPanel;
            public GString shortcutPrompt;
            public GString combinePrompt;
        }

        [Serializable]
        public sealed class ContextMenu
        {
            public GameObject contextMenu;
            public GameObject blockerPanel;
            public float disabledAlpha = 0.35f;

            [Header("Context Buttons")]
            public Button contextUse;
            public Button contextExamine;
            public Button contextCombine;
            public Button contextShortcut;
            public Button contextDrop;
            public Button contextDiscard;
        }

        [Serializable]
        public sealed class ShortcutSettings
        {
            public ShortcutSlot Slot01;
            public ShortcutSlot Slot02;
            public ShortcutSlot Slot03;
            public ShortcutSlot Slot04;
        }

        [Serializable]
        public sealed class Sounds
        {
            public AudioClip itemSelectSound;
            public AudioClip itemMoveSound;
            public AudioClip itemPutSound;
            public AudioClip itemErrorSound;

            [Header("Settings")]
            [Range(0f, 1f)]
            public float volume = 1f;
            public float nextSoundDelay = 0.1f;
        }

        [Serializable]
        public sealed class ExpandableSlots
        {
            public bool enabled;
            public bool showExpandableSlots;
            public ushort expandableRows;
        }

        [Serializable]
        public struct StartingItem
        {
            public string GUID;
            public string title;
            public ushort quantity;
            public bool setShortcut;
            public ushort shortcutKey;
            public ItemCustomData data;
        }

        public struct OccupyData
        {
            public InventoryItem inventoryItem;
            public InventorySlot[] occupiedSlots;
        }

        public struct FreeSpace
        {
            public int x;
            public int y;
            public Orientation orientation;

            public FreeSpace(int x, int y, Orientation orientation)
            {
                this.x = x;
                this.y = y;
                this.orientation = orientation;
            }
        }

        public struct ItemCreationData
        {
            public string itemGuid;
            public ushort quantity;
            public Orientation orientation;
            public Vector2Int coords;
            public ItemCustomData customData;
            public Transform parent;
            public InventorySlot[,] slotsSpace;
        }

        public enum SlotType { Locked, Inventory, Container }
        #endregion

        public InventoryDatabase inventoryDatabase;

        // references
        public Transform inventoryContainers;
        public GridLayoutGroup slotsLayoutGrid;
        public Transform itemsTransform;

        // control contexts
        public ControlsContext[] ControlsContexts;

        // settings
        public Settings settings;
        public SlotSettings slotSettings;
        public ContainerSettings containerSettings;
        public ItemInfo itemInfo;
        public ShortcutSettings shortcutSettings;
        public PromptSettings promptSettings;
        public ContextMenu contextMenu;
        public Sounds sounds;

        // features
        public List<StartingItem> startingItems = new();
        public ExpandableSlots expandableSlots;

        // inventory
        private SlotType[,] slotArray;
        public InventorySlot[,] slots;
        public Dictionary<string, Item> items;
        public Dictionary<InventoryItem, InventorySlot[]> carryingItems;

        // container
        public InventoryContainer currentContainer;
        public InventorySlot[,] containerSlots;
        public Dictionary<InventoryItem, InventorySlot[]> containerItems;

        private int expandedSlots;
        private float nextSoundDelay;
        private bool contextShown;

        private readonly CompositeDisposable disposables = new();
        private IInventorySelector inventorySelector;
        private PlayerPresenceManager playerPresence;
        private GameManager gameManager;
        private AudioSource inventorySounds;

        public GameObject Player => playerPresence.Player;
        public PlayerItemsManager PlayerItems => playerPresence.PlayerManager.PlayerItems;
        public PlayerHealth PlayerHealth => playerPresence.PlayerManager.PlayerHealth;
        public bool ContainerOpened => currentContainer != null;

        public Subject<ItemUpdate> OnItemAdded = new(); 
        public Subject<ItemUpdate> OnItemRemoved = new();
        public Subject<ItemUpdate> OnStackChanged = new();
        public Subject<ItemUpdate> OnInventoryChanged = new();

        public Vector2Int SlotXY
        {
            get
            {
                int x = settings.columns;
                int y = settings.rows;
                return new(x, y);
            }
        }

        public Vector2Int MaxSlotXY
        {
            get
            {
                int x = slotArray.GetLength(1);
                int y = slotArray.GetLength(0);
                return new(x, y);
            }
        }

        public InventorySlot this[int y, int x]
        {
            get
            {
                try
                {
                    if (ContainerOpened && IsContainerCoords(x, y))
                    {
                        int containerX = x - SlotXY.x;
                        return containerSlots[y, containerX];
                    }

                    return slots[y, x];
                }
                catch
                {
                    return null;
                }
            }
        }

        private void Awake()
        {
            slotArray = new SlotType[settings.rows, settings.columns];
            slots = new InventorySlot[settings.rows, settings.columns];
            items = new Dictionary<string, Item>();

            carryingItems = new Dictionary<InventoryItem, InventorySlot[]>();
            containerItems = new Dictionary<InventoryItem, InventorySlot[]>();

            // slot grid setting
            slotsLayoutGrid.cellSize = new Vector2(settings.cellSize, settings.cellSize);
            slotsLayoutGrid.spacing = new Vector2(settings.spacing, settings.spacing);

            // slot instantiation
            for (int y = 0; y < settings.rows; y++)
            {
                for (int x = 0; x < settings.columns; x++)
                {
                    GameObject slot = Instantiate(slotSettings.slotPrefab, slotsLayoutGrid.transform);
                    slot.name = $"Slot [{y},{x}]";

                    RectTransform rect = slot.GetComponent<RectTransform>();
                    rect.localScale = Vector3.one;

                    InventorySlot inventorySlot = slot.GetComponent<InventorySlot>();
                    inventorySlot.frame.sprite = slotSettings.normalSlotFrame;

                    slots[y, x] = inventorySlot;
                    slotArray[y, x] = SlotType.Inventory;

                    if (expandableSlots.enabled && y >= settings.rows - expandableSlots.expandableRows)
                    {
                        inventorySlot.frame.sprite = slotSettings.lockedSlotFrame;
                        inventorySlot.CanvasGroup.alpha = 0.3f;
                        slotArray[y, x] = SlotType.Locked;
                    }
                }
            }

            if (!inventoryDatabase) throw new NullReferenceException("Inventory asset is not set!");

            // item caching
            foreach (var section in inventoryDatabase.Sections)
            {
                foreach (var item in section.Items)
                {
                    Item itemClone = item.DeepCopy();

#if UHFPS_LOCALIZATION
                    // item title
                    itemClone.LocalizationSettings.titleKey.SubscribeGloc(text =>
                    {
                        if (!string.IsNullOrEmpty(text))
                            itemClone.Title = text;
                    });

                    // item description
                    itemClone.LocalizationSettings.descriptionKey.SubscribeGloc(text =>
                    {
                        if (!string.IsNullOrEmpty(text))
                            itemClone.Description = text;
                    });
#endif

                    items.Add(item.GUID, itemClone);
                }
            }

            // initialize other stuff
            playerPresence = GetComponent<PlayerPresenceManager>();
            gameManager = GetComponent<GameManager>();
            inventorySounds = GetComponent<AudioSource>();
            contextMenu.contextMenu.SetActive(false);
            contextMenu.blockerPanel.SetActive(false);
            itemInfo.infoPanel.SetActive(false);

            // subscribe events to one event
            disposables.Add(OnItemAdded.Subscribe(OnInventoryChanged));
            disposables.Add(OnItemRemoved.Subscribe(OnInventoryChanged));
            disposables.Add(OnStackChanged.Subscribe(OnInventoryChanged));

            // initialize context handler
            InitializeContextHandler();
        }

        private void Start()
        {
            if (!SaveGameManager.GameWillLoad)
            {
                foreach (var item in startingItems)
                {
                    if(item.setShortcut) AddItem(item.GUID, item.quantity, item.shortcutKey, item.data);
                    else AddItem(item.GUID, item.quantity, item.data);
                }
            }

            foreach (var control in ControlsContexts)
            {
                control.SubscribeGloc();
            }

            promptSettings.shortcutPrompt.SubscribeGlocMany();
            promptSettings.combinePrompt.SubscribeGloc();
        }

        private void Update()
        {
            nextSoundDelay = nextSoundDelay > 0
                ? nextSoundDelay -= Time.deltaTime : 0;

            ContextUpdate();
        }

        private void OnDestroy()
        {
            disposables.Dispose();
        }

        /// <summary>
        /// Add item to the free inventory space.
        /// </summary>
        /// <param name="guid">Unique ID of the item.</param>
        /// <param name="quantity">Quantity of the item to be added.</param>
        /// <param name="customData">Custom data of specified item.</param>
        /// <returns>Status whether the item has been added to the inventory.</returns>
        public bool AddItem(string guid, ushort quantity, ItemCustomData customData)
        {
            return AddItem(guid, quantity, customData, out _);
        }

        /// <summary>
        /// Add item to the free inventory space.
        /// </summary>
        /// <param name="guid">Unique ID of the item.</param>
        /// <param name="quantity">Quantity of the item to be added.</param>
        /// <param name="shortcut">The shortcut that will be set to this item.</param>
        /// <param name="customData">Custom data of specified item.</param>
        /// <returns>Status whether the item has been added to the inventory.</returns>
        public bool AddItem(string guid, ushort quantity, ushort shortcut, ItemCustomData customData)
        {
            bool result = AddItem(guid, quantity, customData, out InventoryItem inventoryItem);
            if (result) SetShortcut(shortcut, inventoryItem);
            return result;
        }

        /// <summary>
        /// Add item to the free inventory space.
        /// </summary>
        /// <param name="guid">Unique ID of the item.</param>
        /// <param name="quantity">Quantity of the item to be added.</param>
        /// <param name="customData">Custom data of specified item.</param>
        /// <param name="inventoryItem">Item that has been added to the inventory.</param>
        /// <returns>Status whether the item has been added to the inventory.</returns>
        public bool AddItem(string guid, ushort quantity, ItemCustomData customData, out InventoryItem inventoryItem)
        {
            inventoryItem = null;
            int availableQ = quantity;

            // check if the guid is in the items cache
            if (items.ContainsKey(guid))
            {
                Item item = items[guid];
                ushort maxStack = item.Properties.maxStack;

                if (item.Settings.isStackable && ContainsItemMany(guid, out OccupyData[] itemDatas))
                {
                    // sort items by the closest quantity to maxStack
                    itemDatas = itemDatas.OrderByDescending(x =>
                    {
                        int itemQuantity = x.inventoryItem.Quantity;
                        return maxStack - itemQuantity;
                    }).ToArray();

                    // iterate over each item with the same guid
                    foreach (var itemData in itemDatas)
                    {
                        int currQuantity = itemData.inventoryItem.Quantity;
                        inventoryItem = itemData.inventoryItem;

                        if (maxStack == 0)
                        {
                            currQuantity += availableQ;
                            itemData.inventoryItem.SetQuantity(currQuantity);
                            OnStackChanged.OnNext(new(guid, itemData.inventoryItem.Quantity));
                        }
                        else if (currQuantity < maxStack)
                        {
                            int newQ = currQuantity + availableQ;
                            int q = Mathf.Min(maxStack, newQ);
                            availableQ -= q - currQuantity;
                            itemData.inventoryItem.SetQuantity(q);
                            OnStackChanged.OnNext(new(guid, itemData.inventoryItem.Quantity));
                        }

                        if (availableQ <= 0)
                            break;
                    }

                    // if there is still some quantity left, create new items
                    if (availableQ > 0)
                    {
                        int iterations = (int)Math.Ceiling((float)availableQ / maxStack);
                        for (int i = 0; i < iterations; i++)
                        {
                            int q = Mathf.Min(maxStack, availableQ);
                            availableQ -= q;
                            inventoryItem = CreateItem(guid, (ushort)q, customData);
                            OnItemAdded.OnNext(new(guid, q));
                        }
                    }
                }
                else
                {
                    if (availableQ < maxStack || maxStack == 0)
                    {
                        inventoryItem = CreateItem(guid, availableQ, customData);
                        OnItemAdded.OnNext(new(guid, availableQ));
                    }
                    else
                    {
                        int iterations = (int)Math.Ceiling((float)availableQ / maxStack);
                        for (int i = 0; i < iterations; i++)
                        {
                            int q = Mathf.Min(maxStack, availableQ);
                            availableQ -= q;
                            inventoryItem = CreateItem(guid, (ushort)q, customData);
                            OnItemAdded.OnNext(new(guid, q));
                        }
                    }
                }
            }

            return inventoryItem != null;
        }

        /// <summary>
        /// Remove item from inventory completly.
        /// </summary>
        /// <param name="guid">Unique ID of the item.</param>
        /// <returns>Status whether the item has been removed from the inventory.</returns>
        public bool RemoveItem(string guid)
        {
            if (ContainsItem(guid, out OccupyData itemData))
            {
                int quantity = itemData.inventoryItem.Quantity;
                carryingItems.Remove(itemData.inventoryItem);
                Destroy(itemData.inventoryItem.gameObject);

                foreach (var slot in itemData.occupiedSlots)
                {
                    slot.itemInSlot = null;
                }

                OnItemRemoved.OnNext(new (guid, quantity));
                return true;
            }

            return false;
        }

        /// <summary>
        /// Remove item quantity from inventory.
        /// </summary>
        /// <param name="guid">Unique ID of the item.</param>
        /// <param name="quantity">Quantity of the item to be removed.</param>
        /// <returns>Quantity of the item in the inevntory.</returns>
        public int RemoveItem(string guid, ushort quantity)
        {
            if (ContainsItem(guid, out OccupyData itemData))
            {
                if ((itemData.inventoryItem.Quantity - quantity) >= 1)
                {
                    int q = itemData.inventoryItem.Quantity - quantity;
                    itemData.inventoryItem.SetQuantity(q);
                    OnItemRemoved.OnNext(new(guid, quantity));
                    return q;
                }
                else
                {
                    carryingItems.Remove(itemData.inventoryItem);
                    Destroy(itemData.inventoryItem.gameObject);

                    foreach (var slot in itemData.occupiedSlots)
                    {
                        slot.itemInSlot = null;
                    }

                    OnItemRemoved.OnNext(new(guid, quantity));
                }
            }

            return 0;
        }

        /// <summary>
        /// Remove item from inventory or container completly.
        /// </summary>
        public void RemoveItem(InventoryItem inventoryItem)
        {
            string guid = inventoryItem.ItemGuid;
            int quantity = inventoryItem.Quantity;

            if (!inventoryItem.isContainerItem && carryingItems.ContainsKey(inventoryItem))
            {
                InventorySlot[] occupiedSlots = carryingItems[inventoryItem];
                carryingItems.Remove(inventoryItem);

                foreach (var slot in occupiedSlots)
                {
                    slot.itemInSlot = null;
                }
            }
            else if (containerItems.ContainsKey(inventoryItem))
            {
                InventorySlot[] occupiedSlots = containerItems[inventoryItem];
                containerItems.Remove(inventoryItem);

                foreach (var slot in occupiedSlots)
                {
                    slot.itemInSlot = null;
                }

                if (currentContainer != null)
                    currentContainer.Remove(inventoryItem);
            }

            OnItemRemoved.OnNext(new(guid, quantity));
            Destroy(inventoryItem.gameObject);
        }

        /// <summary>
        /// Remove item quantity from inventory.
        /// </summary>
        public int RemoveItem(InventoryItem inventoryItem, ushort quantity)
        {
            string guid = inventoryItem.ItemGuid;

            if (carryingItems.ContainsKey(inventoryItem))
            {
                if ((inventoryItem.Quantity - quantity) >= 1)
                {
                    int q = inventoryItem.Quantity - quantity;
                    inventoryItem.SetQuantity(q);
                    return q;
                }
                else
                {
                    InventorySlot[] occupiedSlots = carryingItems[inventoryItem];
                    carryingItems.Remove(inventoryItem);

                    foreach (var slot in occupiedSlots)
                    {
                        slot.itemInSlot = null;
                    }

                    Destroy(inventoryItem.gameObject);
                }
            }

            OnItemRemoved.OnNext(new(guid, quantity));
            return 0;
        }

        /// <summary>
        /// Expand the inventory slots that are expandable.
        /// </summary>
        /// <param name="rows">Rows to be expanded.</param>
        public void ExpandInventory(int expandSlots, bool expandRows)
        {
            if (expandableSlots.enabled)
            {
                int expandableY = SlotXY.y - expandableSlots.expandableRows;
                int expandable = expandRows ? expandSlots * SlotXY.x : expandSlots;
                int toExpand = expandable;

                for (int y = expandableY; y < SlotXY.y; y++)
                {
                    for (int x = 0; x < SlotXY.x; x++)
                    {
                        if (toExpand == 0) 
                            break;

                        if(slotArray[y, x] == SlotType.Locked)
                        {
                            InventorySlot slot = slots[y, x];
                            slot.frame.sprite = slotSettings.normalSlotFrame;
                            slot.CanvasGroup.alpha = 1f;

                            slotArray[y, x] = SlotType.Inventory;
                            toExpand--;
                        }
                    }
                }

                expandedSlots += expandable;
            }
        }

        /// <summary>
        /// Move item to the desired position.
        /// </summary>
        public void MoveItem(Vector2Int lastCoords, Vector2Int newCoords, InventoryItem inventoryItem)
        {
            // check if the last and new coordinates are in the inventory or container space
            bool lastContainerSpace = false, newContainerSpace = false;
            if (ContainerOpened)
            {
                lastContainerSpace = IsContainerCoords(lastCoords.x, lastCoords.y);
                newContainerSpace = IsContainerCoords(newCoords.x, newCoords.y);
            }

            if (!lastContainerSpace)
            {
                // unoccupy slots from inventory space
                if (carryingItems.TryGetValue(inventoryItem, out var inventorySlots))
                {
                    foreach (var slot in inventorySlots)
                    {
                        slot.itemInSlot = null;
                    }
                }
            }
            else
            {
                // unoccupy slots from container space
                if (containerItems.TryGetValue(inventoryItem, out var containerSlots))
                {
                    foreach (var slot in containerSlots)
                    {
                        slot.itemInSlot = null;
                    }
                }
            }

            string guid = inventoryItem.ItemGuid;
            int quantity = inventoryItem.Quantity;

            // if the new coordinates are in inventory space
            if (!newContainerSpace)
            {
                if (lastContainerSpace)
                {
                    // remove item from the container space
                    currentContainer.Remove(inventoryItem);
                    containerItems.Remove(inventoryItem);
                    inventoryItem.ContainerGuid = string.Empty;
                    inventoryItem.isContainerItem = false;

                    // add item to inventory space
                    carryingItems.Add(inventoryItem, null);
                    OnItemAdded.OnNext(new(guid, quantity));
                }

                // set item parent to the inventory panel transform
                inventoryItem.transform.SetParent(itemsTransform);
            }
            // if the new coordinates are in container space
            else
            {
                Vector2Int localCoords = newCoords;
                localCoords.x -= SlotXY.x;

                if (!lastContainerSpace)
                {
                    // remove item from the inventory space
                    carryingItems.Remove(inventoryItem);
                    RemoveShortcut(inventoryItem);

                    // add item to container space
                    currentContainer.Store(inventoryItem, localCoords);
                    containerItems.Add(inventoryItem, null);
                    inventoryItem.isContainerItem = true;
                    OnItemRemoved.OnNext(new(guid, quantity));
                }
                else
                {
                    // move a container item to new coordinates
                    currentContainer.Move(inventoryItem, new FreeSpace()
                    {
                        x = localCoords.x,
                        y = localCoords.y,
                        orientation = inventoryItem.orientation
                    });
                }

                // set item parent to the container panel transform
                inventoryItem.transform.SetParent(containerSettings.containerItems);

                // if the item is equipped, unequip the current item
                if (inventoryItem.Item.UsableSettings.usableType == UsableType.PlayerItem)
                {
                    int playerItemIndex = inventoryItem.Item.UsableSettings.playerItemIndex;
                    if (playerItemIndex >= 0 && PlayerItems.CurrentItemIndex == playerItemIndex)
                        PlayerItems.DeselectCurrent();
                }
            }

            // occupy new slots
            OccupySlots(newContainerSpace, newCoords, inventoryItem);
        }

        /// <summary>
        /// Open the inventory container.
        /// </summary>
        /// <param name="container">Container to be opened.</param>
        public void OpenContainer(InventoryContainer container)
        {
            // expand inventory slots with container slots
            SetInventorySlots(container, true);

            // initialize container slots
            containerSlots = new InventorySlot[container.Rows, container.Columns];
            currentContainer = container;

            // slot grid setting
            containerSettings.containerSlots.cellSize = new Vector2(settings.cellSize, settings.cellSize);
            containerSettings.containerSlots.spacing = new Vector2(settings.spacing, settings.spacing);

            // set the container panel size to fit the number of container columns
            Vector2 grdLayoutSize = containerSettings.containerObject.sizeDelta;
            grdLayoutSize.x = settings.cellSize * container.Columns + settings.spacing * (container.Columns - 1);
            containerSettings.containerObject.sizeDelta = grdLayoutSize;

            // slot instantiation
            for (int y = 0; y < container.Rows; y++)
            {
                for (int x = 0; x < container.Columns; x++)
                {
                    GameObject slot = Instantiate(slotSettings.slotPrefab, containerSettings.containerSlots.transform);
                    slot.name = $"Container Slot [{y},{x}]";

                    RectTransform rect = slot.GetComponent<RectTransform>();
                    rect.localScale = Vector3.one;

                    InventorySlot inventorySlot = slot.GetComponent<InventorySlot>();
                    containerSlots[y, x] = inventorySlot;
                }
            }

            // container items creation
            foreach (var containerItem in container.ContainerItems)
            {
                InventoryItem inventoryItem = CreateItem(new ItemCreationData()
                {
                    itemGuid = containerItem.Value.ItemGuid,
                    quantity = (ushort)containerItem.Value.Quantity,
                    orientation = containerItem.Value.Orientation,
                    coords = containerItem.Value.Coords,
                    customData = containerItem.Value.CustomData,
                    parent = containerSettings.containerItems,
                    slotsSpace = containerSlots
                });

                inventoryItem.ContainerGuid = containerItem.Key;
                inventoryItem.isContainerItem = true;
                inventoryItem.ContainerOpened((ushort)SlotXY.x);
                containerItems.Add(inventoryItem, null);

                Vector2Int containerCoords = containerItem.Value.Coords;
                containerCoords.x += SlotXY.x;

                OccupySlots(true, containerCoords, inventoryItem);
            }

            containerSettings.containerObject.gameObject.SetActive(true);

            if (!string.IsNullOrEmpty((container.ContainerTitle)))
            {
                string title = container.ContainerTitle;
                containerSettings.containerName.text = title.ToUpper();
                containerSettings.containerName.enabled = true;
            }

            gameManager.SetBlur(true, true);
            gameManager.FreezePlayer(true, true, false);
            gameManager.ShowInventoryPanel(true);
        }

        /// <summary>
        /// Open the inventory item selection menu.
        /// </summary>
        public void OpenItemSelector(IInventorySelector inventorySelector)
        {
            itemSelector = true;
            this.inventorySelector = inventorySelector;
            gameManager.ShowInventoryPanel(true);
        }

        /// <summary>
        /// Play inventory sound.
        /// </summary>
        public void PlayInventorySound(InventorySound sound)
        {
            if (inventorySounds != null && nextSoundDelay <= 0)
            {
                AudioClip clip = null;

                switch (sound)
                {
                    case InventorySound.ItemSelect:
                        clip = sounds.itemSelectSound;
                        break;
                    case InventorySound.ItemMove:
                        clip = sounds.itemMoveSound;
                        break;
                    case InventorySound.ItemPut:
                        clip = sounds.itemPutSound;
                        break;
                    case InventorySound.ItemError:
                        clip = sounds.itemErrorSound;
                        break;
                }

                if (clip != null)
                {
                    inventorySounds.PlayOneShot(clip, sounds.volume);
                    nextSoundDelay = sounds.nextSoundDelay;
                }
            }
        }

        public void ShowInventoryPrompt(bool show, string text, bool forceHide = false)
        {
            if (!show && !promptSettings.promptPanel.gameObject.activeSelf)
                return;

            if (show)
            {
                promptSettings.promptPanel.gameObject.SetActive(true);
                promptSettings.promptPanel.GetComponentInChildren<TMP_Text>().text = text;
            }
            else if(forceHide)
            {
                promptSettings.promptPanel.alpha = 0f;
                promptSettings.promptPanel.gameObject.SetActive(false);
                return;
            }

            var coroutine = CanvasGroupFader.StartFade(promptSettings.promptPanel, show, 5f, () =>
            {
                if(!show) promptSettings.promptPanel.gameObject.SetActive(false);
            });

            StartCoroutine(coroutine);
        }

        #region Events
        public void ShowContextMenu(bool show, InventoryItem invItem = null)
        {
            if (!ContainerOpened && show && invItem != null)
            {
                activeItem = invItem;
                Item item = invItem.Item;

                Vector3[] itemCorners = new Vector3[4];
                invItem.GetComponent<RectTransform>().GetWorldCorners(itemCorners);

                if (invItem.orientation == Orientation.Horizontal)
                    contextMenu.contextMenu.transform.position = itemCorners[2];
                else if (invItem.orientation == Orientation.Vertical)
                    contextMenu.contextMenu.transform.position = itemCorners[1];

                // use button
                bool use = item.Settings.isUsable || itemSelector;
                bool canHeal = PlayerHealth.EntityHealth < PlayerHealth.MaxEntityHealth;
                bool isHealthItem = item.UsableSettings.usableType == UsableType.HealthItem;
                bool useEnabled = !isHealthItem || isHealthItem && canHeal;
                float useAlpha = useEnabled ? 1f : contextMenu.disabledAlpha;
                contextMenu.contextUse.GetComponent<CanvasGroup>().alpha = useAlpha;
                contextMenu.contextUse.interactable = useEnabled;
                contextMenu.contextUse.gameObject.SetActive(use);

                // examine button
                bool examine = item.Settings.isExaminable && !itemSelector;
                contextMenu.contextExamine.gameObject.SetActive(examine);

                // combine button
                bool combineEnabled = CheckAndRegisterCombinePartners(invItem);
                bool combine = item.Settings.isCombinable && !invItem.isContainerItem && !itemSelector;
                float combineAlpha = combineEnabled ? 1f : contextMenu.disabledAlpha;

                contextMenu.contextCombine.GetComponent<CanvasGroup>().alpha = combineAlpha;
                contextMenu.contextCombine.interactable = combineEnabled;
                contextMenu.contextCombine.gameObject.SetActive(combine);

                // shortcut button
                bool shortcut = item.Settings.canBindShortcut && !invItem.isContainerItem && !itemSelector;
                contextMenu.contextShortcut.gameObject.SetActive(shortcut);

                // drop button
                bool drop = item.Settings.isDroppable;
                contextMenu.contextDrop.gameObject.SetActive(drop);

                // discard button
                bool discard = item.Settings.isDiscardable;
                contextMenu.contextDiscard.gameObject.SetActive(discard);

                if (use || examine || combine || shortcut || drop || discard)
                {
                    contextMenu.contextMenu.SetActive(true);
                    contextMenu.blockerPanel.SetActive(true);
                    contextShown = true;
                }
            }
            else
            {
                contextMenu.contextMenu.SetActive(false);
                contextMenu.blockerPanel.SetActive(false);
                contextMenu.contextUse.gameObject.SetActive(false);
                contextMenu.contextExamine.gameObject.SetActive(false);
                contextMenu.contextCombine.gameObject.SetActive(false);
                contextMenu.contextShortcut.gameObject.SetActive(false);
                contextMenu.contextDrop.gameObject.SetActive(false);
                contextMenu.contextDiscard.gameObject.SetActive(false);
                contextShown = false;
            }
        }

        public void ShowItemInfo(Item item, ItemCustomData customData)
        {
            itemInfo.itemTitle.text = item.Title;

            string description = item.Description;
            if (description.RegexGetMany('{', '}', out string[] paths))
            {
                var json = customData.GetJson();
                foreach (var path in paths)
                {
                    JToken token = json.SelectToken(path);
                    if(token != null)
                    {
                        string replacment = token.ToString();
                        if (float.TryParse(replacment, out float result))
                        {
                            result = (float)Math.Round(result, 2);
                            replacment = result.ToString();
                        }

                        string word = "{" + path + "}";
                        description = description.Replace(word, replacment);
                    }
                }
            }

            description = description.Replace("\n", Environment.NewLine);
            itemInfo.itemDescription.text = description;
            itemInfo.infoPanel.SetActive(true);
        }

        public void HideItemInfo()
        {
            if (!contextShown) itemInfo.infoPanel.SetActive(false);
        }

        public void OnBlockerClicked()
        {
            ShowContextMenu(false);
            ShowInventoryPrompt(false, null);
            combinePartners.Clear();

            if (bindShortcut)
            {
                bindShortcut = false;
                activeItem = null;
            }
        }

        public void OnCloseInventory()
        {
            if (ContainerOpened)
            {
                foreach (var item in containerItems)
                {
                    Destroy(item.Key.gameObject);
                }

                if (containerSlots != null)
                {
                    foreach (var slot in containerSlots)
                    {
                        Destroy(slot.gameObject);
                    }
                }

                SetInventorySlots(currentContainer, false);

                containerSlots = new InventorySlot[0, 0];
                containerItems.Clear();

                containerSettings.containerObject.gameObject.SetActive(false);
                containerSettings.containerName.enabled = false;

                currentContainer.OnStorageClose();
                currentContainer = null;
            }

            foreach (var item in carryingItems)
            {
                item.Key.OnCloseInventory();
            }

            itemSelector = false;
            bindShortcut = false;

            inventorySelector = null;
            activeItem = null;

            combinePartners.Clear();
            gameManager.ShowControlsInfo(false, null);
            ShowInventoryPrompt(false, null, true);
            ShowContextMenu(false);
            HideItemInfo();
        }
        #endregion

        private InventoryItem CreateItem(string guid, int quantity, ItemCustomData customData)
        {
            Item item = items[guid];

            if (CheckSpace(item.Width, item.Height, out FreeSpace space))
            {
                InventoryItem inventoryItem = CreateItem(new ItemCreationData()
                {
                    itemGuid = guid,
                    quantity = (ushort)quantity,
                    orientation = space.orientation,
                    coords = new Vector2Int(space.x, space.y),
                    customData = customData,
                    parent = itemsTransform,
                    slotsSpace = slots
                });

                AddItemToFreeSpace(space, inventoryItem);
                return inventoryItem;
            }

            return null;
        }

        private InventoryItem CreateItem(ItemCreationData itemCreationData)
        {
            Item item = items[itemCreationData.itemGuid];

            GameObject itemGO = Instantiate(slotSettings.slotItemPrefab, itemCreationData.parent);
            RectTransform rect = itemGO.GetComponent<RectTransform>();
            InventoryItem inventoryItem = itemGO.GetComponent<InventoryItem>();

            if (itemCreationData.orientation == Orientation.Vertical)
                rect.localEulerAngles = new Vector3(0, 0, -90);

            float width = settings.cellSize * item.Width;
            width += item.Width > 1 ? settings.spacing * (item.Width - 1) : 0;

            float height = settings.cellSize * item.Height;
            height += item.Height > 1 ? settings.spacing * (item.Height - 1) : 0;

            rect.sizeDelta = new Vector2(width, height);
            rect.localScale = Vector3.one;

            Vector2Int coords = itemCreationData.coords;
            RectTransform slot = itemCreationData.slotsSpace[coords.y, coords.x].GetComponent<RectTransform>();

            Vector2 offset = inventoryItem.GetOrientationOffset();
            Vector2 position = new Vector2(slot.localPosition.x, slot.localPosition.y) + offset;
            rect.anchoredPosition = position;

            inventoryItem.SetItem(this, new InventoryItem.ItemData()
            {
                guid = itemCreationData.itemGuid,
                item = item,
                quantity = itemCreationData.quantity,
                orientation = itemCreationData.orientation,
                customData = itemCreationData.customData,
                slotSpace = itemCreationData.coords
            });

            return inventoryItem;
        }

        public StorableCollection OnCustomSave()
        {
            StorableCollection saveableBuffer = new StorableCollection();
            StorableCollection itemsToSave = new StorableCollection();
            StorableCollection shortcutsSave = new StorableCollection();

            int index = 0;
            foreach (var item in carryingItems)
            {
                itemsToSave.Add("item_" + index++, new StorableCollection()
                {
                    { "item", item.Key.ItemGuid },
                    { "quantity", item.Key.Quantity },
                    { "orientation", item.Key.orientation },
                    { "position", item.Key.Position.ToSaveable() },
                    { "customData", item.Key.CustomData?.GetJson() },
                });
            }

            shortcutsSave.Add("shortcut_0", shortcuts[0].item != null ? shortcuts[0].item.ItemGuid : "{}");
            shortcutsSave.Add("shortcut_1", shortcuts[1].item != null ? shortcuts[1].item.ItemGuid : "{}");
            shortcutsSave.Add("shortcut_2", shortcuts[2].item != null ? shortcuts[2].item.ItemGuid : "{}");
            shortcutsSave.Add("shortcut_3", shortcuts[3].item != null ? shortcuts[3].item.ItemGuid : "{}");

            saveableBuffer.Add("expanded", expandedSlots);
            saveableBuffer.Add("items", itemsToSave);
            saveableBuffer.Add("shortcuts", shortcutsSave);
            return saveableBuffer;
        }

        public void OnCustomLoad(JToken data)
        {
            int expandedRowsCount = (int)data["expanded"];
            if (expandedRowsCount > 0) ExpandInventory(expandedRowsCount, false);

            JObject items = (JObject)data["items"];

            foreach (var itemProp in items.Properties())
            {
                JToken token = itemProp.Value;

                string itemGuid = token["item"].ToString();
                int quantity = (int)token["quantity"];
                Orientation orientation = (Orientation)(int)token["orientation"];
                Vector2Int position = token["position"].ToObject<Vector2Int>();
                ItemCustomData customData = new ItemCustomData()
                {
                    JsonData = token["customData"].ToString()
                };

                InventoryItem inventoryItem = CreateItem(new ItemCreationData()
                {
                    itemGuid = itemGuid,
                    quantity = (ushort)quantity,
                    orientation = orientation,
                    coords = position,
                    customData = customData,
                    parent = itemsTransform,
                    slotsSpace = slots
                });

                AddItemToFreeSpace(new FreeSpace()
                {
                    x = position.x,
                    y = position.y,
                    orientation = orientation
                }, inventoryItem);
            }

            LoadShortcut(0, data["shortcuts"]["shortcut_0"].ToString());
            LoadShortcut(1, data["shortcuts"]["shortcut_1"].ToString());
            LoadShortcut(2, data["shortcuts"]["shortcut_2"].ToString());
            LoadShortcut(3, data["shortcuts"]["shortcut_3"].ToString());
        }

        private void LoadShortcut(int index, string itemGuid)
        {
            if (string.IsNullOrEmpty(itemGuid)) return;
            InventoryItem inventoryItem = GetInventoryItem(itemGuid);
            if(inventoryItem != null) SetShortcut(index, inventoryItem);
        }
    }
}