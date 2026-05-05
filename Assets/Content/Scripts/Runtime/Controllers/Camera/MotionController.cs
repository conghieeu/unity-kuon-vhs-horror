using System.Reactive.Disposables;
using System;
using UnityEngine;
using UHFPS.Scriptable;
using ThunderWire.Attributes;

namespace UHFPS.Runtime
{
    [Docs("https://docs.twgamesdev.com/uhfps/guides/motion-controller")]
    [Summary("Điều khiển việc pha trộn và áp dụng các hiệu ứng chuyển động (Motion) lên tay và đầu của nhân vật.")]
    public class MotionController : PlayerComponent
    {
        public MotionBlender MotionBlender = new();

        [Tooltip("Transform áp dụng chuyển động cho hai tay (Hands).")]
        public Transform HandsMotionTransform;
        [Tooltip("Transform áp dụng chuyển động cho góc nhìn (Head/Camera).")]
        public Transform HeadMotionTransform;
        [Tooltip("ScriptableObject chứa các cài đặt cấu hình chuyển động có sẵn.")]
        public MotionPreset MotionPreset;

        [Tooltip("Có làm giảm dần/tắt hiệu ứng chuyển động khi cần thiết không.")]
        public bool MotionSuppress = true;
        [Tooltip("Tốc độ tắt dần hiệu ứng chuyển động.")]
        public float MotionSuppressSpeed = 2f;
        [Tooltip("Tốc độ khôi phục hiệu ứng chuyển động.")]
        public float MotionResetSpeed = 2f;

        /// <summary>
        /// Head bob Y axis wave value
        /// </summary>
        public float BobWave
        {
            get
            {
                bool flag1 = PlayerStateMachine.IsCurrent(PlayerStateMachine.WALK_STATE);
                bool flag2 = PlayerStateMachine.IsCurrent(PlayerStateMachine.RUN_STATE);
                bool flag3 = PlayerStateMachine.IsCurrent(PlayerStateMachine.CROUCH_STATE);

                if ((flag1 || flag2 || flag3)
                    && MotionBlender != null
                    && MotionBlender.Instance.TryGetValue("waveY", out object value))
                    return (float)value;

                return 0f;
            }
        }

        public CompositeDisposable Disposables = new();

        private void Awake()
        {
            MotionBlender.Init(MotionPreset, HeadMotionTransform, this);
        }

        private void OnDestroy()
        {
            MotionBlender.Dispose();
            Disposables.Dispose();
        }

        private void Update()
        {
            if (MotionSuppress)
            {
                if (isEnabled && MotionBlender.Weight < 1f)
                {
                    MotionBlender.Weight = Mathf.MoveTowards(MotionBlender.Weight, 1f, Time.deltaTime * MotionResetSpeed);
                }
                else if (!isEnabled && MotionBlender.Weight > 0f)
                {
                    MotionBlender.Weight = Mathf.MoveTowards(MotionBlender.Weight, 0f, Time.deltaTime * MotionSuppressSpeed);
                }
            }

            MotionBlender.BlendMotions(Time.deltaTime, out var position, out var rotation);
            HeadMotionTransform.SetLocalPositionAndRotation(position, rotation);
        }

        /// <summary>
        /// Get motion that is added to the default motion state.
        /// </summary>
        public T GetDefaultMotion<T>() where T : MotionModule
        {
            if (MotionBlender.IsInitialized)
            {
                Type motionType = typeof(T);
                foreach (var state in MotionBlender.Instance.StateMotions)
                {
                    if (state.StateID == MotionBlender.Default)
                    {
                        foreach (var motion in state.Motions)
                        {
                            if (motion.GetType() == motionType)
                                return (T)motion;
                        }
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Get motion that is added to the specific motion state.
        /// </summary>
        public T GetStateMotion<T>(string stateID) where T : MotionModule
        {
            if (MotionBlender.IsInitialized)
            {
                Type motionType = typeof(T);
                foreach (var state in MotionBlender.Instance.StateMotions)
                {
                    if (state.StateID == stateID)
                    {
                        foreach (var motion in state.Motions)
                        {
                            if (motion.GetType() == motionType)
                                return (T)motion;
                        }
                    }
                }
            }

            return null;
        }

        public void ResetMotions()
        {
            MotionBlender.ResetMotions();
            HeadMotionTransform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
        }
    }
}