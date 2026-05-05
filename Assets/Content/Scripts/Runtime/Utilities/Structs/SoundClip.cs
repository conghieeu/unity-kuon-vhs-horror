using System;
using UnityEngine;
using ThunderWire.Attributes;

namespace UHFPS.Runtime
{
    /// <summary>
    /// Cấu trúc lưu trữ một AudioClip kèm theo âm lượng tùy chỉnh.
    /// </summary>
    [Serializable]
    [Summary("Tệp âm thanh kèm âm lượng.")]
    public sealed class SoundClip
    {
        [Tooltip("Tệp âm thanh.")]
        public AudioClip audioClip;
        [Tooltip("Âm lượng (0-1).")]
        public float volume = 1f;

        public SoundClip(AudioClip audioClip, float volume = 1f)
        {
            this.audioClip = audioClip;
            this.volume = volume;
        }
    }
}