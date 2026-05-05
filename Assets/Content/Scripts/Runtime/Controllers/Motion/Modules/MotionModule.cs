using System;
using System.Collections.Generic;
using UnityEngine;
using UHFPS.Scriptable;
using ThunderWire.Attributes;

namespace UHFPS.Runtime
{
    [Serializable]
    [Summary("Lớp cơ sở trừu tượng cho tất cả các module chuyển động (Motion).")]
    public abstract class MotionModule
    {
        [Range(0f, 1f)]
        [Tooltip("Trọng số ảnh hưởng của chuyển động này (0 = không tác động, 1 = 100%).")]
        public float Weight = 1f;

        [NonSerialized] protected MotionPreset preset;
        [NonSerialized] protected string state;

        [NonSerialized] protected PlayerComponent component;
        [NonSerialized] protected MotionBlender motionBlender;
        [NonSerialized] protected Transform transform;

        [NonSerialized] protected CharacterController controller;
        [NonSerialized] protected PlayerStateMachine player;
        [NonSerialized] protected LookController look;

        /// <summary>
        /// Runtime motion parameters.
        /// </summary>
        protected Dictionary<string, object> Parameters => preset.RuntimeParameters;

        /// <summary>
        /// Check whether the module is updatable.
        /// </summary>
        protected bool IsUpdatable
        {
            get
            {
                string currentState = player.StateName;
                return state == MotionBlender.Default || state == currentState;
            }
        }

        public virtual void Initialize(MotionSettings motionSettings)
        {
            preset = motionSettings.preset;
            state = motionSettings.motionState;

            component = motionSettings.component;
            motionBlender = motionSettings.motionBlender;
            transform = motionSettings.motionTransform;

            controller = component.PlayerCollider;
            player = component.PlayerStateMachine;
            look = component.LookController;

            motionBlender.Disposables.Add(player.ObservableState.Subscribe(OnStateChange));
        }

        public abstract string Name { get; }

        public abstract void MotionUpdate(float deltaTime);
        public abstract Vector3 GetPosition(float deltaTime);
        public abstract Quaternion GetRotation(float deltaTime);

        public virtual void Reset() { }

        public virtual void ResetSpring() { }

        public virtual void OnStateChange(string state) { }

        protected abstract void SetTargetPosition(Vector3 target);
        protected abstract void SetTargetRotation(Vector3 target);

        protected abstract void SetTargetPosition(Vector3 target, float multiplier = 1f);
        protected abstract void SetTargetRotation(Vector3 target, float multiplier = 1f);
    }
}