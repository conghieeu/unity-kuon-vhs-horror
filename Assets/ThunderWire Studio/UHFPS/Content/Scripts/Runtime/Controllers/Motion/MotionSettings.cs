using UHFPS.Scriptable;
using UnityEngine;

namespace UHFPS.Runtime
{
    public struct MotionSettings
    {
        public MotionPreset preset;
        public PlayerComponent component;
        public MotionBlender motionBlender;
        public Transform motionTransform;
        public string motionState;
    }
}