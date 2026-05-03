using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace UHFPS.Runtime
{
    [System.Serializable]
    public class FloatingIconModule : ManagerModule
    {
        public struct ObjectIconPair
        {
            public GameObject gameObject;
            public FloatingIconObject floatingIcon;

            public ObjectIconPair(GameObject obj)
            {
                gameObject = obj;
                obj.TryGetComponent(out floatingIcon);
            }
        }

        public sealed class FloatingIconData
        {
            public FloatingIcon floatingIcon;
            public FloatingIconObject targetIcon;
            public Transform iconTranform;
            public GameObject targetObject;
            public Vector3 lastPosition;
            public bool wasDisabled;

            public void UpdateLastPosition()
            {
                if(targetObject != null)
                    lastPosition = targetObject.transform.position;
            }
        }

        public override string Name => "Floating Icon";

        public GameObject FloatingIconPrefab;

        [Header("Settings")]
        public LayerMask CullLayers;
        public float DistanceShow = 4;
        public float DistanceHide = 4;
        public float FadeInTime = 0.2f;
        public float FadeOutTime = 0.05f;

        private readonly List<FloatingIconData> uiFloatingIcons = new List<FloatingIconData>();
        private List<ObjectIconPair> worldFloatingIcons = new List<ObjectIconPair>();

        public override void OnAwake()
        {
            worldFloatingIcons = (from interactable in Object.FindObjectsByType<InteractableItem>(FindObjectsSortMode.None)
                                 where interactable.ShowFloatingIcon
                                 select new ObjectIconPair(interactable.gameObject)).ToList();

            foreach (var item in Object.FindObjectsByType<FloatingIconObject>(FindObjectsSortMode.None))
            {
                worldFloatingIcons.Add(new ObjectIconPair()
                {
                    gameObject = item.gameObject,
                    floatingIcon = item
                });
            }
        }

        /// <summary>
        /// Add object to floating icons list.
        /// </summary>
        public void AddFloatingIcon(GameObject gameObject)
        {
            worldFloatingIcons.Add(new ObjectIconPair(gameObject));
        }

        /// <summary>
        /// Remove object from floating icons list.
        /// </summary>
        public void RemoveFloatingIcon(GameObject gameObject)
        {
            worldFloatingIcons.RemoveAll(x => x.gameObject == gameObject);
        }

        public override void OnUpdate()
        {
            for (int i = 0; i < worldFloatingIcons.Count; i++)
            {
                ObjectIconPair pair = worldFloatingIcons[i];
                FloatingIconObject floatingIcon = pair.floatingIcon;
                GameObject obj = pair.gameObject;

                if(obj == null)
                {
                    worldFloatingIcons.RemoveAt(i);
                    continue;
                }

                LayerMask cullLayers = CullLayers;
                float distanceShow = DistanceShow;

                if (floatingIcon != null && floatingIcon.Override && floatingIcon.OverrideCulling)
                {
                    cullLayers = floatingIcon.CullLayers;
                    distanceShow = floatingIcon.DistanceShow;
                }

                if (Vector3.Distance(PlayerPresence.PlayerCamera.transform.position, obj.transform.position) <= distanceShow)
                {
                    if (!uiFloatingIcons.Any(x => x.targetObject == obj) && VisibleByCamera(obj, cullLayers) && IsIconUpdatable(obj))
                    {
                        Vector3 screenPoint = PlayerPresence.PlayerCamera.WorldToScreenPoint(obj.transform.position);
                        GameObject floatingIconObj = Object.Instantiate(FloatingIconPrefab, screenPoint, Quaternion.identity, GameManager.FloatingIcons);

                        FloatingIcon icon = floatingIconObj.AddComponent<FloatingIcon>();
                        if (floatingIcon != null && floatingIcon.Override)
                                icon.SetSprite(floatingIcon.CustomIcon, floatingIcon.IconSize);

                        uiFloatingIcons.Add(new FloatingIconData()
                        {
                            floatingIcon = icon,
                            targetIcon = floatingIcon,
                            iconTranform = floatingIconObj.transform,
                            targetObject = obj,
                            lastPosition = obj.transform.position
                        });

                        icon.FadeIn(FadeInTime);
                    }
                }
            }

            for (int i = 0; i < uiFloatingIcons.Count; i++)
            {
                FloatingIconData item = uiFloatingIcons[i];
                FloatingIconObject obj = item.targetIcon;

                LayerMask cullLayers = CullLayers;
                float distanceHide = DistanceHide;

                if (obj != null && obj.Override && obj.OverrideCulling)
                {
                    cullLayers = obj.CullLayers;
                    distanceHide = obj.DistanceHide;
                }

                if (item.iconTranform == null)
                {
                    uiFloatingIcons.RemoveAt(i);
                    continue;
                }

                if (IsIconUpdatable(item.targetObject))
                {
                    // update last object position
                    item.UpdateLastPosition();

                    // update distance
                    float distance = Vector3.Distance(PlayerPresence.PlayerCamera.transform.position, item.lastPosition);

                    // set point position
                    Vector3 screenPoint = PlayerPresence.PlayerCamera.WorldToScreenPoint(item.lastPosition);
                    item.iconTranform.position = screenPoint;

                    if (item.targetObject == null)
                    {
                        // destroy the floating icon if the target object is removed
                        Object.Destroy(item.iconTranform.gameObject);
                        uiFloatingIcons.RemoveAt(i);
                    }
                    else if (distance > distanceHide)
                    {
                        // destroy and remove the item if it is out of distance
                        item.floatingIcon.FadeOut(FadeOutTime);
                    }
                    else if (!VisibleByCamera(item.targetObject, cullLayers))
                    {
                        // disable an item if it is behind an object
                        item.iconTranform.gameObject.SetActive(false);
                        item.wasDisabled = true;
                    }
                    else if (item.wasDisabled)
                    {
                        // enable an object if it is visible when it has been disabled
                        item.floatingIcon.FadeIn(FadeInTime);
                        item.iconTranform.gameObject.SetActive(true);
                        item.wasDisabled = false;
                    }
                }
                else
                {
                    // destroy the floating icon if the target object is disabled
                    Object.Destroy(item.iconTranform.gameObject);
                    uiFloatingIcons.RemoveAt(i);
                }
            }
        }

        private bool VisibleByCamera(GameObject obj, LayerMask cullLayers)
        {
            if (obj != null)
            {
                bool linecastResult = Physics.Linecast(PlayerPresence.PlayerCamera.transform.position, obj.transform.position, out RaycastHit hit, cullLayers);

                if (!linecastResult || linecastResult && hit.collider.gameObject == obj)
                {
                    Vector3 screenPoint = PlayerPresence.PlayerCamera.WorldToViewportPoint(obj.transform.position);
                    return screenPoint.x >= 0 && screenPoint.x <= 1 && screenPoint.y >= 0 && screenPoint.y <= 1 && screenPoint.z > 0;
                }
            }

            return false;
        }

        private bool IsIconUpdatable(GameObject targetObj)
        {
            return targetObj != null && (targetObj.activeSelf || targetObj.activeSelf && targetObj.TryGetComponent(out Renderer renderer) && renderer.enabled);
        }
    }
}