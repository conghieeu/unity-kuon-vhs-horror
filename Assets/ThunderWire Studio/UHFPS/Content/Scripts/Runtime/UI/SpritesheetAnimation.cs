using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace UHFPS.Runtime
{
    public class SpritesheetAnimation : MonoBehaviour
    {
        public Sprite Spritesheet;
        public Image Image;
        public float FrameRate = 30f;
        public bool PlayOnStart = true;

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