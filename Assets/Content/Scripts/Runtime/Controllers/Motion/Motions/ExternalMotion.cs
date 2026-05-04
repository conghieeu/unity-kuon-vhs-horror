using System;
using System.Collections.Generic;
using UnityEngine;

namespace UHFPS.Runtime
{
    [Serializable]
    public class ExternalMotion : SpringMotionModule
    {
        public override string Name => "Camera/External Motion";
        private readonly List<ExternalMotionModule> positionModules = new();
        private readonly List<ExternalMotionModule> rotationModules = new();

        public void AddPositionForce(ExternalMotionModule motionModule)
        {
            positionModules.Add(motionModule);
            SetTargetPosition(EvaluatePositionForces());
        }

        public void AddRotationForce(ExternalMotionModule motionModule)
        {
            rotationModules.Add(motionModule);
            SetTargetRotation(EvaluateRotationForces());
        }

        public override void MotionUpdate(float deltaTime)
        {
            if (!IsUpdatable)
                return;

            SetTargetPosition(EvaluatePositionForces());
            SetTargetRotation(EvaluateRotationForces());
        }

        private Vector3 EvaluatePositionForces()
        {
            Vector3 force = Vector3.zero;
            for (int i = 0; i < positionModules.Count;)
            {
                var module = positionModules[i];
                force += module.Evaluate();

                if (module.IsFinished)
                    positionModules.RemoveAt(i);
                else i++;
            }

            return force;
        }

        private Vector3 EvaluateRotationForces()
        {
            Vector3 force = Vector3.zero;
            for (int i = 0; i < rotationModules.Count;)
            {
                var module = rotationModules[i];
                force += module.Evaluate();

                if (module.IsFinished)
                    rotationModules.RemoveAt(i);
                else i++;
            }

            return force;
        }
    }
}