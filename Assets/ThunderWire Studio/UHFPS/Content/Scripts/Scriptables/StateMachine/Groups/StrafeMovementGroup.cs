using UnityEngine;

namespace UHFPS.Scriptable
{
    [CreateAssetMenu(fileName = "StrafeMovementGroup", menuName = "UHFPS/Player/Strafe Movement Group")]
    public class StrafeMovementGroup : PlayerStatesGroup
    {
        public float friction = 6f;
        public float acceleration = 3f;
        public float deceleration = 1f;

        [Header("Air Settings")]
        public float airAcceleration = 1f;
        public float airAccelerationCap = 0.8f;
    }
}