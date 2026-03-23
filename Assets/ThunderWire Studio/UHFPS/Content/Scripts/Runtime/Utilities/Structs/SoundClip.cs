using System;
using UnityEngine;

namespace UHFPS.Runtime
{
    [Serializable]
    public sealed class SoundClip
    {
        public AudioClip audioClip;
        public float volume = 1f;

        public SoundClip(AudioClip audioClip, float volume = 1f)
        {
            this.audioClip = audioClip;
            this.volume = volume;
        }
    }
}