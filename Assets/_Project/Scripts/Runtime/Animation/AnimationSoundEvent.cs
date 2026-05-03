using System;
using UnityEngine;
using UHFPS.Tools;

namespace UHFPS.Runtime
{
    [RequireComponent(typeof(AudioSource))]
    public class AnimationSoundEvent : MonoBehaviour
    {
        [Serializable]
        public struct SoundEvent
        {
            public string Name;
            public SoundClip Sound;
        }

        public SoundEvent[] SoundEvents;
        private AudioSource audioSource;

        private void Awake()
        {
            audioSource = GetComponent<AudioSource>();
        }

        public void PlaySound(string name)
        {
            foreach (var sound in SoundEvents)
            {
                if(sound.Name == name)
                {
                    audioSource.PlayOneShotSoundClip(sound.Sound);
                    break;
                }
            }
        }
    }
}