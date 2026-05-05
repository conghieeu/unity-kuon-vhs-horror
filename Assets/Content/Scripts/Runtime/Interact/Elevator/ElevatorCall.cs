using ThunderWire.Attributes;
using UHFPS.Tools;
using UnityEngine;

namespace UHFPS.Runtime
{
    [InspectorHeader("Elevator Call")]
    [HelpBox("Place this script on the elevator call button. It will call the elevator, to move to the current floor.")]
    [Summary("Gắn trên nút gọi thang máy ngoài hành lang. Khi bấm sẽ gọi thang máy di chuyển tới tầng hiện tại.")]
    public class ElevatorCall : MonoBehaviour, IInteractStart
    {
        [Space]
        [Tooltip("Hệ thống thang máy đích mà nút bấm này sẽ tương tác.")]
        public ElevatorSystem ElevatorSystem;

        [Tooltip("Tầng hiện tại mà nút bấm này đang được đặt (Dùng để báo cho thang máy tới đón).")]
        public uint CurrentFloor;

        [Header("Indicator Settings")]
        [Tooltip("Tên tham số phát sáng trong Material (thường là _EMISSION).")]
        public string EmissionKeyword = "_EMISSION";

        [Tooltip("Material dùng để phát sáng nút bấm khi gọi thang máy.")]
        public RendererMaterial IndicatorMaterial;

        [Header("Sound Settings")]
        [Tooltip("Âm thanh phát ra khi nhấn nút.")]
        public SoundClip PressSound;

        public void InteractStart()
        {
            /*
            if (ElevatorSystem.CallElevator(this))
            {
                if(IndicatorMaterial.IsAssigned)
                    IndicatorMaterial.ClonedMaterial.EnableKeyword(EmissionKeyword);

                GameTools.PlayOneShot3D(transform.position, PressSound, "ButonPressSound");
            }
            */
        }

        public void DisableEmission()
        {
            if (IndicatorMaterial.IsAssigned)
                IndicatorMaterial.ClonedMaterial.DisableKeyword(EmissionKeyword);
        }
    }
}