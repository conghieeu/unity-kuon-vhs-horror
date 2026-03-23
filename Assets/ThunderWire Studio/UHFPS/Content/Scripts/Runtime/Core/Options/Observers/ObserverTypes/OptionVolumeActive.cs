using System;

namespace UHFPS.Runtime
{
    [Serializable]
    public class OptionVolumeActive : OptionObserverType
    {
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