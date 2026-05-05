using UnityEngine;
using UHFPS.Tools;
using ThunderWire.Attributes;

namespace UHFPS.Runtime
{
    [InspectorHeader("Radio Tuner")]
    [Summary("Quản lý núm xoay/vặn (theo trục ngang) để thay đổi tần số (Tuner) của đài Radio.")]
    public class RadioTuner : MonoBehaviour, IExamineDragHorizontal
    {
        [Tooltip("Trục quay của núm xoay.")]
        public Axis RotateAxis;

        [Tooltip("Giới hạn góc vặn tối đa của núm.")]
        public MinMax RotateLimits;

        [Tooltip("Tỷ lệ quy đổi từ chuột sang góc quay.")]
        public float RotateAmount;

        [Tooltip("Tốc độ quay tối đa (Giới hạn lực kéo của chuột).")]
        public float MaxRotateSpeed;

        [Tooltip("Đảo ngược hướng quay so với hướng chuột.")]
        public bool FlipMouse;

        private Radio radio;
        private float currAngle;

        public float TunerAngle
        {
            get => currAngle;
            set
            {
                currAngle = value;
                Vector3 rotation = transform.localEulerAngles;
                rotation = rotation.SetComponent(RotateAxis, currAngle);
                transform.localEulerAngles = rotation;

                float t = Mathf.InverseLerp(RotateLimits.RealMin, RotateLimits.RealMax, currAngle);
                radio.UpdateTuner(t);
            }
        }

        private void Awake()
        {
            radio = GetComponentInParent<Radio>();
        }

        public void OnExamineDragHorizontal(float dragDelta)
        {
            Vector3 rotation = transform.localEulerAngles;
            dragDelta = Mathf.Clamp(dragDelta, -MaxRotateSpeed, MaxRotateSpeed);
            dragDelta = FlipMouse ? -dragDelta : dragDelta;

            currAngle = rotation.Component(RotateAxis);
            currAngle = Mathf.Clamp(currAngle + dragDelta * RotateAmount, RotateLimits.RealMin, RotateLimits.RealMax);
            rotation = rotation.SetComponent(RotateAxis, currAngle);

            transform.localEulerAngles = rotation;

            float t = Mathf.InverseLerp(RotateLimits.RealMin, RotateLimits.RealMax, currAngle);
            radio.UpdateTuner(t);
        }
    }
}