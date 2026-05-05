using System;
using UnityEngine;

using ThunderWire.Attributes;

namespace UHFPS.Runtime
{
    [Serializable]
    [Summary("Observer lắng nghe thay đổi Option và tự động Bật/Tắt một hiệu ứng Volume (Post Processing) cụ thể.")]
    public class OptionVolumeActive : OptionObserverType
    {
        [Tooltip("Tham chiếu đến Component trong Volume Profile cần bật/tắt (Ví dụ: VHS, Chromatic Aberration).")]
        public VolumeComponentReferecne volumeComponent = new();

        public override string Name => "Volume Active";

        public override void OptionUpdate(object value)
        {
            if (value == null || volumeComponent.Volume == null)
                return;

            volumeComponent.SetVolumeComponentActive((bool)value);
        }
    }
}