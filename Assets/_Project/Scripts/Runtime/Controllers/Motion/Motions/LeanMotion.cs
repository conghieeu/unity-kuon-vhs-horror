using UHFPS.Input;
using UHFPS.Tools;
using UnityEngine;

namespace UHFPS.Runtime
{
    public class LeanMotion : SpringMotionModule
    {
        public override string Name => "Camera/Lean Motion";

        [Header("General Settings")]
        public LayerMask leanMask;
        public float leanPosition;
        public float leanTiltAmount;
        public float leanColliderRadius;

        public override void MotionUpdate(float deltaTime)
        {
            if (!IsUpdatable)
                return;

            float leanDir = InputManager.ReadInput<float>(Controls.LEAN);
            Vector3 leanPos = new(leanDir * leanPosition, 0f, 0f);

            // calculate the lean tilt value
            float leanBlend = VectorE.InverseLerp(Vector3.zero, leanPos, transform.localPosition);
            Vector3 leanTilt = -1 * leanDir * leanTiltAmount * leanBlend * Vector3.forward;

            // calculate the head position offset value
            Vector3 leanDirection = transform.right * leanDir;
            Ray leanRay = new Ray(transform.position, leanDirection);

            // convert the max lean distance to a multiplier and multiply it with the leanPos value
            if (Physics.SphereCast(leanRay, leanColliderRadius, out RaycastHit hit, leanPosition, leanMask))
                leanPos *= GameTools.Remap(0f, leanPosition, 0f, 1f, hit.distance);

            SetTargetPosition(leanPos);
            SetTargetRotation(leanTilt);
        }
    }
}