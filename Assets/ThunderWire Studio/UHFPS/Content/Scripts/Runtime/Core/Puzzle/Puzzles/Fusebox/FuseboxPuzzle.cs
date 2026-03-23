using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Newtonsoft.Json.Linq;
using UHFPS.Tools;

namespace UHFPS.Runtime
{
    [RequireComponent(typeof(AudioSource))]
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

        public ItemProperty FuseItem;
        public bool UseInteract = false;

        public List<FuseElement> Fuses = new();

        public bool UseFuseColors = false;
        public string EmissionKeyword = "_EMISSION";
        public string EmissionColorName = "_EmissionColor";
        public string BaseColorName = "_BaseColor";
        public Color InsertedFuseColor = Color.white;
        public Color NoFuseColor = Color.white;

        public SoundClip FuseInsertSound;
        public SoundClip FusesConnectedSound;

        public UnityEvent OnAllFusesConnected;
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