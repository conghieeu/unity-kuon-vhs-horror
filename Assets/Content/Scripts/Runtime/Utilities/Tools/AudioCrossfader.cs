using System.Collections;
using UnityEngine;
using UHFPS.Runtime;

namespace UHFPS.Tools
{
    /// <summary>
    /// A simple component that allows the cross-over of two audio clips. 
    /// Requires two Audio Sources for crossfade or one for fade In/Out.
    /// </summary>
    public sealed class AudioCrossfader
    {
        private readonly AudioSource audioSourceA;
        private readonly AudioSource audioSourceB;

        public bool IsTransitioning { get; private set; }

        public AudioCrossfader(AudioSource audioSourceA, AudioSource audioSourceB) 
        {
            this.audioSourceA = audioSourceA;
            this.audioSourceB = audioSourceB;
        }

        public AudioCrossfader(AudioSource audioSourceA)
        {
            this.audioSourceA = audioSourceA;
        }

        public IEnumerator CrossfadeAB(SoundClip fromA, SoundClip toB, float blendTime, bool toIsLoop = true)
        {
            if (audioSourceA == null || audioSourceB == null)
                yield break;

            IsTransitioning = true;

            audioSourceA.clip = fromA.audioClip;
            audioSourceA.volume = fromA.volume;
            audioSourceA.loop = false;
            audioSourceA.Play();

            audioSourceB.clip = toB.audioClip;
            audioSourceB.volume = 0f;
            audioSourceB.loop = toIsLoop;
            audioSourceB.Stop();

            float timeToStartFade = fromA.audioClip.length - blendTime;
            float elapsedTime = 0f;

            while (elapsedTime < timeToStartFade)
            {
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            float crossfadeStartTime = Time.time;
            while (Time.time - crossfadeStartTime <= blendTime)
            {
                float t = (Time.time - crossfadeStartTime) / blendTime;
                audioSourceA.volume = Mathf.Lerp(fromA.volume, 0, t);
                audioSourceB.volume = Mathf.Lerp(0, toB.volume, t);

                // Start audioSourceB playing if it hasn't started already.
                if (!audioSourceB.isPlaying)
                    audioSourceB.Play();

                yield return null;
            }

            audioSourceA.Stop();
            audioSourceA.clip = null;
            audioSourceA.volume = 0f;

            audioSourceB.volume = toB.volume;
            IsTransitioning = false;
        }

        public IEnumerator CrossfadeA(SoundClip toA, float blendTime, bool toIsLoop = false)
        {
            if (audioSourceA == null || audioSourceB == null)
                yield break;

            IsTransitioning = true;

            audioSourceA.clip = toA.audioClip;
            audioSourceA.volume = toA.volume;
            audioSourceA.loop = toIsLoop;
            audioSourceA.Play();

            float fromVolume = audioSourceB.volume;
            float time = 0;

            while (time < blendTime)
            {
                audioSourceB.volume = Mathf.Lerp(fromVolume, 0f, time / blendTime);
                audioSourceA.volume = Mathf.Lerp(0f, toA.volume, time / blendTime);
                time += Time.deltaTime;
                yield return null;
            }

            audioSourceB.Stop();
            audioSourceB.clip = null;
            audioSourceB.loop = false;
            audioSourceB.volume = 0f;

            audioSourceA.volume = toA.volume;
            IsTransitioning = false;
        }

        public IEnumerator CrossfadeB(SoundClip toB, float blendTime, bool toIsLoop = false)
        {
            if (audioSourceA == null || audioSourceB == null)
                yield break;

            IsTransitioning = true;

            audioSourceB.clip = toB.audioClip;
            audioSourceB.volume = toB.volume;
            audioSourceB.loop = toIsLoop;
            audioSourceB.Play();

            float fromVolume = audioSourceA.volume;
            float time = 0;

            while (time < blendTime)
            {
                audioSourceA.volume = Mathf.Lerp(fromVolume, 0f, time / blendTime);
                audioSourceB.volume = Mathf.Lerp(0f, toB.volume, time / blendTime);
                time += Time.deltaTime;
                yield return null;
            }

            audioSourceA.Stop();
            audioSourceA.clip = null;
            audioSourceA.loop = false;
            audioSourceA.volume = 0f;

            audioSourceB.volume = toB.volume;
            IsTransitioning = false;
        }

        public IEnumerator FadeIn(SoundClip clip, float fadeTime, bool toLoop = false)
        {
            if (audioSourceA == null)
                yield break;

            IsTransitioning = true;

            audioSourceA.clip = clip.audioClip;
            audioSourceA.volume = 0f;
            audioSourceA.loop = toLoop;
            audioSourceA.Play();

            float elapsedTime = 0f;
            while(elapsedTime < fadeTime)
            {
                audioSourceA.volume = Mathf.Lerp(0f, clip.volume, elapsedTime / fadeTime);
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            audioSourceA.volume = clip.volume;

            IsTransitioning = false;
        }

        public IEnumerator FadeOut(float fadeTime)
        {
            if (audioSourceA == null)
                yield break;

            IsTransitioning = true;

            float volume = audioSourceA.volume;
            float elapsedTime = 0f;

            while (elapsedTime < fadeTime)
            {
                audioSourceA.volume = Mathf.Lerp(volume, 0f, elapsedTime / fadeTime);
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            audioSourceA.Stop();
            audioSourceA.volume = 0f;

            IsTransitioning = false;
        }
    }
}