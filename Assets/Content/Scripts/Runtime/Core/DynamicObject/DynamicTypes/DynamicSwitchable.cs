using System;
using UnityEngine;
using UHFPS.Tools;
using Newtonsoft.Json.Linq;

namespace UHFPS.Runtime
{
    [Serializable]
    [ThunderWire.Attributes.Summary("Cấu hình vật thể có thể bật/tắt (Ví dụ: Công tắc đèn, Cần gạt).")]
    public class DynamicSwitchable : DynamicObjectType
    {
        [Header("Limits")]
        [Tooltip("Giới hạn góc tối thiểu/tối đa mà công tắc có thể di chuyển.")]
        public MinMax switchLimits;
        [Tooltip("Góc độ ban đầu của công tắc khi trò chơi bắt đầu.")]
        public float startingAngle;
        [Tooltip("Trục bản lề (Hinge Axis) của công tắc. Thường là trục Y.")]
        public Axis targetHinge = Axis.Y;
        [Tooltip("Trục hướng về phía trước của công tắc. Thường là trục X.")]
        public Axis targetForward = Axis.X;

        [Tooltip("Sử dụng các trục địa phương (Local Axes) thay vì trục toàn cầu (Global Axes).")]
        public bool useLocalAxes;
        [Tooltip("Đảo ngược góc độ bắt đầu (khi mũi tên đỏ nằm ở hướng ngược lại).")]
        public bool startingAngleFlip;
        [Tooltip("Phản chiếu (Mirror) hướng xoay quanh trục bản lề.")]
        public bool targetHingeMirror;
        [Tooltip("Phản chiếu (Mirror) hướng xoay quanh trục phía trước.")]
        public bool targetForwardMirror;

        // switchable properties
        [Header("Switchable Properties")]
        [Tooltip("Transform chứa (cha) của tay cầm công tắc (Handle). Thường là phần đế của công tắc.")]
        public Transform rootObject;
        [Tooltip("Đường cong Animation định nghĩa tốc độ khi bật công tắc lên. (0 = bắt đầu, 1 = kết thúc).")]
        public AnimationCurve switchOnCurve = new(new(0, 1), new(1, 1));
        [Tooltip("Đường cong Animation định nghĩa tốc độ khi tắt công tắc đi. (0 = bắt đầu, 1 = kết thúc).")]
        public AnimationCurve switchOffCurve = new(new(0, 1), new(1, 1));
        [Tooltip("Tốc độ gạt công tắc (Bật/Tắt).")]
        public float switchSpeed = 1f;
        [Tooltip("Độ cản (Damper) của khớp nối (Joint) khi dùng cơ chế Vật lý.")]
        public float damping = 1f;

        [Tooltip("Đảo ngược hướng gạt công tắc, ví dụ khi công tắc đã được bật sẵn.")]
        public bool flipSwitchDirection = false;
        [Tooltip("Đảo ngược hướng kéo thả bằng chuột.")]
        public bool flipMouse = false;
        [Tooltip("Đảo ngược giới hạn min/max. Thường dùng khi âm thanh gạt bị ngược.")]
        public bool flipAngle = false;
        [Tooltip("Khóa công tắc lại sau khi đã gạt thành công một lần.")]
        public bool lockOnSwitch = true;
        [Tooltip("Hiển thị vùng quét giới hạn (Gizmos) trong Editor.")]
        public bool showGizmos = true;

        // private
        private float currentAngle;
        private float targetAngle;
        private float mouseSmooth;

        private bool isSwitched;
        private bool isMoving;
        private bool isSwitchLocked;
        private bool isSwitchSound;

        private Vector3 hingeAxis;
        private Vector3 hingeAxisLocal;
        private Vector3 forwardAxisLocal;

        public override bool ShowGizmos => showGizmos;

        public override bool IsOpened => isSwitched;

        public override void OnDynamicInit()
        {
            int mirrorUpwd = targetHingeMirror ? -1 : 1;
            int mirrorFwd = targetForwardMirror ? -1 : 1;

            // for local axes and editor preview
            hingeAxisLocal = Target.Direction(targetHinge) * mirrorUpwd;
            forwardAxisLocal = Target.Direction(targetForward) * mirrorFwd;

            if (useLocalAxes)
            {
                hingeAxis = hingeAxisLocal;
            }
            else
            {
                hingeAxis = targetHinge.Convert() * mirrorUpwd;
            }

            if (InteractType == DynamicObject.InteractType.Dynamic)
            {
                targetAngle = startingAngle;
                currentAngle = startingAngle;
            }
            else if(InteractType == DynamicObject.InteractType.Mouse)
            {
                currentAngle = startingAngle;
            }
        }

        public override void OnDynamicStart(PlayerManager player)
        {
            if (isSwitchLocked) return;
            if (!DynamicObject.isLocked)
            {
                if (InteractType == DynamicObject.InteractType.Dynamic && !isMoving)
                {
                    isSwitched = !isSwitched;
                    targetAngle = flipSwitchDirection
                        ? (isSwitched ? switchLimits.max : switchLimits.min)
                        : (isSwitched ? switchLimits.min : switchLimits.max);

                    if (lockOnSwitch) isSwitchLocked = true;
                }
                else if (InteractType == DynamicObject.InteractType.Animation && !Animator.IsAnyPlaying())
                {
                    if (isSwitched = !isSwitched)
                    {
                        Animator.SetTrigger(DynamicObject.useTrigger1);
                        DynamicObject.useEvent1?.Invoke(); // on event
                    }
                    else
                    {
                        Animator.SetTrigger(DynamicObject.useTrigger2);
                        DynamicObject.useEvent2?.Invoke(); // off eevent
                    }

                    if (lockOnSwitch) isSwitchLocked = true;
                }
            }
            else
            {
                TryUnlock();
            }
        }

        public override void OnDynamicOpen()
        {
            if (InteractType == DynamicObject.InteractType.Dynamic && !isMoving)
            {
                targetAngle = flipSwitchDirection
                    ? switchLimits.max : switchLimits.min;

                isSwitched = true;
                if (lockOnSwitch) isSwitchLocked = true;
            }
            else if (InteractType == DynamicObject.InteractType.Animation && !Animator.IsAnyPlaying())
            {
                Animator.SetTrigger(DynamicObject.useTrigger1);
                DynamicObject.useEvent1?.Invoke();

                isSwitched = true;
                if (lockOnSwitch) isSwitchLocked = true;
            }
        }

        public override void OnDynamicClose()
        {
            if (InteractType == DynamicObject.InteractType.Dynamic && !isMoving)
            {
                targetAngle = flipSwitchDirection
                    ? switchLimits.min : switchLimits.max;

                isSwitched = false;
                if (lockOnSwitch) isSwitchLocked = true;
            }
            else if (InteractType == DynamicObject.InteractType.Animation && !Animator.IsAnyPlaying())
            {
                Animator.SetTrigger(DynamicObject.useTrigger2);
                DynamicObject.useEvent2?.Invoke();

                isSwitched = false;
                if (lockOnSwitch) isSwitchLocked = true;
            }
        }

        public override void OnDynamicUpdate()
        {
            float t = 0;

            if(InteractType != DynamicObject.InteractType.Animation)
            {
                float angle = GetSignedAngle(targetHinge);
                angle = Mathf.Clamp(angle, switchLimits.RealMin, switchLimits.RealMax);

                // 1,1 = false, 0,0 = false, 0,1 or 1,0 = true
                bool flip = startingAngleFlip ^ flipAngle;
                float minAngle = flip ? switchLimits.max : switchLimits.min;
                float maxAngle = flip ? switchLimits.min : switchLimits.max;
                t = Mathf.InverseLerp(minAngle, maxAngle, angle);

                if (InteractType == DynamicObject.InteractType.Dynamic)
                {
                    isMoving = t > 0 && t < 1;

                    float modifier = isSwitched ? switchOnCurve.Evaluate(t) : switchOffCurve.Evaluate(1 - t);
                    currentAngle = Mathf.MoveTowards(currentAngle, targetAngle, Time.deltaTime * switchSpeed * 10 * modifier);
                    SetSwitchableAngle(currentAngle);
                }
                else if (InteractType == DynamicObject.InteractType.Mouse)
                {
                    mouseSmooth = Mathf.MoveTowards(mouseSmooth, targetAngle, Time.deltaTime * (targetAngle != 0 ? switchSpeed : damping));
                    currentAngle = Mathf.Clamp(currentAngle + mouseSmooth, switchLimits.RealMin, switchLimits.RealMax);
                    SetSwitchableAngle(currentAngle);

                    if (t > 0.99f && !isSwitched) isSwitched = true;
                    else if (t < 0.01f && isSwitched) isSwitched = false;
                }

                if (isSwitched && !isSwitchSound && t > 0.99f)
                {
                    DynamicObject.useEvent1?.Invoke(); // on event
                    DynamicObject.PlaySound(DynamicSoundType.Open);
                    isSwitchSound = true;
                }
                else if (!isSwitched && isSwitchSound && t < 0.01f)
                {
                    DynamicObject.useEvent2?.Invoke(); // off event
                    DynamicObject.PlaySound(DynamicSoundType.Close);
                    isSwitchSound = false;
                }
            }

            // value change event
            DynamicObject.onValueChange?.Invoke(t);
        }

        private void SetSwitchableAngle(float angle)
        {
            Quaternion rotation = Quaternion.AngleAxis(angle, hingeAxis);
            if (TransformType == DynamicObject.TransformType.Local)
            {
                Target.localRotation = rotation;
            }
            else
            {
                Target.rotation = rotation;
            }
        }

        private float GetSignedAngle(Axis axis)
        {
            float angle = Target.localEulerAngles.Component(axis);
            if (angle > 180) angle -= 360;
            return angle;
        }

        private float GetStartingAngle()
        {
            float _startingAngle = startingAngle;
            if (startingAngleFlip)
            {
                float t = Mathf.InverseLerp(switchLimits.min, switchLimits.max, startingAngle);
                _startingAngle = Mathf.Lerp(switchLimits.max, switchLimits.min, t);
            }

            return _startingAngle;
        }

        public override void OnDynamicHold(Vector2 mouseDelta)
        {
            if (InteractType == DynamicObject.InteractType.Mouse && !isSwitchLocked)
            {
                mouseDelta.x = 0;
                float mouseInput = Mathf.Clamp(mouseDelta.y, -1, 1) * (flipMouse ? 1 : -1);
                targetAngle = mouseDelta.magnitude > 0 ? mouseInput : 0;
            }

            IsHolding = true;
        }

        public override void OnDynamicEnd()
        {
            if (InteractType == DynamicObject.InteractType.Mouse && !isSwitchLocked)
            {
                targetAngle = 0;
            }

            IsHolding = false;
        }

        public override void OnDrawGizmos()
        {
            if (DynamicObject == null || rootObject == null || InteractType == DynamicObject.InteractType.Animation) 
                return;

            int mirrorFwd = targetForwardMirror ? -1 : 1;
            int mirrorUpwd = targetHingeMirror ? -1 : 1;

            Vector3 upward = Application.isPlaying ? hingeAxisLocal : (rootObject.Direction(targetHinge) * mirrorUpwd);
            Vector3 forward = Application.isPlaying ? forwardAxisLocal : (rootObject.Direction(targetForward) * mirrorFwd);
            float radius = 0.3f;

            HandlesDrawing.DrawLimits(rootObject.position, switchLimits, forward, upward, true, radius: 0.25f);
            Vector3 startingDir = Quaternion.AngleAxis(GetStartingAngle(), upward) * forward;

            Gizmos.color = Color.red;
            Gizmos.DrawRay(Target.position, startingDir * (radius + 0.1f));
        }

        public override StorableCollection OnSave()
        {
            StorableCollection saveableBuffer = new StorableCollection();

            if (InteractType != DynamicObject.InteractType.Animation)
            {
                saveableBuffer.Add("rotation", Target.eulerAngles.ToSaveable());
                saveableBuffer.Add("angle", currentAngle);
                saveableBuffer.Add(nameof(isSwitchSound), isSwitchSound);
                saveableBuffer.Add(nameof(isSwitchLocked), isSwitchLocked);
            }

            saveableBuffer.Add(nameof(isSwitched), isSwitched);
            return saveableBuffer;
        }

        public override void OnLoad(JToken token)
        {
            if (InteractType != DynamicObject.InteractType.Animation)
            {
                Target.eulerAngles = token["rotation"].ToObject<Vector3>();
                currentAngle = (float)token["angle"];
                targetAngle = currentAngle;
                isSwitchSound = (bool)token[nameof(isSwitchSound)];
                isSwitchLocked = (bool)token[nameof(isSwitchLocked)];
            }

            isSwitched = (bool)token[nameof(isSwitched)];
        }
    }
}