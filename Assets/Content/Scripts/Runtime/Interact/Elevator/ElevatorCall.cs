using ThunderWire.Attributes;
using UHFPS.Tools;
using UnityEngine;

namespace UHFPS.Runtime
{
    [InspectorHeader("Elevator Call")]
    [HelpBox("Place this script on the elevator call button. It will call the elevator, to move to the current floor.")]
    public class ElevatorCall : MonoBehaviour, IInteractStart
    {
        [Space]
        public ElevatorSystem ElevatorSystem;
        public uint CurrentFloor;

        [Header("Indicator Settings")]
        public string EmissionKeyword = "_EMISSION";
        public RendererMaterial IndicatorMaterial;

        [Header("Sound Settings")]
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