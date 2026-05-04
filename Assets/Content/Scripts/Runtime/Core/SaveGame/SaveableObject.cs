using System;
using UnityEngine;
using Newtonsoft.Json.Linq;

namespace UHFPS.Runtime
{
    public class SaveableObject : MonoBehaviour, ISaveable
    {
        [Flags]
        public enum SaveableFlagsEnum
        {        
            None = 0,
            Position = 1 << 0,
            Rotation = 1 << 1,
            Scale = 1 << 2,
            ObjectActive = 1 << 3,
            RendererActive = 1 << 4,
            ReferencesActive = 1 << 5
        }

        public SaveableFlagsEnum SaveableFlags;
        public MeshRenderer MeshRenderer;
        public Behaviour[] References;

        public StorableCollection OnSave()
        {
            StorableCollection storableCollection = new();

            if (SaveableFlags.HasFlag(SaveableFlagsEnum.Position))
            {
                storableCollection.Add("position", transform.position.ToSaveable());
            }

            if (SaveableFlags.HasFlag(SaveableFlagsEnum.Rotation))
            {
                storableCollection.Add("rotation", transform.eulerAngles.ToSaveable());
            }

            if (SaveableFlags.HasFlag(SaveableFlagsEnum.Scale))
            {
                storableCollection.Add("scale", transform.localScale.ToSaveable());
            }

            if (SaveableFlags.HasFlag(SaveableFlagsEnum.ObjectActive))
            {
                storableCollection.Add("objectActive", gameObject.activeSelf);
            }

            if (SaveableFlags.HasFlag(SaveableFlagsEnum.ObjectActive) && MeshRenderer != null)
            {
                storableCollection.Add("rendererEnabled", MeshRenderer.enabled);
            }

            if (SaveableFlags.HasFlag(SaveableFlagsEnum.ObjectActive) && References.Length > 0)
            {
                for (int i = 0; i < References.Length; i++)
                {
                    string name = "referenceId_" + i;
                    storableCollection.Add(name, References[i].enabled);
                }
            }

            return storableCollection;
        }

        public void OnLoad(JToken data)
        {
            if (SaveableFlags.HasFlag(SaveableFlagsEnum.Position))
            {
                Vector3 position = data["position"].ToObject<Vector3>();
                transform.position = position;
            }

            if (SaveableFlags.HasFlag(SaveableFlagsEnum.Rotation))
            {
                Vector3 rotation = data["rotation"].ToObject<Vector3>();
                transform.eulerAngles = rotation;
            }

            if (SaveableFlags.HasFlag(SaveableFlagsEnum.Scale))
            {
                Vector3 scale = data["scale"].ToObject<Vector3>();
                transform.localScale = scale;
            }

            if (SaveableFlags.HasFlag(SaveableFlagsEnum.ObjectActive))
            {
                bool active = (bool)data["objectActive"];
                gameObject.SetActive(active);
            }

            if (SaveableFlags.HasFlag(SaveableFlagsEnum.ObjectActive) && MeshRenderer != null)
            {
                bool active = (bool)data["rendererEnabled"];
                MeshRenderer.enabled = active;
            }

            if (SaveableFlags.HasFlag(SaveableFlagsEnum.ObjectActive) && References.Length > 0)
            {
                for (int i = 0; i < References.Length; i++)
                {
                    string name = "referenceId_" + i;
                    bool active = (bool)data[name];
                    References[i].enabled = active;
                }
            }
        }
    }
}