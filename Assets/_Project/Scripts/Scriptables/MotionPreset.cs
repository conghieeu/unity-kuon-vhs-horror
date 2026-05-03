using System.Collections.Generic;
using UnityEngine;
using UHFPS.Runtime;

namespace UHFPS.Scriptable
{
    [CreateAssetMenu(fileName = "Motion Preset", menuName = "UHFPS/Game/Motion Preset")]
    public class MotionPreset : ScriptableObject
    {
        public List<StateMotionData> StateMotions = new();
        public Dictionary<string, object> RuntimeParameters = new();

        public object this[string key]
        {
            get
            {
                if (RuntimeParameters.TryGetValue(key, out object value))
                    return value;

                return null;
            }
            set 
            {
                RuntimeParameters[key] = value;
            }
        }

        public bool TryGetValue(string key, out object value)
        {
            if (RuntimeParameters.TryGetValue(key, out object val))
            {
                value = val;
                return true;
            }

            value = null;
            return false;
        }

        public void Initialize(MotionBlender motionBlender, PlayerComponent component, Transform motionTransform)
        {
            foreach (var state in StateMotions)
            {
                foreach (var motion in state.Motions)
                {
                    motion.Initialize(new MotionSettings()
                    {
                        preset = this,
                        component = component,
                        motionBlender = motionBlender,
                        motionTransform = motionTransform,
                        motionState = state.StateID
                    });
                }
            }
        }

        public void Reset()
        {
            foreach (var state in StateMotions)
            {
                foreach (var motion in state.Motions)
                {
                    motion.Reset();
                    motion.ResetSpring();
                }
            }
        }
    }
}