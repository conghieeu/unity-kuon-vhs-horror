using Unity.Cinemachine;
using System;

namespace UHFPS.Runtime
{
    [Serializable]
    public class OptionCameraFOV : OptionObserverType
    {
        public CinemachineCamera VirtualCamera;

        public override string Name => "Camera FOV";

        public override void OptionUpdate(object value)
        {
            if (value == null || VirtualCamera == null)
                return;

            VirtualCamera.Lens.FieldOfView = Convert.ToInt32(value);
        }
    }
}