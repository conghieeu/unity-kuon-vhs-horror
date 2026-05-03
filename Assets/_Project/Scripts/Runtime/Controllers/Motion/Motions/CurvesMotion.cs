using System;
using UnityEngine;

namespace UHFPS.Runtime
{
    [Serializable]
    public class CurvesMotion : SpringMotionModule
    {
        public override string Name => "General/Curves Motion";

        [Header("Enter Curve Settings")]
        public Curve3D enterPositionCurves;
        public Curve3D enterRotationCurves;
        public float enterTimeModifier = 1f;

        [Header("Exit Curve Settings")]
        public Curve3D exitPositionCurves;
        public Curve3D exitRotationCurves;
        public float exitTimeModifier = 1f;

        private float currentCurveTime;
        private bool posCurveCompleted;
        private bool rotCurveCompleted;
        private bool hasEntered;
        private bool reset;

        public override void MotionUpdate(float deltaTime)
        {
            if (IsUpdatable)
            {
                if (!reset)
                {
                    posCurveCompleted = false;
                    rotCurveCompleted = false;
                    currentCurveTime = 0f;
                    reset = true;
                }

                if (!posCurveCompleted || !rotCurveCompleted)
                {
                    EvaluateEnterCurves(deltaTime);
                    hasEntered = true;
                }
                else
                {
                    SetTargetPosition(Vector3.zero);
                    SetTargetRotation(Vector3.zero);
                }
            }
            else if(hasEntered)
            {
                if (reset)
                {
                    posCurveCompleted = false;
                    rotCurveCompleted = false;
                    currentCurveTime = 0f;
                    reset = false;
                }

                if (!posCurveCompleted || !rotCurveCompleted)
                {
                    EvaluateExitCurves(deltaTime);
                }
                else
                {
                    SetTargetPosition(Vector3.zero);
                    SetTargetRotation(Vector3.zero);
                    hasEntered = false;
                }
            }
        }

        private void EvaluateEnterCurves(float deltaTime)
        {
            posCurveCompleted = enterPositionCurves.Duration < currentCurveTime;
            if (!posCurveCompleted)
            {
                Vector3 positionCurve = enterPositionCurves.Evaluate(currentCurveTime);
                SetTargetPosition(positionCurve);
            }

            rotCurveCompleted = enterRotationCurves.Duration < currentCurveTime;
            if (!rotCurveCompleted)
            {
                Vector3 rotationCurve = enterRotationCurves.Evaluate(currentCurveTime);
                SetTargetRotation(rotationCurve);
            }

            currentCurveTime += deltaTime * enterTimeModifier;
        }

        private void EvaluateExitCurves(float deltaTime)
        {
            posCurveCompleted = exitPositionCurves.Duration < currentCurveTime;
            if (!posCurveCompleted)
            {
                Vector3 positionCurve = exitPositionCurves.Evaluate(currentCurveTime);
                SetTargetPosition(positionCurve);
            }

            rotCurveCompleted = exitRotationCurves.Duration < currentCurveTime;
            if (!rotCurveCompleted)
            {
                Vector3 rotationCurve = exitRotationCurves.Evaluate(currentCurveTime);
                SetTargetRotation(rotationCurve);
            }

            currentCurveTime += deltaTime * exitTimeModifier;
        }

        public override void Reset()
        {
            posCurveCompleted = false;
            rotCurveCompleted = false;
            currentCurveTime = 0f;
            hasEntered = false;
        }
    }
}