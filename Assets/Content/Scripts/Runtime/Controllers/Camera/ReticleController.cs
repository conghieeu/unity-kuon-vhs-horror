using System;
using UnityEngine;
using UnityEngine.UI;
using ThunderWire.Attributes;
using Object = UnityEngine.Object;

namespace UHFPS.Runtime
{
    [Serializable]
    public sealed class Reticle
    {
        [Tooltip("Hình ảnh icon của con trỏ.")]
        public Sprite Sprite;
        [Tooltip("Màu sắc của con trỏ.")]
        public Color Color = Color.white;
        [Tooltip("Kích thước của con trỏ (width, height).")]
        public Vector2 Size = Vector2.one;
    }

    [InspectorHeader("Reticle Controller", space = false)]
    [RequireComponent(typeof(InteractController))]
    [Summary("Quản lý việc hiển thị và thay đổi con trỏ (Reticle/Crosshair) dựa trên đối tượng mà người chơi đang nhìn vào.")]
    public class ReticleController : MonoBehaviour
    {
        [Header("Interact")]
        [Tooltip("Con trỏ mặc định khi không nhìn vào vật thể nào.")]
        public Reticle DefaultReticle;
        [Tooltip("Con trỏ hiển thị khi có thể tương tác với vật thể.")]
        public Reticle InteractReticle;
        [Tooltip("Bật chế độ mượt mà khi thay đổi kích thước/màu sắc con trỏ.")]
        public bool DynamicReticle = true;
        [Tooltip("Thời gian làm mượt chuyển đổi con trỏ.")]
        public float ChangeTime = 0.05f;

        [Header("Custom Reticles")]
        [RequireInterface(typeof(IReticleProvider))]
        [Tooltip("Danh sách các nhà cung cấp con trỏ tùy chỉnh (thường là scriptable object hoặc script khác cung cấp IReticleProvider).")]
        public Object[] ReticleProviders;

        private InteractController interactController;
        private RectTransform crosshairRect;
        private Image crosshairImage;

        private CustomInteractReticle holdReticle;
        private Vector2 crosshairChangeVel;
        private bool resetReticle;

        private void Awake()
        {
            interactController = GetComponent<InteractController>();
            GameManager gameManager = GameManager.Instance;
            crosshairImage = gameManager.ReticleImage;
            crosshairRect = gameManager.ReticleImage.rectTransform;
        }

        private void Update()
        {
            if(interactController.RaycastObject != null || holdReticle != null)
            {
                GameObject raycastObject = interactController.RaycastObject;
                OnChangeReticle(raycastObject);
            }
            else
            {
                OnChangeReticle(null);
            }
        }

        public void ResetReticle()
        {
            OnChangeReticle(null);
            ChangeReticle(DefaultReticle);
        }

        private void OnChangeReticle(GameObject raycastObject)
        {
            CustomInteractReticle customReticle = null;
            if (raycastObject != null && raycastObject.TryGetComponent(out customReticle) || holdReticle != null)
            {
                CustomInteractReticle reticleProvider = holdReticle != null ? holdReticle : customReticle;

                IReticleProvider customProvider = holdReticle != null ? holdReticle : customReticle;
                var (_, reticle, hold) = customProvider.OnProvideReticle();

                if (hold) holdReticle = reticleProvider;
                else holdReticle = null;

                ChangeReticle(reticle);
                resetReticle = true;
                return;
            }

            bool customReticleFlag = false;
            foreach (var provider in ReticleProviders)
            {
                IReticleProvider reticleProvider = provider as IReticleProvider;
                var (targetType, reticle, hold) = reticleProvider.OnProvideReticle();

                if(targetType == null || reticle == null)
                    continue;

                if (raycastObject != null && raycastObject.TryGetComponent(targetType, out _) || hold)
                {
                    ChangeReticle(reticle);
                    customReticleFlag = true;
                    break;
                }
            }

            if (!customReticleFlag)
            {
                if (resetReticle)
                {
                    crosshairImage.color = Color.white;
                    crosshairRect.sizeDelta = DefaultReticle.Size;
                    resetReticle = false;
                }

                if(raycastObject != null)
                {
                    if (DynamicReticle)
                    {
                        crosshairImage.sprite = InteractReticle.Sprite;
                        crosshairImage.color = InteractReticle.Color;
                        crosshairRect.sizeDelta = Vector2.SmoothDamp(crosshairRect.sizeDelta, InteractReticle.Size, ref crosshairChangeVel, ChangeTime);
                    }
                    else
                    {
                        ChangeReticle(InteractReticle);
                    }
                }
                else
                {
                    if (DynamicReticle)
                    {
                        crosshairImage.sprite = DefaultReticle.Sprite;
                        crosshairImage.color = DefaultReticle.Color;
                        crosshairRect.sizeDelta = Vector2.SmoothDamp(crosshairRect.sizeDelta, DefaultReticle.Size, ref crosshairChangeVel, ChangeTime);
                    }
                    else
                    {
                        ChangeReticle(DefaultReticle);
                    }
                }
            }
            else
            {
                resetReticle = true;
            }
        }

        private void ChangeReticle(Reticle reticle)
        {
            if (reticle != null)
            {
                crosshairImage.sprite = reticle.Sprite;
                crosshairImage.color = reticle.Color;
                crosshairRect.sizeDelta = reticle.Size;
            }
            else
            {
                crosshairImage.sprite = null;
                crosshairImage.color = Color.white;
                crosshairRect.sizeDelta = Vector2.zero;
            }
        }
    }
}