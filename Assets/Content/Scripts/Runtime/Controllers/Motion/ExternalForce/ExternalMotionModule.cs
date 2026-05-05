using System;
using UnityEngine;
using ThunderWire.Attributes;

namespace UHFPS.Runtime
{
    [Summary("Lớp cơ sở cho các module ngoại lực (External Motion) độc lập.")]
    public abstract class ExternalMotionModule
    {
        public abstract bool IsFinished { get; }
        public abstract Vector3 Evaluate();
    }

    [Serializable]
    [Summary("Dữ liệu cấu hình cho một External Motion.")]
    public abstract class ExternalMotionData
    {
        public abstract string Name { get; }

        public abstract ExternalMotionModule GetPosition { get; }
        public bool PositionEnable = true;

        public abstract ExternalMotionModule GetRotation { get; }
        public bool RotationEnable = true;
    }
}