using Newtonsoft.Json.Linq;
using ThunderWire.Attributes;
using UHFPS.Tools;
using UnityEngine;

namespace UHFPS.Runtime
{
    [InspectorHeader("Elevator Interact")]
    [HelpBox("Place this script on the elevator floor button and it will send a signal to the parent elevator script instructing it to call the elevator to the current floor or which floor you want to go to.")]
    [Summary("Gắn trên các nút bấm (Bảng điều khiển) thang máy. Xử lý lệnh chọn tầng để đi đến hoặc gọi thang.")]
    public class ElevatorInteract : MonoBehaviour, IInteractStart
    {
        public enum InteractTypeEnum { CallElevator, FloorSelect }
        private ElevatorInteract[] interacts;

        [Space]
        [Tooltip("Hệ thống thang máy mục tiêu.")]
        public ElevatorSystem ElevatorSystem;

        [Tooltip("Loại nút (Gọi thang máy hoặc Nút chọn tầng bên trong thang máy).")]
        public InteractTypeEnum InteractType = InteractTypeEnum.CallElevator;

        [Header("Floor Setup")]
        [Tooltip("Tầng mục tiêu sẽ tới (hoặc tầng đang gọi nếu là nút gọi).")]
        public uint FloorLevel;

        [Header("Indicator Settings")]
        [Tooltip("Tên tham số phát sáng trong Material.")]
        public string EmissionKeyword = "_EMISSION";

        [Tooltip("Material phát sáng khi nút được nhấn.")]
        public RendererMaterial IndicatorMaterial;

        [Header("Sound Settings")]
        [Tooltip("Âm thanh khi bấm nút.")]
        public SoundClip PressSound;

        private void Awake()
        {
            if(InteractType == InteractTypeEnum.FloorSelect)
                interacts = transform.parent.GetComponentsInChildren<ElevatorInteract>();
        }

        public void InteractStart()
        {
            if (ElevatorSystem == null || ElevatorSystem.State == ElevatorSystem.ElevatorState.Moving)
                return;

            bool interact = true;
            if(InteractType == InteractTypeEnum.CallElevator)
            {
                interact = ElevatorSystem.CallElevator(this);
            }
            else if(ElevatorSystem.PlayerEntered)
            {
                DisableOtherEmissions();
                ElevatorSystem.MoveElevatorToLevel(this);
            }
            else
            {
                interact = false;
            }

            if (interact)
            {
                if (IndicatorMaterial.IsAssigned)
                    IndicatorMaterial.ClonedMaterial.EnableKeyword(EmissionKeyword);

                GameTools.PlayOneShot3D(transform.position, PressSound, "ButonPressSound");
            }
        }

        public void SetEmission(bool state)
        {
            if (!IndicatorMaterial.IsAssigned)
                return;

            if(state) IndicatorMaterial.ClonedMaterial.EnableKeyword(EmissionKeyword);
            else IndicatorMaterial.ClonedMaterial.DisableKeyword(EmissionKeyword);
        }

        public void DisableOtherEmissions()
        {
            foreach (var button in interacts)
            {
                button.SetEmission(false);
            }
        }
    }
}