using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UHFPS.Tools;
using Newtonsoft.Json.Linq;

using ThunderWire.Attributes;

namespace UHFPS.Runtime
{
    [Summary("Quản lý hộp lưu trữ đồ (Rương/Storage), nơi người chơi có thể cất và lấy vật phẩm.")]
    public class ItemsStorage : InventoryContainer, IInteractTimed, IInteractStart, IInteractStop
    {
        [Serializable]
        public struct StorageItem
        {
            public ItemGuid Item;
            public uint Quantity;
            public Vector2Int Coords;
            public ItemCustomData ItemData;
        }

        [Tooltip("Danh sách vật phẩm có sẵn trong Rương khi mới bắt đầu.")]
        public List<StorageItem> StoredItems = new();

        [Tooltip("Giữ nút tương tác một khoảng thời gian để mở hộp (Ví dụ: Hành động lục soát).")]
        public bool TimedOpen;
        [Tooltip("Sau khi lục soát lần đầu, hộp sẽ có thể mở ngay lập tức ở các lần sau.")]
        public bool KeepSearched;
        [Tooltip("Tự động sắp xếp vị trí của các vật phẩm có sẵn trong rương.")]
        public bool AutoCoords;

        [field: SerializeField]
        [Tooltip("Thời gian cần giữ nút để lục soát rương (Nếu TimedOpen = true).")]
        public float InteractTime { get; set; }

        public AudioSource AudioSource;
        [Tooltip("Âm thanh khi đang lục soát rương.")]
        public SoundClip SearchingSound;
        [Tooltip("Âm thanh khi mở rương.")]
        public SoundClip OpenStorageSound;
        [Tooltip("Âm thanh khi đóng rương.")]
        public SoundClip CloseStorageSound;

        public UnityEvent OnStartSearch;
        public UnityEvent OnOpenStorage;
        public UnityEvent OnCloseStorage;

        public bool NoInteract => isSearched || !TimedOpen;
        private bool isSearched;

        private void Start()
        {
            ContainerTitle.SubscribeGloc();

            if (!SaveGameManager.GameWillLoad)
            {
                foreach (var storedItem in StoredItems)
                {
                    string containerGuid = GameTools.GetGuid();
                    Item item = storedItem.Item.GetItem();
                    ushort width = item.Width;
                    ushort height = item.Height;

                    if (!AutoCoords)
                    {
                        ContainerItems.Add(containerGuid, new()
                        {
                            ItemGuid = storedItem.Item.GUID,
                            Item = item,
                            Quantity = (int)storedItem.Quantity,
                            Orientation = Orientation.Horizontal,
                            CustomData = storedItem.ItemData,
                            Coords = storedItem.Coords
                        });
                    }
                    else if(CheckSpace(width, height, out var freeSpace))
                    {
                        ContainerItems.Add(containerGuid, new()
                        {
                            ItemGuid = storedItem.Item.GUID,
                            Item = item,
                            Quantity = (int)storedItem.Quantity,
                            Orientation = freeSpace.orientation,
                            CustomData = storedItem.ItemData,
                            Coords = new(freeSpace.x, freeSpace.y)
                        });
                    }
                    else
                    {
                        Debug.LogError($"There is no space in the container for the item '{item.Title}'!");
                    }
                }
            }
        }

        public void InteractTimed()
        {
            if (!TimedOpen)
                return;

            OpenInventoryContainer();
            isSearched = KeepSearched;
        }

        public void InteractStart()
        {
            if (!isSearched && TimedOpen)
            {
                AudioSource.SetSoundClip(SearchingSound, play: true);
                OnStartSearch?.Invoke();
                return;
            }

            OpenInventoryContainer();
        }

        public void InteractStop()
        {
            if (TimedOpen && AudioSource != null)
                AudioSource.Stop();
        }

        private void OpenInventoryContainer()
        {
            if (TimedOpen && AudioSource != null)
                AudioSource.Stop();

            inventory.OpenContainer(this);
            AudioSource.PlayOneShotSoundClip(OpenStorageSound);
            OnOpenStorage?.Invoke();
        }

        public override void OnStorageClose()
        {
            AudioSource.PlayOneShotSoundClip(CloseStorageSound);
            OnCloseStorage?.Invoke();
        }

        public override StorableCollection OnSave()
        {
            StorableCollection saveableBuffer = new()
            {
                { "items", base.OnSave() },
                { nameof(isSearched), isSearched }
            };
            return saveableBuffer;
        }

        public override void OnLoad(JToken data)
        {
            base.OnLoad(data["items"]);
            isSearched = (bool)data[nameof(isSearched)];
        }
    }
}