using System.Collections;
using UnityEngine;
using UHFPS.Rendering;

namespace UHFPS.Runtime
{
    public class RainingModule : ManagerModule
    {
        public override string Name => "Raining";

        private Raindrop raindrop;
        private Coroutine coroutine;

        public override void OnAwake()
        {
            raindrop = GameManager.GetStack<Raindrop>();
        }

        public void SetRaining(Raindrop raindrop, bool defaultState)
        {
            this.raindrop = raindrop;
            raindrop.Raining.value = defaultState ? 1f : 0f;
        }

        public void FadeRaindrop(bool fadeIn, float duration)
        {
            if (raindrop == null)
                return;

            if (coroutine != null) StopCoroutine(coroutine);
            coroutine = RunCoroutine(FadeRaining(fadeIn, duration));
        }

        IEnumerator FadeRaining(bool fadeIn, float duration)
        {
            float current = raindrop.Raining.value;
            float target = fadeIn ? 1f : 0f;
            float elapsedTime = 0;

            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                float t = elapsedTime / duration;

                raindrop.Raining.value = Mathf.Lerp(current, target, t);
                yield return null;
            }

            raindrop.Raining.value = target;
        }
    }
}