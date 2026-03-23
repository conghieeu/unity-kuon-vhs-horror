using System;
using UnityEngine;

namespace UHFPS.Runtime
{
    public abstract class ExternalMotionModule
    {
        public abstract bool IsFinished { get; }
        public abstract Vector3 Evaluate();
    }

    [Serializable]
    public abstract class ExternalMotionData
    {
        public abstract string Name { get; }

        public abstract ExternalMotionModule GetPosition { get; }
        public bool PositionEnable = true;

        public abstract ExternalMotionModule GetRotation { get; }
        public bool RotationEnable = true;
    }
}