using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using ThunderWire.Attributes;

namespace UHFPS.Runtime
{
    [Summary("Tạo hiệu ứng hoạt ảnh cho Image UI bằng cách lặp qua một mảng các Sprites.")]
    public class SpritesheetAnimation : MonoBehaviour
    {
        [Tooltip("Spritesheet chứa các frame hoạt ảnh (không bắt buộc nếu đã gán mảng sprites).")]
        public Sprite Spritesheet;
        [Tooltip("Image UI sẽ hiển thị hoạt ảnh.")]
        public Image Image;
        [Tooltip("Số khung hình trên giây (FPS).")]
        public float FrameRate = 30f;
        [Tooltip("Tự động chạy hoạt ảnh khi bắt đầu.")]
        public bool PlayOnStart = true;

        [Tooltip("Mảng các Sprites chứa từng frame của hoạt ảnh.")]
        public Sprite[] sprites;
        private int currentSpriteIndex;

        private void Start()
        {
            if(PlayOnStart) StartCoroutine(AnimateSpriteSheet());
        }

        private IEnumerator AnimateSpriteSheet()
        {
            while (true)
            {
                Image.sprite = sprites[currentSpriteIndex];
                yield return new WaitForSeconds(1f / FrameRate);
                currentSpriteIndex = (currentSpriteIndex + 1) % sprites.Length;
            }
        }

        public void SetAnimationStatus(bool state)
        {
            if (state) StartCoroutine(AnimateSpriteSheet());
            else StopAllCoroutines();
        }
    }
}