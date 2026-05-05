using UnityEngine;
using UHFPS.Tools;
using UnityEngine.Events;
using ThunderWire.Attributes;
using Newtonsoft.Json.Linq;

namespace UHFPS.Runtime
{
    [InspectorHeader("Simple Switcher")]
    public class SimpleSwitcher : MonoBehaviour, IInteractStart, ISaveable
    {
        public enum SwitchTypeEnum { MoveTowards, SmoothDamp }

        public bool IsInteractable = true;
        public SwitchTypeEnum SwitchType = SwitchTypeEnum.MoveTowards;
        public Axis SwitchAxis;

        [Header("Settings")]
        public float SwitchOffAngle;
        public float SwitchOnAngle;
        public float SwitchSmoothSpeed;

        [Header("Sounds")]
        public SoundClip SwitchOn;
        public SoundClip SwitchOff;

        [Header("Events")]
        public UnityEvent<bool> OnSwitch;

        private float velocity;
        private float targetAngle;
        private Vector3 currRotation;
        private bool isSwitchedOn;

        public bool IsSwitched => isSwitchedOn;

        private void Awake()
        {
            targetAngle = SwitchOffAngle;
            currRotation = transform.localEulerAngles;
            IsInteractable = true;
        }

        public void InteractStart()
        {
            if (!IsInteractable)
                return;

            isSwitchedOn = !isSwitchedOn;
            targetAngle = isSwitchedOn ? SwitchOnAngle : SwitchOffAngle;

            if (isSwitchedOn)
            {
                GameTools.PlayOneShot3D(transform.position, SwitchOn, "SwitchOn");
                OnSwitch?.Invoke(true);
            }
            else
            {
                GameTools.PlayOneShot3D(transform.position, SwitchOff, "SwitchOff");
                OnSwitch?.Invoke(false);
            }
        }

        private void Update()
        {
            float currAngle = currRotation.Component(SwitchAxis);

            if (SwitchType == SwitchTypeEnum.MoveTowards)
                currAngle = Mathf.MoveTowardsAngle(currAngle, targetAngle, Time.deltaTime * SwitchSmoothSpeed * 100);
            else
                currAngle = Mathf.SmoothDampAngle(currAngle, targetAngle, ref velocity, SwitchSmoothSpeed);

            currRotation = currRotation.SetComponent(SwitchAxis, currAngle);
            transform.localEulerAngles = currRotation;
        }

        public void SetSwitcherState(bool state)
        {
            isSwitchedOn = state;
            targetAngle = state ? SwitchOnAngle : SwitchOffAngle;

            currRotation = transform.localEulerAngles;
            currRotation = currRotation.SetComponent(SwitchAxis, targetAngle);
            transform.localEulerAngles = currRotation;
        }

        public StorableCollection OnSave()
        {
            return new StorableCollection()
            {
                { nameof(isSwitchedOn), isSwitchedOn }
            };
        }

        public void OnLoad(JToken data)
        {
            bool state = data[nameof(isSwitchedOn)].ToObject<bool>();
            SetSwitcherState(state);
        }
    }
}