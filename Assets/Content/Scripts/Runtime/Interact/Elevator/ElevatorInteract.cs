using Newtonsoft.Json.Linq;
using ThunderWire.Attributes;
using UHFPS.Tools;
using UnityEngine;

namespace UHFPS.Runtime
{
    [InspectorHeader("Elevator Interact")]
    [HelpBox("Place this script on the elevator floor button and it will send a signal to the parent elevator script instructing it to call the elevator to the current floor or which floor you want to go to.")]
    public class ElevatorInteract : MonoBehaviour, IInteractStart
    {
        public enum InteractTypeEnum { CallElevator, FloorSelect }
        private ElevatorInteract[] interacts;

        [Space]
        public ElevatorSystem ElevatorSystem;
        public InteractTypeEnum InteractType = InteractTypeEnum.CallElevator;

        [Header("Floor Setup")]
        public uint FloorLevel;

        [Header("Indicator Settings")]
        public string EmissionKeyword = "_EMISSION";
        public RendererMaterial IndicatorMaterial;

        [Header("Sound Settings")]
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