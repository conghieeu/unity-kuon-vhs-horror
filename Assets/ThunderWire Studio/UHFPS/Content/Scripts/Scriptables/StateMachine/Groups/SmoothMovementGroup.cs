using UnityEngine;

namespace UHFPS.Scriptable
{
    [CreateAssetMenu(fileName = "SmoothMovementGroup", menuName = "UHFPS/Player/Smooth Movement Group")]
    public class SmoothMovementGroup : PlayerStatesGroup
    {
        public float friction = 6f;
        public float acceleration = 3f;
        public float deceleration = 1f;
    }
}