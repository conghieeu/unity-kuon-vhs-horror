using UnityEngine;
using ThunderWire.Attributes;

namespace UHFPS.Runtime
{
    [InspectorHeader("Canvas Loot At Camera")]
    public class CanvasLootAtCamera : MonoBehaviour
    {
        public CanvasGroup CanvasGroup;
        public Vector2 ViewportSize = Vector2.one;

        [Header("Settings")]
        public float FadeDistance = 5f;
        public float SmoothTime = 1f;
        public bool InvertDirection;

        private Camera playerCamera;
        private float velocity;

        private void Awake()
        {
            playerCamera = PlayerPresenceManager.Instance.PlayerCamera;
            GameManager.SubscribePauseEvent(HideCanvas);
            GameManager.SubscribeInventoryEvent(HideCanvas);
        }

        private void HideCanvas(bool hidden)
        {
            gameObject.SetActive(!hidden);
        }

        private void Update()
        {
            Vector3 cameraPos = playerCamera.transform.position;
            Vector3 position = transform.position;
            float distance = Vector3.Distance(transform.position, cameraPos);
            bool fadeValue = false;

            if(distance < FadeDistance)
            {
                Vector3 screenPoint = playerCamera.WorldToViewportPoint(position);
                if (screenPoint.x >= 0 && screenPoint.x <= 1 && screenPoint.y >= 0 && screenPoint.y <= 1 && screenPoint.z > 0)
                {
                    float xMin = 1 - Remap(ViewportSize.x);
                    float xMax = Remap(ViewportSize.x);

                    float yMin = 1 - Remap(ViewportSize.y);
                    float yMax = Remap(ViewportSize.y);

                    fadeValue = screenPoint.x >= xMin && screenPoint.x <= xMax && screenPoint.y >= yMin && screenPoint.y <= yMax;
                }

                Vector3 forward = playerCamera.transform.position - transform.position;
                transform.forward = InvertDirection ? -forward : forward;
            }

            if(CanvasGroup != null)
            {
                float targetAlpha = fadeValue ? 1f : 0f;
                CanvasGroup.alpha = Mathf.SmoothDamp(CanvasGroup.alpha, targetAlpha, ref velocity, SmoothTime);
            }
        }

        private float Remap(float value)
        {
            return (value - 0) / (1 - 0) * (1 - 0.5f) + 0.5f;
        }
    }
}