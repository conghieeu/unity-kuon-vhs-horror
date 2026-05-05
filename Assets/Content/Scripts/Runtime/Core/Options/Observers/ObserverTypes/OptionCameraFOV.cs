using Unity.Cinemachine;
using System;
using UnityEngine;

using ThunderWire.Attributes;

namespace UHFPS.Runtime
{
    [Serializable]
    [Summary("Observer lắng nghe thay đổi Option FOV và tự động điều chỉnh Field of View của Cinemachine Camera.")]
    public class OptionCameraFOV : OptionObserverType
    {
        [Tooltip("Camera Cinemachine sẽ bị ảnh hưởng bởi tuỳ chọn FOV này.")]
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