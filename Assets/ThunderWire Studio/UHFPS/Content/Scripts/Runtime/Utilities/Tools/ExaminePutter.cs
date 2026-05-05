using UHFPS.Tools;
using UnityEngine;

namespace UHFPS.Runtime
{
    public class ExaminePutter : MonoBehaviour
    {
        public sealed class PutCurve
        {
            private readonly AnimationCurve curve;

            public float EvalMultiply { get; set; } = 1f;
            public float CurveTime { get; set; } = 0.1f;

            public PutCurve(AnimationCurve curve) => this.curve = curve;

            public float Eval(float time) => curve.Evaluate(time) * EvalMultiply;
        }

        public sealed class RigidbodySettings
        {
            public Rigidbody Rigidbody { get; set; }
            public bool IsKinematic { get; set; }
            public bool UseGravity { get; set; }

            public RigidbodySettings(Rigidbody rigidbody)
            {
                Rigidbody = rigidbody;
                IsKinematic = rigidbody.isKinematic;
                UseGravity = rigidbody.useGravity;
            }
        }

        public readonly struct TransformSettings
        {
            public Vector3 Position { get; }
            public Quaternion Rotation { get; }
            public Vector3 ControlOffset { get; }

            public TransformSettings(Vector3 position, Quaternion rotation, Vector3 controlOffset)
            {
                Position = position;
                Rotation = rotation;
                ControlOffset = controlOffset;
            }
        }

        public readonly struct CurveSettings
        {
            public PutCurve PositionCurve { get; }
            public PutCurve RotationCurve { get; }

            public CurveSettings(PutCurve positionCurve, PutCurve rotationCurve)
            {
                PositionCurve = positionCurve;
                RotationCurve = rotationCurve;
            }
        }

        public readonly struct PutSettings
        {
            public TransformSettings TransformData { get; }
            public CurveSettings CurveData { get; }
            public RigidbodySettings RigidbodySettings { get; }
            public bool IsLocalSpace { get; }

            public PutSettings(Transform tr, TransformSettings transformSettings, CurveSettings curveSettings, RigidbodySettings rigidbodySettings, bool isLocalSpace)
            {
                TransformData = new TransformSettings(
                    isLocalSpace ? tr.localPosition : tr.position,
                    isLocalSpace ? tr.localRotation : tr.rotation,
                    isLocalSpace ? tr.localPosition + transformSettings.ControlOffset : tr.position + transformSettings.ControlOffset
                );

                CurveData = curveSettings;
                RigidbodySettings = rigidbodySettings;
                IsLocalSpace = isLocalSpace;
            }
        }

        private PutSettings _putSettings;
        private Vector3 _putStartPos;
        private Quaternion _putStartRot;
        private bool _putStarted;

        private float _putPosT;
        private float _putPosVelocity;

        private float _putRotT;
        private float _putRotVelocity;

        public void Put(PutSettings putSettings)
        {
            _putSettings = putSettings;
            _putStartPos = putSettings.IsLocalSpace ? transform.localPosition : transform.position;
            _putStartRot = putSettings.IsLocalSpace ? transform.localRotation : transform.rotation;
            _putStarted = true;
        }

        private void Update()
        {
            if (!_putStarted) 
                return;

            UpdatePosition();
            UpdateRotation();

            if (_putPosT * _putRotT >= 0.99f)
            {
                SetFinalTransformState();
                HandleRigidbodySettings();
                Destroy(this);
            }
        }

        private void UpdatePosition()
        {
            float putPosCurve = _putSettings.CurveData.PositionCurve.Eval(_putPosT);
            _putPosT = Mathf.SmoothDamp(_putPosT, 1f, ref _putPosVelocity, _putSettings.CurveData.PositionCurve.CurveTime + putPosCurve);

            if (!_putSettings.IsLocalSpace)
                transform.position = VectorE.QuadraticBezier(_putStartPos, _putSettings.TransformData.Position, _putSettings.TransformData.ControlOffset, _putPosT);
            else
                transform.localPosition = VectorE.QuadraticBezier(_putStartPos, _putSettings.TransformData.Position, _putSettings.TransformData.ControlOffset, _putPosT);
        }

        private void UpdateRotation()
        {
            float putRotCurve = _putSettings.CurveData.RotationCurve.Eval(_putRotT);
            _putRotT = Mathf.SmoothDamp(_putRotT, 1f, ref _putRotVelocity, _putSettings.CurveData.RotationCurve.CurveTime + putRotCurve);

            if (!_putSettings.IsLocalSpace)
                transform.rotation = Quaternion.Slerp(_putStartRot, _putSettings.TransformData.Rotation, _putRotT);
            else
                transform.localRotation = Quaternion.Slerp(_putStartRot, _putSettings.TransformData.Rotation, _putRotT);
        }

        private void SetFinalTransformState()
        {
            if (!_putSettings.IsLocalSpace)
                transform.SetPositionAndRotation(_putSettings.TransformData.Position, _putSettings.TransformData.Rotation);
            else
                transform.SetLocalPositionAndRotation(_putSettings.TransformData.Position, _putSettings.TransformData.Rotation);
        }

        private void HandleRigidbodySettings()
        {
            if (_putSettings.RigidbodySettings == null || _putSettings.RigidbodySettings.Rigidbody == null) 
                return;

            Rigidbody rigidbody = _putSettings.RigidbodySettings.Rigidbody;
            rigidbody.isKinematic = _putSettings.RigidbodySettings.IsKinematic;
            rigidbody.useGravity = _putSettings.RigidbodySettings.UseGravity;
        }
    }
}
