using System;
using UnityEngine;

namespace UHFPS.Runtime
{
    [Serializable]
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