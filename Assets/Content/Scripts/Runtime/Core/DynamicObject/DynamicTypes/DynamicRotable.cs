using System;
using UnityEngine;
using UHFPS.Tools;
using Newtonsoft.Json.Linq;

namespace UHFPS.Runtime
{
    [Serializable]
    public class DynamicRotable : DynamicObjectType
    {
        // limits
        [Tooltip("The maximum limit at which rotable can be rotated.")]
        public float rotationLimit = 360;
        [Tooltip("The axis around which to rotate.")]
        public Axis rotateAroundAxis = Axis.Z;

        // rotable properties
        [Tooltip("The curve that defines the rotable speed for modifier. 0 = start to 1 = end.")]
        public AnimationCurve rotateCurve = new(new(0, 1), new(1, 1));
        [Tooltip("Defines the rotation speed.")]
        public float rotationSpeed = 2f;
        [Tooltip("Mouse multiplier to adjust mouse input.")]
        public float mouseMultiplier = 1f;
        [Tooltip("Defines the damping of the rotable object.")]
        public float damping = 1f;

        [Tooltip("Hold use button to rotate the object.")]
        public bool holdToRotate = true;
        [Tooltip("When the maximum limit is reached, lock the rotable object.")]
        public bool lockOnRotate = false;
        [Tooltip("Show the rotable gizmos to visualize the limits.")]
        public bool showGizmos = true;

        // private
        private float currentAngle;
        private float targetAngle;

        private float mouseSmooth;
        private float targetMove;

        private bool isHolding;
        private bool isRotated;
        private bool isMoving;
        private bool isRotateLocked;
        private bool isTurnSound;

        private Vector3 rotableForward;

        public override bool ShowGizmos => showGizmos;

        public override bool IsOpened => isRotated;

        public override void OnDynamicInit()
        {
            rotableForward = Target.Direction(rotateAroundAxis);
            targetAngle = rotationLimit;
        }

        public override void OnDynamicStart(PlayerManager player)
        {
            if (isRotateLocked) return;

            if (!DynamicObject.isLocked)
            {
                if (InteractType == DynamicObject.InteractType.Dynamic)
                {
                    if (lockOnRotate)
                    {
                        targetAngle = rotationLimit;
                        DynamicObject.useEvent1?.Invoke();  // rotate on event
                    }
                    else if (!isMoving)
                    {
                        if(isRotated = !isRotated)
                        {
                            targetAngle = rotationLimit;
                        }
                        else
                        {
                            targetAngle = 0;
                        }
                    }

                    isHolding = true;
                }
                else if (InteractType == DynamicObject.InteractType.Animation && !Animator.IsAnyPlaying())
                {
                    if (isRotated = !isRotated)
                    {
                        Animator.SetTrigger(DynamicObject.useTrigger1);
                        DynamicObject.useEvent1?.Invoke();  // rotate on event
                    }
                    else
                    {
                        Animator.SetTrigger(DynamicObject.useTrigger2);
                        DynamicObject.useEvent2?.Invoke();  // rotate off event
                    }

                    if (lockOnRotate) isRotateLocked = true;
                }
            }
            else
            {
                TryUnlock();
            }
        }

        public override void OnDynamicOpen()
        {
            if (InteractType == DynamicObject.InteractType.Dynamic)
            {
                targetAngle = rotationLimit;
                DynamicObject.useEvent1?.Invoke();
                isRotated = true;
            }
            else if (InteractType == DynamicObject.InteractType.Animation && !Animator.IsAnyPlaying())
            {
                Animator.SetTrigger(DynamicObject.useTrigger1);
                DynamicObject.useEvent1?.Invoke();  // rotate on event

                isRotated = true;
                if (lockOnRotate) isRotateLocked = true;
            }
        }

        public override void OnDynamicClose()
        {
            if (InteractType == DynamicObject.InteractType.Dynamic)
            {
                targetAngle = 0;
                DynamicObject.useEvent2?.Invoke();
                isRotated = false;
            }
            else if (InteractType == DynamicObject.InteractType.Animation && !Animator.IsAnyPlaying())
            {
                Animator.SetTrigger(DynamicObject.useTrigger2);
                DynamicObject.useEvent2?.Invoke();

                isRotated = false;
                if (lockOnRotate) isRotateLocked = true;
            }
        }

        public override void OnDynamicUpdate()
        {
            float t = 0;

            if(InteractType == DynamicObject.InteractType.Dynamic)
            {
                t = Mathf.InverseLerp(0, rotationLimit, currentAngle);
                isMoving = t > 0 && t < 1;

                float modifier = rotateCurve.Evaluate(t);
                if ((holdToRotate && isHolding) || !holdToRotate) 
                    currentAngle = Mathf.MoveTowards(currentAngle, targetAngle, Time.deltaTime * rotationSpeed * 10 * modifier);

                Vector3 rotation = Target.localEulerAngles;
                rotation = rotation.SetComponent(rotateAroundAxis, currentAngle);
                Target.localEulerAngles = rotation;
            }
            else if(InteractType == DynamicObject.InteractType.Mouse)
            {
                t = Mathf.InverseLerp(0, rotationLimit, currentAngle);
                if (lockOnRotate) { if (t >= 1) isRotateLocked = true; }

                mouseSmooth = Mathf.MoveTowards(mouseSmooth, targetMove, Time.deltaTime * (targetMove != 0 ? rotationSpeed : damping));
                currentAngle = Mathf.Clamp(currentAngle + mouseSmooth, 0, rotationLimit);

                Vector3 rotation = Target.localEulerAngles;
                rotation = rotation.SetComponent(rotateAroundAxis, currentAngle);
                Target.localEulerAngles = rotation;
            }

            if(InteractType != DynamicObject.InteractType.Animation)
            {
                if (t >= 1f && !isRotated)
                {
                    DynamicObject.useEvent1?.Invoke();  // rotate on event
                    isRotated = true;
                }
                else if (t <= 0f && isRotated)
                {
                    DynamicObject.useEvent2?.Invoke();  // rotate off event
                    isRotated = false;
                }

                if(t > 0.05f && !isTurnSound && !isRotated)
                {
                    DynamicObject.PlaySound(DynamicSoundType.Open);
                    isTurnSound = true;
                }
                else if(t < 0.95f && isTurnSound && isRotated)
                {
                    DynamicObject.PlaySound(DynamicSoundType.Close);
                    isTurnSound = false;
                }

                // value change event
                DynamicObject.onValueChange?.Invoke(t);
            }
        }

        public override void OnDynamicHold(Vector2 mouseDelta)
        {
            if(InteractType == DynamicObject.InteractType.Mouse && !isRotateLocked)
            {
                mouseDelta.x = 0;
                float mouseInput = Mathf.Clamp(mouseDelta.y, -1, 1) * mouseMultiplier;
                targetMove = mouseDelta.magnitude > 0 ? mouseInput : 0;
            }

            IsHolding = true;
        }

        public override void OnDynamicEnd()
        {
            if (InteractType == DynamicObject.InteractType.Dynamic)
            {
                isHolding = false;
            }
            else if (InteractType == DynamicObject.InteractType.Mouse)
            {
                targetMove = 0;
            }

            IsHolding = false;
        }

        public override void OnDrawGizmos()
        {
            if (DynamicObject == null || Target == null || InteractType == DynamicObject.InteractType.Animation) return;

            Vector3 forward = Application.isPlaying ? rotableForward : Target.Direction(rotateAroundAxis);
            Gizmos.color = Color.red;
            Gizmos.DrawRay(Target.position, forward * 0.1f);
        }

        public override StorableCollection OnSave()
        {
            StorableCollection saveableBuffer = new StorableCollection();

            if (InteractType != DynamicObject.InteractType.Animation)
            {
                saveableBuffer.Add("rotation", Target.eulerAngles.ToSaveable());
                saveableBuffer.Add("angle", currentAngle);
                saveableBuffer.Add(nameof(isTurnSound), isTurnSound);
            }

            saveableBuffer.Add(nameof(isRotateLocked), isRotateLocked);
            saveableBuffer.Add(nameof(isRotated), isRotated);
            return saveableBuffer;
        }

        public override void OnLoad(JToken token)
        {
            if (InteractType != DynamicObject.InteractType.Animation)
            {
                Target.eulerAngles = token["rotation"].ToObject<Vector3>();
                currentAngle = (float)token["angle"];
                isTurnSound = (bool)token[nameof(isTurnSound)];
            }

            isRotateLocked = (bool)token[nameof(isRotateLocked)];
            isRotated = (bool)token[nameof(isRotated)];
        }
    }
}