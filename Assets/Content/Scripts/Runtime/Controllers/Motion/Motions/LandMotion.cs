using System;
using UnityEngine;

namespace UHFPS.Runtime
{
    [Serializable]
    public class LandMotion : SpringMotionModule
    {
        public override string Name => "General/Land Motion";

        [Header("General Settings")]
        public OffsetMotion.OffsetSettings landSettings;

        [Header("Impact Settings")]
        public float minActivateAirTime = 1f;
        public float maxImpactAirTime = 2f;
        public float positionMultiplier = 1f;
        public float rotationMultiplier = 1f;

        private float remainingResetDuration;
        private float airTime;
        private bool airborne;

        public override void MotionUpdate(float deltaTime)
        {
            if (!IsUpdatable)
            {
                SetTargetPosition(Vector3.zero);
                SetTargetRotation(Vector3.zero);
                return;
            }

            if (remainingResetDuration > 0f)
                remainingResetDuration -= Time.deltaTime;

            if (!player.StateGrounded)
            {
                airTime += deltaTime;
                airborne = airTime > minActivateAirTime;
            }
            else if(airborne)
            {
                remainingResetDuration = landSettings.duration;

                float multMod = Mathf.InverseLerp(0f, maxImpactAirTime, airTime);
                float posMult = Mathf.Lerp(0f, positionMultiplier, multMod);
                float rotMult = Mathf.Lerp(0f, rotationMultiplier, multMod);

                SetTargetPosition(landSettings.positionOffset * posMult);
                SetTargetRotation(landSettings.rotationOffset * rotMult);

                airborne = false;
                airTime = 0f;
            }
            else
            {
                airborne = false;
                airTime = 0f;
            }

            if (remainingResetDuration <= 0f)
            {
                SetTargetPosition(Vector3.zero);
                SetTargetRotation(Vector3.zero);
                remainingResetDuration = 0f;
            }
        }

        public override void Reset()
        {
            remainingResetDuration = 0f;
            airTime = 0f;
            airborne = false;
        }
    }
}