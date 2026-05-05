using UnityEngine;
using UnityEngine.UI;

namespace UHFPS.Runtime
{
    public class FloatingIcon : MonoBehaviour
    {
        private float fadeTime;
        private Image iconImage;

        private float targetFade = -1f;
        private float fadeVelocity;

        public Image IconImage
        {
            get
            {
                if (iconImage == null)
                    iconImage = GetComponent<Image>();

                return iconImage;
            }
        }

        private void Awake()
        {
            targetFade = -1f;
        }

        public void SetSprite(Sprite sprite, Vector2 size)
        {
            if (!IconImage)
                return;

            IconImage.sprite = sprite;
            IconImage.rectTransform.sizeDelta = size;
        }

        public void FadeIn(float fadeTime)
        {
            if (!IconImage) 
                return;

            this.fadeTime = fadeTime;
            targetFade = 1f;

            Color color = IconImage.color;
            color.a = 0f;
            IconImage.color = color;
        }

        public void FadeOut(float fadeTime)
        {
            this.fadeTime = fadeTime;
            targetFade = 0f;
        }

        private void Update()
        {
            if (targetFade >= 0f && IconImage)
            {
                Color color = IconImage.color;
                color.a = Mathf.SmoothDamp(color.a, targetFade, ref fadeVelocity, fadeTime);
                IconImage.color = color;

                if (color.a < 0.01f && targetFade == 0f)
                    Destroy(gameObject);
            }
        }
    }
}