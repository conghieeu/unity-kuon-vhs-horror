using UnityEngine;
using UHFPS.Tools;
using ThunderWire.Attributes;

namespace UHFPS.Runtime
{
    [InspectorHeader("Radio Tuner")]
    public class RadioTuner : MonoBehaviour, IExamineDragHorizontal
    {
        public Axis RotateAxis;
        public MinMax RotateLimits;
        public float RotateAmount;
        public float MaxRotateSpeed;
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