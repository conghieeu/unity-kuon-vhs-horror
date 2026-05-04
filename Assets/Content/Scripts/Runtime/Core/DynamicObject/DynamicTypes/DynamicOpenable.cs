using System.Collections;
using System;
using UnityEngine;
using UHFPS.Tools;
using Newtonsoft.Json.Linq;
using TMPro;

namespace UHFPS.Runtime
{
    [Serializable]
    public class DynamicOpenable : DynamicObjectType
    {
        // limits
        [Tooltip("Limits that define the minimum/maximum angle at which the openable can be opened.")]
        public MinMax openLimits;
        [Tooltip("Angle at which an openable is opened when the game is started.")]
        public float startingAngle;
        [Tooltip("Usually the axis that defines the higne joint. Most likely the Y-axis.")]
        public Axis targetHinge = Axis.Y;
        [Tooltip("Usually the axis that defines the openable forward or model extend direction. Most likely the Z-axis.")]
        public Axis targetForward = Axis.Z;
        [Tooltip("Usually the axis that determines the open direction of the frame. The direction is used to determine in which direction the door should open. Most likely the Z-axis.")]
        public Axis frameForward = Axis.Z;

        [Tooltip("Use local axes instead of global axes (can be useful in some situations).")]
        public bool useLocalAxes;
        [Tooltip("Flip the starting angle when the limits are inversed (the red arrow is on the other side).")]
        public bool startingAngleFlip;
        [Tooltip("Mirror the direction around the hinge axis.")]
        public bool targetHingeMirror;
        [Tooltip("Mirror the direction around the forward axis.")]
        public bool targetForwardMirror;

        // openable properties
        [Tooltip("The curve that defines the opening speed for modifier. 0 = start to 1 = end.")]
        public AnimationCurve openCurve = new(new(0, 1), new(1, 1));
        [Tooltip("The curve that defines the closing speed for modifier. 0 = start to 1 = end.")]
        public AnimationCurve closeCurve = new(new(0, 1), new(1, 1));

        [Tooltip("Defines the open/close speed of the openable.")]
        public float openSpeed = 1f;
        [Tooltip("Defines the damping of an openable joint.")]
        public float damper = 1f;
        [Tooltip("Defines the minimum volume at which the open/close motion sound will be played.")]
        public float dragSoundPlay = 0.2f;

        [Tooltip("Flip the open direction, for example when the openable is already opened or the open limits are flipped.")]
        public bool flipOpenDirection = false;
        [Tooltip("Defines if the openable can be opened on both sides.")]
        public bool bothSidesOpen = false;
        [Tooltip("Allows to use drag sounds.")]
        public bool dragSounds = false;
        [Tooltip("Play sound when the openable is closed.")]
        public bool playCloseSound = true;
        [Tooltip("Flip the mouse drag direction.")]
        public bool flipMouse = false;
        [Tooltip("Flip open min/max limits. Usually when open/close sounds are inversed.")]
        public bool flipAngle = false;
        [Tooltip("Show the openable gizmos to visualize the limits.")]
        public bool showGizmos = true;

        public bool useLockedMotion = false;
        public AnimationCurve lockedPattern = new(new Keyframe(0, 0), new Keyframe(1, 0));
        public float lockedMotionAmount;
        public float lockedMotionTime;

        // sounds
        public SoundClip dragSound;

        // private
        private float currentAngle;
        private float targetAngle;
        private float openAngle;
        private float prevAngle;

        private bool isOpened;
        private bool isMoving;
        private bool isOpenSound;
        private bool isCloseSound;
        private bool isLockedTry;
        private bool disableSounds;

        // axes
        private Vector3 frameFwd;
        private Vector3 hingeAxis;

        private Vector3 hingeAxisLocal;
        private Vector3 forwardAxisLocal;

        public override bool ShowGizmos => showGizmos;

        public override bool IsOpened => isOpened;

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
                frameFwd = Target.Direction(frameForward) * -1;
            }
            else
            {
                hingeAxis = targetHinge.Convert() * mirrorUpwd;
                frameFwd = frameForward.Convert() * -1;
            }

            if (InteractType == DynamicObject.InteractType.Mouse && Joint != null)
            {
                // configure joint limits
                JointLimits limits = Joint.limits;
                limits.min = openLimits.min;
                limits.max = openLimits.max;
                Joint.limits = limits;

                // configure joint spring
                JointSpring spring = Joint.spring;
                spring.damper = damper;
                Joint.spring = spring;

                // configure joint motor
                JointMotor motor = Joint.motor;
                motor.force = 1f;
                Joint.motor = motor;

                // enable/disable joint features
                Joint.useSpring = true;
                Joint.useLimits = true;
                Joint.useMotor = false;

                // configure joint axis and rigidbody
                Joint.axis = targetHinge.Convert();
                Rigidbody.isKinematic = false;
                Rigidbody.useGravity = true;
            }

            if(InteractType != DynamicObject.InteractType.Animation)
            {
                float _startingAngle = GetStartingAngle();
                SetOpenableAngle(_startingAngle);

                targetAngle = _startingAngle;
                currentAngle = _startingAngle;
                openAngle = _startingAngle;

                float mid = Mathf.Lerp(openLimits.min, openLimits.max, 0.5f);
                disableSounds = Mathf.Abs(_startingAngle) > Mathf.Abs(mid);
                isOpenSound = disableSounds;
            }
        }

        public override void OnDynamicStart(PlayerManager player)
        {
            if (DynamicObject.isLocked)
            {
                TryUnlock();
                return;
            }

            if (InteractType == DynamicObject.InteractType.Dynamic)
            {
                if (isMoving) 
                    return;

                prevAngle = openAngle;
                if (bothSidesOpen)
                {
                    // calculate how much the player is looking in the object forward direction
                    float lookDirection = Vector3.Dot(frameFwd, player.MainCamera.transform.forward);

                    // get the target angle based on the look direction
                    float target = flipOpenDirection
                        ? (lookDirection > 0 ? openLimits.max : openLimits.min)
                        : (lookDirection > 0 ? openLimits.min : openLimits.max);

                    // set the target angle
                    isOpened = !isOpened;
                    openAngle = targetAngle = isOpened ? target : 0;
                }
                else
                {
                    // get the target angle based on the open state
                    float target = flipOpenDirection
                        ? (isOpened ? openLimits.max : openLimits.min)
                        : (isOpened ? openLimits.min : openLimits.max);

                    // set the target angle
                    openAngle = targetAngle = target;
                    isOpened = !isOpened;
                }
            }
            else if (InteractType == DynamicObject.InteractType.Animation && !Animator.IsAnyPlaying())
            {
                if (isOpened = !isOpened)
                {
                    if (bothSidesOpen)
                    {
                        float lookDirection = Vector3.Dot(frameFwd, player.MainCamera.transform.forward);
                        Animator.SetBool(DynamicObject.useTrigger3, Mathf.RoundToInt(lookDirection) > 0);
                    }

                    Animator.SetTrigger(DynamicObject.useTrigger1);
                    DynamicObject.PlaySound(DynamicSoundType.Open);
                    DynamicObject.useEvent1?.Invoke();  // open event
                }
                else
                {
                    Animator.SetTrigger(DynamicObject.useTrigger2);
                    DynamicObject.useEvent2?.Invoke(); // close event
                    isCloseSound = true;
                }
            }

            if (disableSounds)
            {
                isOpenSound = false;
                disableSounds = false;
            }
        }

        public override void OnDynamicOpen()
        {
            if (InteractType == DynamicObject.InteractType.Dynamic)
            {
                if (isMoving)
                    return;

                prevAngle = openAngle;
                openAngle = targetAngle = flipOpenDirection
                    ? openLimits.min : openLimits.max;
                isOpened = true;
            }
            else if (InteractType == DynamicObject.InteractType.Animation && !Animator.IsAnyPlaying())
            {
                Animator.SetTrigger(DynamicObject.useTrigger1);
                DynamicObject.PlaySound(DynamicSoundType.Open);
                DynamicObject.useEvent1?.Invoke();
                isOpened = true;
            }
        }

        public override void OnDynamicClose()
        {
            if (InteractType == DynamicObject.InteractType.Dynamic)
            {
                if (isMoving)
                    return;

                prevAngle = openAngle;
                openAngle = targetAngle = flipOpenDirection
                    ? openLimits.max : openLimits.min;
                isOpened = false;
            }
            else if (InteractType == DynamicObject.InteractType.Animation && !Animator.IsAnyPlaying())
            {
                Animator.SetTrigger(DynamicObject.useTrigger2);
                DynamicObject.useEvent2?.Invoke();
                isCloseSound = true;
                isOpened = false;
            }
        }

        public override void OnDynamicLocked()
        {
            if (isLockedTry || !useLockedMotion)
                return;

            DynamicObject.StartCoroutine(OnLocked());
            isLockedTry = true;
        }

        IEnumerator OnLocked()
        {
            float elapsedTime = 0f;
            while (elapsedTime < lockedMotionTime)
            {
                float t = elapsedTime / lockedMotionTime;
                float pattern = lockedPattern.Evaluate(t) * lockedMotionAmount;
                SetOpenableAngle(currentAngle + pattern);
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            SetOpenableAngle(currentAngle);
            isLockedTry = false;
        }

        public override void OnDynamicUpdate()
        {
            float t = 0;

            if (InteractType == DynamicObject.InteractType.Dynamic)
            {
                t = Mathf.InverseLerp(prevAngle, openAngle, currentAngle);
                DynamicObject.onValueChange?.Invoke(t);
                isMoving = t > 0 && t < 1;

                float modifier = isOpened ? openCurve.Evaluate(t) : closeCurve.Evaluate(t);
                currentAngle = Mathf.MoveTowards(currentAngle, targetAngle, Time.deltaTime * openSpeed * 10 * modifier);
                SetOpenableAngle(currentAngle);

                if (!disableSounds)
                {
                    if (isOpened && !isOpenSound && t > 0.02f)
                    {
                        DynamicObject.PlaySound(DynamicSoundType.Open);
                        DynamicObject.useEvent1?.Invoke(); // open event
                        isOpenSound = true;
                    }
                    else if (!isOpened && isOpenSound && t > 0.95f)
                    {
                        DynamicObject.PlaySound(DynamicSoundType.Close);
                        DynamicObject.useEvent2?.Invoke(); // close event
                        isOpenSound = false;
                    }
                }
            }
            else if(InteractType == DynamicObject.InteractType.Mouse)
            {
                float angle = GetSignedAngle(targetHinge);
                angle = Mathf.Clamp(angle, openLimits.RealMin, openLimits.RealMax);

                // 1,1 = false, 0,0 = false, 0,1 or 1,0 = true
                bool flip = startingAngleFlip ^ flipAngle;
                float minAngle = flip ? openLimits.max : openLimits.min;
                float maxAngle = flip ? openLimits.min : openLimits.max;
                t = Mathf.InverseLerp(minAngle, maxAngle, angle);

                if (!disableSounds)
                {
                    if (!isOpened && t > 0.02f)
                    {
                        DynamicObject.PlaySound(DynamicSoundType.Open);
                        DynamicObject.useEvent1?.Invoke(); // open event
                        isOpened = true;
                    }
                    else if (isOpened && t < 0.01f)
                    {
                        DynamicObject.PlaySound(DynamicSoundType.Close);
                        DynamicObject.useEvent2?.Invoke(); // close event
                        isOpened = false;
                    }
                }

                if (dragSounds)
                {
                    float volumeMag = Mathf.Clamp01(Rigidbody.linearVelocity.magnitude);

                    if (volumeMag > dragSoundPlay && ((Vector2)openLimits).InRange(angle))
                    {
                        AudioSource.SetSoundClip(dragSound, volumeMag, true);
                    }
                    else
                    {
                        if (AudioSource.volume > 0.01f)
                        {
                            AudioSource.volume = Mathf.MoveTowards(AudioSource.volume, 0f, Time.deltaTime);
                        }
                        else
                        {
                            AudioSource.volume = 0f;
                            AudioSource.Stop();
                        }
                    }
                }
            }

            if(InteractType != DynamicObject.InteractType.Animation)
            {
                // value change event
                DynamicObject.onValueChange?.Invoke(t);
            }
            else if(playCloseSound && !isOpened && isCloseSound && !Animator.IsAnyPlaying())
            {
                // animation close sound
                DynamicObject.PlaySound(DynamicSoundType.Close);
                isCloseSound = false;
            }
        }

        private void SetOpenableAngle(float angle)
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
                float t = Mathf.InverseLerp(openLimits.min, openLimits.max, startingAngle);
                _startingAngle = Mathf.Lerp(openLimits.max, openLimits.min, t);
            }

            return _startingAngle;
        }


        public override void OnDynamicHold(Vector2 mouseDelta)
        {
            if (InteractType == DynamicObject.InteractType.Mouse && Joint != null)
            {
                mouseDelta.y = 0;
                if (mouseDelta.magnitude > 0)
                {
                    Joint.useMotor = true;
                    JointMotor motor = Joint.motor;
                    motor.targetVelocity = mouseDelta.x * openSpeed * 10 * (flipMouse ? -1 : 1);
                    Joint.motor = motor;
                }
                else
                {
                    Joint.useMotor = false;
                    JointMotor motor = Joint.motor;
                    motor.targetVelocity = 0f;
                    Joint.motor = motor;
                }
            }

            IsHolding = true;
        }

        public override void OnDynamicEnd()
        {
            if (InteractType == DynamicObject.InteractType.Mouse && Joint != null)
            {
                Joint.useMotor = false;
                JointMotor motor = Joint.motor;
                motor.targetVelocity = 0f;
                Joint.motor = motor;
            }

            IsHolding = false;
        }

        public override void OnDrawGizmos()
        {
            if (DynamicObject == null || Target == null || InteractType == DynamicObject.InteractType.Animation)
                return;

            int mirrorFwd = targetForwardMirror ? -1 : 1;
            int mirrorUpwd = targetHingeMirror ? -1 : 1;

            Vector3 upward = Application.isPlaying ? hingeAxisLocal : (Target.Direction(targetHinge) * mirrorUpwd);
            Vector3 forward = Application.isPlaying ? forwardAxisLocal : (Target.Direction(targetForward) * mirrorFwd);
            float radius = 0.3f;

            HandlesDrawing.DrawLimits(
                DynamicObject.transform.position,
                openLimits,
                forward,
                upward,
                bothSidesOpen,
                flipOpenDirection,
                radius
            );

            Vector3 startingDir = Quaternion.AngleAxis(GetStartingAngle(), upward) * forward;

            Gizmos.color = Color.red;
            Gizmos.DrawRay(Target.position, startingDir * (radius + 0.1f));

            if (bothSidesOpen)
            {
                Vector3 opForward = Application.isPlaying ? (frameFwd * -1) : Target.Direction(frameForward);
                if(flipOpenDirection) opForward *= -1;

                Gizmos.color = Color.green;
                Gizmos.DrawRay(Target.position, opForward * (radius + 0.1f));
            }
        }

        public override StorableCollection OnSave()
        {
            StorableCollection saveableBuffer = new StorableCollection();
            saveableBuffer.Add("rotation", Target.eulerAngles.ToSaveable());

            if (InteractType != DynamicObject.InteractType.Animation)
            {
                saveableBuffer.Add("targetAngle", targetAngle);
                saveableBuffer.Add("currentAngle", currentAngle);
                saveableBuffer.Add("openAngle", openAngle);
                saveableBuffer.Add("isOpenSound", isOpenSound);
                saveableBuffer.Add("disableSounds", disableSounds);
                saveableBuffer.Add("isOpened", isOpened);
            }

            return saveableBuffer;
        }

        public override void OnLoad(JToken token)
        {
            Target.eulerAngles = token["rotation"].ToObject<Vector3>();

            if (InteractType != DynamicObject.InteractType.Animation)
            {
                targetAngle = (float)token["targetAngle"];
                currentAngle = (float)token["currentAngle"];
                openAngle = (float)token["openAngle"];
                isOpenSound = (bool)token["isOpenSound"];
                disableSounds = (bool)token["disableSounds"];
                isOpened = (bool)token["isOpened"];
            }
        }
    }
}