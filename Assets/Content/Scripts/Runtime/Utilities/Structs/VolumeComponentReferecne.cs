using System;
using UnityEngine;
using UnityEngine.Rendering;
using ThunderWire.Attributes;

namespace UHFPS.Runtime
{
    /// <summary>
    /// Tham chiếu đến một thành phần (Component) cụ thể trong Profile của Volume URP.
    /// </summary>
    [Serializable]
    public struct VolumeComponentReferecne
    {
        [Tooltip("Volume mục tiêu.")]
        public Volume Volume;
        [Tooltip("Chỉ số của Component trong danh sách Profile.")]
        public int ComponentIndex;

        public VolumeComponent GetVolumeComponent()
        {
            return Volume.profile.components[ComponentIndex] != null
                ? Volume.profile.components[ComponentIndex] : null;
        }

        public bool TryGetVolumeComponent(out VolumeComponent volumeComponent)
        {
            if (Volume != null && Volume.profile.components.Count > ComponentIndex)
            {
                volumeComponent = GetVolumeComponent();
                return true;
            }

            volumeComponent = null;
            return false;
        }

        public bool TryGetVolumeComponent<T>(out T volumeComponent) where T : VolumeComponent
        {
            if (Volume != null && Volume.profile.components.Count > ComponentIndex)
            {
                var component = GetVolumeComponent();
                if (component is T typedComponent)
                {
                    volumeComponent = typedComponent;
                    return true;
                }
            }

            volumeComponent = null;
            return false;
        }

        public void SetVolumeComponentActive(bool state)
        {
            if (TryGetVolumeComponent(out VolumeComponent volumeComponent))
                volumeComponent.active = state;
        }
    }
}