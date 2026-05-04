using System;
using UnityEngine.Rendering;

namespace UHFPS.Runtime
{
    [Serializable]
    public struct VolumeComponentReferecne
    {
        public Volume Volume;
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