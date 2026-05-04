using System.Collections.Generic;
using UnityEngine;

namespace UHFPS.Runtime
{
    public class InteractIconModule : ManagerModule
    {
        public struct IconUpdateCache
        {
            public InteractIconObject Component;
            public FloatingIcon Icon;
        }

        public override string Name => "Interact Icon";

        public GameObject InteractIconPrefab;
        public float FadeInTime = 0.2f;
        public float FadeOutTime = 0.05f;

        private readonly Dictionary<InteractIconObject, FloatingIcon> floatingIcons = new();
        private readonly List<IconUpdateCache> updateCache = new();

        public void ShowInteractIcon(InteractIconObject obj)
        {
            if (floatingIcons.ContainsKey(obj))
                return;

            Vector3 screenPoint = PlayerPresence.PlayerCamera.WorldToScreenPoint(obj.IconPosition);
            GameObject iconObject = Object.Instantiate(InteractIconPrefab, screenPoint, Quaternion.identity, GameManager.FloatingIcons);

            FloatingIcon floatingIcon = iconObject.AddComponent<FloatingIcon>();
            floatingIcon.SetSprite(obj.HoverIcon, obj.HoverSize);
            floatingIcon.FadeIn(FadeInTime);

            floatingIcons.Add(obj, floatingIcon);
            updateCache.Add(new IconUpdateCache()
            {
                Component = obj,
                Icon = floatingIcon
            });
        }

        public void DestroyInteractIcon(InteractIconObject obj)
        {
            if (!floatingIcons.TryGetValue(obj, out FloatingIcon icon))
                return;

            icon.FadeOut(FadeOutTime);
            floatingIcons.Remove(obj);
        }

        public void SetIconToHover(InteractIconObject obj)
        {
            if (!floatingIcons.TryGetValue(obj, out FloatingIcon icon))
                return;

            icon.SetSprite(obj.HoverIcon, obj.HoverSize);
        }

        public void SetIconToHold(InteractIconObject obj)
        {
            if (!floatingIcons.TryGetValue(obj, out FloatingIcon icon))
                return;

            icon.SetSprite(obj.HoldIcon, obj.HoldSize);
        }

        public override void OnUpdate()
        {
            for (int i = 0; i < updateCache.Count; i++)
            {
                IconUpdateCache cache = updateCache[i];

                // clear cache if icon is faded out
                if(cache.Icon == null)
                {
                    updateCache.RemoveAt(i);
                    break;
                }

                Vector3 screenPoint = PlayerPresence.PlayerCamera.WorldToScreenPoint(cache.Component.IconPosition);
                cache.Icon.transform.position = screenPoint;
            }
        }
    }
}