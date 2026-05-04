using System;
using UnityEngine;
using UHFPS.Tools;
using Newtonsoft.Json.Linq;

namespace UHFPS.Runtime
{
    [Serializable]
    public class DynamicPullable : DynamicObjectType
    {
        [Tooltip("Limits that define the minimum/maximum position in which the pullable can be pulled.")]
        public MinMax openLimits;
        [Tooltip("The axis in which the object is to be pulled.")]
        public Axis pullAxis = Axis.Z;

        // pullable properties
        [Tooltip("The curve that defines the pull speed for modifier. 0 = start to 1 = end.")]
        public AnimationCurve openCurve = new(new(0, 1), new(1, 1));
        [Tooltip("Defines the pulling speed.")]
        public float openSpeed = 1f;
        [Tooltip("Defines the damping of the pullable object.")]
        public float damping = 1f;
        [Tooltip("Defines the minimum mouse input at which to play the drag sound.")]
        public float dragSoundPlay = 0.2f;

        [Tooltip("Enable pull sound when dragging the pullable with mouse.")]
        public bool dragSounds = true;
        [Tooltip("Flip the mouse drag direction.")]
        public bool flipMouse = false;

        // private
        private Vector3 targetPosition;
        private Vector3 startPosition;

        private float targetMove;
        private float mouseSmooth;

        private bool isOpened;
        private bool isMoving;

        public override bool IsOpened => isOpened;

        public override void OnDynamicInit()
        {
            if (InteractType == DynamicObject.InteractType.Dynamic)
            {
                startPosition = Target.localPosition.SetComponent(pullAxis, openLimits.min);
                targetPosition = startPosition;
            }
        }

        public override void OnDynamicStart(PlayerManager player)
        {
            if (!DynamicObject.isLocked)
            {
                if (InteractType == DynamicObject.InteractType.Dynamic && !isMoving)
                {
                    if (isOpened = !isOpened)
                    {
                        targetMove = openLimits.max;
                        DynamicObject.PlaySound(DynamicSoundType.Open);
                        DynamicObject.useEvent1?.Invoke();  // open event
                    }
                    else
                    {
                        targetMove = openLimits.min;
                        DynamicObject.PlaySound(DynamicSoundType.Close);
                        DynamicObject.useEvent2?.Invoke();  // close event
                    }

                    targetPosition = startPosition.SetComponent(pullAxis, targetMove);
                }
                else if (InteractType == DynamicObject.InteractType.Animation && !Animator.IsAnyPlaying())
                {
                    if (isOpened = !isOpened)
                    {
                        Animator.SetTrigger(DynamicObject.useTrigger1);
                        DynamicObject.PlaySound(DynamicSoundType.Open);
                        DynamicObject.useEvent1?.Invoke();  // open event
                    }
                    else
                    {
                        Animator.SetTrigger(DynamicObject.useTrigger2);
                        DynamicObject.PlaySound(DynamicSoundType.Close);
                        DynamicObject.useEvent2?.Invoke();  // close event
                    }
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
                targetMove = openLimits.min;
                DynamicObject.PlaySound(DynamicSoundType.Close);
                DynamicObject.useEvent2?.Invoke();

                isOpened = true;
                targetPosition = startPosition.SetComponent(pullAxis, targetMove);
            }
            else if (InteractType == DynamicObject.InteractType.Animation && !Animator.IsAnyPlaying())
            {
                Animator.SetTrigger(DynamicObject.useTrigger2);
                DynamicObject.PlaySound(DynamicSoundType.Close);
                DynamicObject.useEvent2?.Invoke();

                isOpened = true;
            }
        }

        public override void OnDynamicClose()
        {
            if (InteractType == DynamicObject.InteractType.Dynamic && !isMoving)
            {
                targetMove = openLimits.max;
                DynamicObject.PlaySound(DynamicSoundType.Open);
                DynamicObject.useEvent1?.Invoke();

                isOpened = true;
                targetPosition = startPosition.SetComponent(pullAxis, targetMove);
            }
            else if (InteractType == DynamicObject.InteractType.Animation && !Animator.IsAnyPlaying())
            {
                Animator.SetTrigger(DynamicObject.useTrigger1);
                DynamicObject.PlaySound(DynamicSoundType.Open);
                DynamicObject.useEvent1?.Invoke();

                isOpened = true;
            }
        }

        public override void OnDynamicUpdate()
        {
            float t = 0;

            if (InteractType == DynamicObject.InteractType.Dynamic)
            {
                float currentAxisPos = Target.localPosition.Component(pullAxis);
                t = Mathf.InverseLerp(openLimits.min, openLimits.max, currentAxisPos);

                isMoving = t > 0 && t < 1;
                float modifier = openCurve.Evaluate(t);

                Vector3 currentPos = Target.localPosition;
                currentPos = Vector3.MoveTowards(currentPos, targetPosition, Time.deltaTime * openSpeed * modifier);
                Target.localPosition = currentPos;
            }
            else if (InteractType == DynamicObject.InteractType.Mouse)
            {
                mouseSmooth = Mathf.MoveTowards(mouseSmooth, targetMove, Time.deltaTime * (targetMove != 0 ? openSpeed : damping));
                Target.Translate(mouseSmooth * Time.deltaTime * pullAxis.Convert(), Space.Self);

                Vector3 clampedPosition = Target.localPosition.Clamp(pullAxis, openLimits);
                Target.localPosition = clampedPosition;

                float currentAxisPos = Target.localPosition.Component(pullAxis);
                t = Mathf.InverseLerp(openLimits.min, openLimits.max, currentAxisPos);

                if(t > 0.99f && !isOpened)
                {
                    DynamicObject.useEvent1?.Invoke();  // open event
                    isOpened = true;
                }
                else if(t < 0.01f && isOpened)
                {
                    DynamicObject.useEvent2?.Invoke();  // close event
                    isOpened = false;
                }

                if (dragSounds && AudioSource != null)
                {
                    if (mouseSmooth > dragSoundPlay && currentAxisPos < openLimits.max)
                    {
                        AudioSource.SetSoundClip(DynamicObject.useSound1);
                        if (!AudioSource.isPlaying) AudioSource.Play();
                    }
                    else if (mouseSmooth < -dragSoundPlay && currentAxisPos > openLimits.min)
                    {
                        AudioSource.SetSoundClip(DynamicObject.useSound2);
                        if (!AudioSource.isPlaying) AudioSource.Play();
                    }
                    else
                    {
                        if(AudioSource.volume > 0.01f)
                        {
                            AudioSource.volume = Mathf.MoveTowards(AudioSource.volume, 0f, Time.deltaTime * 4f);
                        }
                        else
                        {
                            AudioSource.volume = 0f;
                            AudioSource.Stop();
                        }
                    }
                }
            }

            // value change event
            DynamicObject.onValueChange?.Invoke(t);
        }

        public override void OnDynamicHold(Vector2 mouseDelta)
        {
            if (InteractType == DynamicObject.InteractType.Mouse)
            {
                mouseDelta.x = 0;
                float mouseInput = Mathf.Clamp(mouseDelta.y, -1, 1) * (flipMouse ? 1 : -1);
                targetMove = mouseDelta.magnitude > 0 ? mouseInput : 0;
            }

            IsHolding = true;
        }

        public override void OnDynamicEnd()
        {
            if (InteractType == DynamicObject.InteractType.Mouse)
            {
                targetMove = 0;
            }

            IsHolding = false;
        }

        public override StorableCollection OnSave()
        {
            StorableCollection saveableBuffer = new StorableCollection();

            if(InteractType != DynamicObject.InteractType.Animation)
                saveableBuffer.Add("localPosition", Target.localPosition.ToSaveable());

            saveableBuffer.Add(nameof(isOpened), isOpened);
            return saveableBuffer;
        }

        public override void OnLoad(JToken token)
        {
            if (InteractType != DynamicObject.InteractType.Animation)
            {
                Target.localPosition = token["localPosition"].ToObject<Vector3>();
                startPosition = Target.localPosition;
                targetPosition = startPosition;
            }

            isOpened = (bool)token[nameof(isOpened)];
        }
    }
}