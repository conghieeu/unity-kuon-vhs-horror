using System;
using UnityEngine;
using UHFPS.Tools;
using ThunderWire.Attributes;

namespace UHFPS.Runtime
{
    [RequireComponent(typeof(AudioSource))]
    [Summary("Phát âm thanh từ AudioSource thông qua việc nhận các Animation Event.")]
    public class AnimationSoundEvent : MonoBehaviour
    {
        [Serializable]
        public struct SoundEvent
        {
            [Tooltip("Tên sự kiện âm thanh (cần khớp với tham số trong Animation Event).")]
            public string Name;
            [Tooltip("Clip âm thanh sẽ được phát khi sự kiện này được gọi.")]
            public SoundClip Sound;
        }

        [Tooltip("Danh sách các cấu hình âm thanh để phát qua Animation Event.")]
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