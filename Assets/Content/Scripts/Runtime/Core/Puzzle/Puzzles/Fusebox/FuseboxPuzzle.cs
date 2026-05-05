using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Newtonsoft.Json.Linq;
using UHFPS.Tools;

using ThunderWire.Attributes;

namespace UHFPS.Runtime
{
    [RequireComponent(typeof(AudioSource))]
    [Summary("Hệ thống giải đố Cầu dao (Fusebox). Người chơi cần tìm vật phẩm cầu chì và cắm đủ vào các khe để nối mạch.")]
    public class FuseboxPuzzle : PuzzleBaseSimple, IInventorySelector, ISaveable
    {
        [Serializable]
        public sealed class FuseElement
        {
            public GameObject FuseObject;
            public Light FuseLight;
            public MeshRenderer LightRenderer;
            public bool IsInserted;
        }

        [Tooltip("Cầu chì (Item) mà người chơi cần phải thu thập để cắm vào cầu dao.")]
        public ItemProperty FuseItem;

        [Tooltip("Cắm trực tiếp khi người chơi bấm tương tác (nếu có đủ đồ trong túi) thay vì mở bảng chọn Item.")]
        public bool UseInteract = false;

        [Tooltip("Danh sách cấu hình các khe cắm cầu chì.")]
        public List<FuseElement> Fuses = new();

        [Tooltip("Tự động đổi màu đèn báo trạng thái khi có/chưa có cầu chì.")]
        public bool UseFuseColors = false;
        public string EmissionKeyword = "_EMISSION";
        public string EmissionColorName = "_EmissionColor";
        public string BaseColorName = "_BaseColor";

        [Tooltip("Màu đèn báo khi đã cắm cầu chì.")]
        public Color InsertedFuseColor = Color.white;

        [Tooltip("Màu đèn báo khi chưa cắm cầu chì.")]
        public Color NoFuseColor = Color.white;

        [Tooltip("Âm thanh phát ra khi cắm 1 cầu chì vào khe.")]
        public SoundClip FuseInsertSound;

        [Tooltip("Âm thanh phát ra khi đã cắm ĐỦ tất cả cầu chì.")]
        public SoundClip FusesConnectedSound;

        [Tooltip("Sự kiện gọi ra khi toàn bộ cầu chì được cắm thành công.")]
        public UnityEvent OnAllFusesConnected;

        [Tooltip("Sự kiện gọi ra khi 1 khe cắm nhận được cầu chì (truyền ra ID khe).")]
        public UnityEvent<int> OnFuseConnected;

        public bool FusesConnected => fusesConnected;
        private bool fusesConnected;

        private AudioSource audioSource;

        private void Awake()
        {
            audioSource = GetComponent<AudioSource>();
            if (!SaveGameManager.GameWillLoad)
            {
                foreach (var fuse in Fuses)
                {
                    InsertFuse(fuse, fuse.IsInserted);
                }
            }
        }

        public override void InteractStart()
        {
            if (fusesConnected) return;
            if(!UseInteract) Inventory.Instance.OpenItemSelector(this);
            else
            {
                InventoryItem fuseItem = Inventory.Instance.GetInventoryItem(FuseItem);
                if (fuseItem != null) InsertFuse(fuseItem);
            }
        }

        public void OnInventoryItemSelect(Inventory inventory, InventoryItem selectedItem)
        {
            if (selectedItem.ItemGuid != FuseItem)
                return;

            InsertFuse(selectedItem);
        }

        private void InsertFuse(InventoryItem fuseItem)
        {
            int quantity = fuseItem.Quantity;
            int inserted = 0;

            quantity = Math.Clamp(quantity, 0, Fuses.Count);
            audioSource.PlayOneShotSoundClip(FuseInsertSound);

            for (int i = 0; i < quantity; i++)
            {
                foreach (var fuse in Fuses)
                {
                    if (!fuse.IsInserted)
                    {
                        InsertFuse(fuse, true);
                        OnFuseConnected?.Invoke(Fuses.IndexOf(fuse));
                        inserted++;
                        break;
                    }
                }
            }

            Inventory.Instance.RemoveItem(fuseItem, (ushort)inserted);

            if(Fuses.All(x => x.IsInserted))
            {
                audioSource.PlayOneShotSoundClip(FusesConnectedSound);
                OnAllFusesConnected?.Invoke();
                fusesConnected = true;
                DisableInteract();
            }
        }

        private void InsertFuse(FuseElement fuse, bool connected)
        {
            fuse.FuseObject.SetActive(connected);
            fuse.IsInserted = connected;

            if (connected) fuse.LightRenderer.material.EnableKeyword(EmissionKeyword);
            else fuse.LightRenderer.material.DisableKeyword(EmissionKeyword);

            if (UseFuseColors)
            {
                Color fuseColor = connected ? InsertedFuseColor : NoFuseColor;
                fuse.LightRenderer.material.SetColor(EmissionColorName, fuseColor);
                fuse.LightRenderer.material.SetColor(BaseColorName, fuseColor);
                fuse.FuseLight.color = fuseColor;
                fuse.FuseLight.enabled = true;
            }
        }

        public StorableCollection OnSave()
        {
            StorableCollection saveableBuffer = new();
            for (int i = 0; i < Fuses.Count; i++)
            {
                saveableBuffer.Add("fuse_" + i, Fuses[i].IsInserted);
            }
            return saveableBuffer;
        }

        public void OnLoad(JToken data)
        {
            int inserted = 0;
            for (int i = 0; i < Fuses.Count; i++)
            {
                bool isInserted = (bool)data["fuse_" + i];
                if (isInserted) inserted++;
                InsertFuse(Fuses[i], isInserted);
            }

            if(inserted == Fuses.Count)
            {
                DisableInteract();
            }
        }
    }
}