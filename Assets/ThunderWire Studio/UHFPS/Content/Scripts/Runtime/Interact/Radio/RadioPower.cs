using UnityEngine;
using UHFPS.Tools;
using ThunderWire.Attributes;

namespace UHFPS.Runtime
{
    [InspectorHeader("Radio Power")]
    public class RadioPower : MonoBehaviour, IExamineDragVertical
    {
        public Transform Switch;

        [Header("Settings")]
        public Axis SwitchAxis;
        public MinMax SwitchLimits;
        public float SwitchSpeed;
        public float SwitchDelta;

        [Header("Sounds")]
        public SoundClip SwitchSound;

        private Radio radio;
        private bool isSwitched;
        private float currPos;

        private void Awake()
        {
            currPos = Switch.localPosition.Component(SwitchAxis);
            radio = GetComponentInParent<Radio>();
        }

        public void OnExamineDragVertical(float dragDelta)
        {
            if(!isSwitched && dragDelta >= SwitchDelta)
            {
                isSwitched = true;
                radio.SwitchRadio(true);
                GameTools.PlayOneShot3D(transform.position, SwitchSound, "Radio Switch");
            }
            else if(isSwitched && dragDelta <= -SwitchDelta)
            {
                isSwitched = false;
                radio.SwitchRadio(false);
                GameTools.PlayOneShot3D(transform.position, SwitchSound, "Radio Switch");
            }
        }

        private void Update()
        {
            Vector3 position = Switch.localPosition;
            float nextPos = isSwitched ? SwitchLimits.max : SwitchLimits.min;

            currPos = Mathf.MoveTowards(currPos, nextPos, Time.deltaTime * SwitchSpeed);
            position = position.SetComponent(SwitchAxis, currPos);
            Switch.localPosition = position;
        }
    }
}