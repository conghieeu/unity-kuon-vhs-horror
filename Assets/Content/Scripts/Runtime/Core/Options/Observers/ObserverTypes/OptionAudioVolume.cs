using System;
using UnityEngine;

using ThunderWire.Attributes;

namespace UHFPS.Runtime
{
    [Serializable]
    [Summary("Observer lắng nghe thay đổi Option âm lượng và tự động điều chỉnh Volume của AudioSource cụ thể.")]
    public class OptionAudioVolume : OptionObserverType
    {
        [Tooltip("Nguồn âm thanh sẽ bị ảnh hưởng bởi tuỳ chọn Volume này.")]
        public AudioSource AudioSource;
        private float _audioSourceVolume;

        public override string Name => "Audio Volume";

        public override void OnStart()
        {
            _audioSourceVolume = AudioSource.volume;
        }

        public override void OptionUpdate(object value)
        {
            if (value == null || AudioSource == null)
                return;

            AudioSource.volume = _audioSourceVolume * (float)value;
        }
    }
}