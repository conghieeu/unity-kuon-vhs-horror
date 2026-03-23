using System;
using System.Collections.Generic;
using UnityEngine;

namespace UHFPS.Runtime
{
    [Serializable]
    public sealed class ExternalMotions
    {
        [Serializable]
        public sealed class ExternalMotionState
        {
            public string EventID;

            [SerializeReference]
            public List<ExternalMotionData> ExternalMotions = new();
        }

        public List<ExternalMotionState> MotionStates = new();

        private MotionController motionController;
        private ExternalMotion externalMotion;

        private ExternalMotion ExternalMotion
        {
            get => externalMotion ??= motionController.GetDefaultMotion<ExternalMotion>();
        }

        public void Init(MotionController motionController)
        {
            this.motionController = motionController;
        }

        public void ApplyEffect(string eventID)
        {
            if (ExternalMotion == null)
                throw new NullReferenceException("The ExternalMotion module is not added to the default camera state in Motion Controller!");

            foreach (var state in MotionStates)
            {
                if (state.EventID != eventID)
                    continue;

                foreach (var motion in state.ExternalMotions)
                {
                    if (motion.PositionEnable)
                    {
                        var motionModule = motion.GetPosition;
                        ExternalMotion.AddPositionForce(motionModule);
                    }

                    if (motion.RotationEnable)
                    {
                        var motionModule = motion.GetRotation;
                        ExternalMotion.AddRotationForce(motionModule);
                    }
                }

                break;
            }
        }
    }
}