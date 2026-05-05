using UnityEngine;
using UHFPS.Tools;
using UnityEngine.Events;
using ThunderWire.Attributes;
using Newtonsoft.Json.Linq;

namespace UHFPS.Runtime
{
    [InspectorHeader("Simple Switcher")]
    [Summary("Một công tắc đơn giản (như công tắc điện gạt lên xuống), cho phép xoay một phần mô hình theo trục cụ thể khi tương tác.")]
    public class SimpleSwitcher : MonoBehaviour, IInteractStart, ISaveable
    {
        public enum SwitchTypeEnum { MoveTowards, SmoothDamp }

        [Tooltip("Trạng thái cho phép tương tác.")]
        public bool IsInteractable = true;

        [Tooltip("Kiểu xoay (MoveTowards: Đều đặn, SmoothDamp: Mượt dần).")]
        public SwitchTypeEnum SwitchType = SwitchTypeEnum.MoveTowards;

        [Tooltip("Trục không gian dùng để xoay công tắc.")]
        public Axis SwitchAxis;

        [Header("Settings")]
        [Tooltip("Góc (Angle) của công tắc khi ở trạng thái Tắt.")]
        public float SwitchOffAngle;

        [Tooltip("Góc (Angle) của công tắc khi ở trạng thái Bật.")]
        public float SwitchOnAngle;

        [Tooltip("Tốc độ chuyển đổi (Xoay) giữa hai trạng thái.")]
        public float SwitchSmoothSpeed;

        [Header("Sounds")]
        [Tooltip("Âm thanh phát ra khi bật.")]
        public SoundClip SwitchOn;

        [Tooltip("Âm thanh phát ra khi tắt.")]
        public SoundClip SwitchOff;

        [Header("Events")]
        [Tooltip("Sự kiện gọi ra khi công tắc bị thay đổi trạng thái (trả về bool).")]
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