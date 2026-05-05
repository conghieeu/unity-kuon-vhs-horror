using System;
using UnityEngine;

using ThunderWire.Attributes;

namespace UHFPS.Runtime
{
    [Serializable]
    [Summary("Observer lắng nghe thay đổi Option âm lượng và tự động điều chỉnh Volume tổng của AudioListener.")]
    public class OptionListenerVolume : OptionObserverType
    {
        public override string Name => "Audio Listener Volume";

        public override void OptionUpdate(object value)
        {
            if (value == null)
                return;

            AudioListener.volume = (float)value;
        }
    }
}