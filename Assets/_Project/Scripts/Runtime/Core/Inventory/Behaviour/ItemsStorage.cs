using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UHFPS.Tools;
using Newtonsoft.Json.Linq;

namespace UHFPS.Runtime
{
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

        public List<StorageItem> StoredItems = new();

        [Tooltip("Interact with the container after holding the use button for a certain amount of time.")]
        public bool TimedOpen;
        [Tooltip("Instantly interact with the container after searching the container once.")]
        public bool KeepSearched;
        [Tooltip("Automatically arrange the stored items in the container.")]
        public bool AutoCoords;

        [field: SerializeField]
        public float InteractTime { get; set; }

        public AudioSource AudioSource;
        public SoundClip SearchingSound;
        public SoundClip OpenStorageSound;
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