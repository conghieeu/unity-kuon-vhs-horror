using System;
using System.Collections;
using UnityEngine;
using UHFPS.Input;
using UHFPS.Tools;

namespace UHFPS.Runtime
{
    public class LookController : PlayerComponent
    {
        public enum ForwardStyle { RootForward, LookForward }

        public ForwardStyle PlayerForward = ForwardStyle.LookForward;

        public bool LockCursor;
        public bool SmoothLook;

        public float SensitivityX = 2f;
        public float SensitivityY = 2f;
        public float MultiplierX = 1f;
        public float MultiplierY = 1f;

        public float SmoothTime = 5f;
        public float SmoothMultiplier = 2f;

        public MinMax HorizontalLimits = new(-360, 360);
        public MinMax VerticalLimits = new(-80, 90);

        public Vector2 LookOffset;
        public Vector2 LookRotation;

        private bool blockLook;
        private MinMax horizontalLimitsOrig;
        private MinMax verticalLimitsOrig;

        private Vector2 targetLook;
        private Vector2 startingLook;
        private bool customLerp;

        public Vector2 DeltaInput { get; set; }
        public Quaternion RotationX { get; private set; }
        public Quaternion RotationY { get; private set; }
        public Quaternion RotationFinal { get; private set; }

        public Vector3 LookForward2D
        {
            get
            {
                Vector3 lookDirection = RotationX * Vector3.forward;
                return new(lookDirection.x, 0f, lookDirection.z);
            }
        }

        public Vector3 LookForward => RotationX * Vector3.forward;
        public Vector3 LookCross => Vector3.Cross(LookForward2D, Vector3.up);

        public bool LookLocked
        {
            get => blockLook;
            set => blockLook = value;
        }

        void Start()
        {
            verticalLimitsOrig = VerticalLimits;
            horizontalLimitsOrig = HorizontalLimits;
            if (LockCursor) GameTools.ShowCursor(true, false);

            OptionsManager.ObserveOption("sensitivity", (obj) =>
            {
                SensitivityX = (float)obj;
                SensitivityY = (float)obj;
            });

            OptionsManager.ObserveOption("smoothing", (obj) => SmoothLook = (bool)obj);
            OptionsManager.ObserveOption("smoothing_speed", (obj) => SmoothTime = (float)obj);
        }

        void Update()
        {
            if (Cursor.lockState != CursorLockMode.None && !blockLook && isEnabled)
            {
                DeltaInput = InputManager.ReadInput<Vector2>(Controls.LOOK);
            }
            else
            {
                DeltaInput = Vector2.zero;
            }

            LookRotation.x += DeltaInput.x * (SensitivityX * MultiplierX) / 30 * MainCamera.fieldOfView + LookOffset.x;
            LookRotation.y += DeltaInput.y * (SensitivityY * MultiplierY) / 30 * MainCamera.fieldOfView + LookOffset.y;

            LookRotation.x = ClampAngle(LookRotation.x, HorizontalLimits.RealMin, HorizontalLimits.RealMax);
            LookRotation.y = ClampAngle(LookRotation.y, VerticalLimits.RealMin, VerticalLimits.RealMax);

            RotationX = Quaternion.AngleAxis(LookRotation.x, Vector3.up);
            RotationY = Quaternion.AngleAxis(LookRotation.y, Vector3.left);
            RotationFinal = RotationX * RotationY;

            if (PlayerForward == ForwardStyle.LookForward)
            {
                transform.localRotation = SmoothLook ? Quaternion.Slerp(transform.localRotation, RotationFinal, SmoothTime * SmoothMultiplier * Time.deltaTime) : RotationFinal;
            }
            else
            {
                Transform root = PlayerManager.transform;
                root.localRotation = SmoothLook ? Quaternion.Slerp(root.localRotation, RotationX, SmoothTime * SmoothMultiplier * Time.deltaTime) : RotationX;
                transform.localRotation = SmoothLook ? Quaternion.Slerp(transform.localRotation, RotationY, SmoothTime * SmoothMultiplier * Time.deltaTime) : RotationY;
            }

            LookOffset.y = 0F;
            LookOffset.x = 0F;
        }

        /// <summary>
        /// Parent look controller to fix rotation snapping.
        /// </summary>
        public void ParentToObject(Transform parent)
        {
            if (PlayerForward == ForwardStyle.LookForward)
                return;

            Quaternion parentOffset = Quaternion.Inverse(parent.rotation) * RotationFinal;
            LookRotation.x = parentOffset.eulerAngles.y;
        }

        /// <summary>
        /// Unparent look controller from parent to fix rotation snapping.
        /// </summary>
        public void UnparentFromObject()
        {
            if (PlayerForward == ForwardStyle.LookForward)
                return;

            Transform parent = PlayerManager.transform.parent;
            Quaternion parentOffset = parent.rotation * RotationFinal;
            LookRotation.x = parentOffset.eulerAngles.y;
        }

        /// <summary>
        /// Transform wish direction to look direction.
        /// </summary>
        public Vector3 TransformWishDir(Vector3 wishDir)
        {
            if (PlayerForward == ForwardStyle.LookForward)
                return RotationX * wishDir;
            else if(PlayerForward == ForwardStyle.RootForward)
                return PlayerManager.transform.TransformDirection(wishDir);

            return wishDir;
        }

        /// <summary>
        /// Lerp look rotation to a specific target rotation.
        /// </summary>
        public void LerpRotation(Vector2 target, float duration = 0.5f)
        {
            target.x = ClampAngle(target.x);
            target.y = ClampAngle(target.y);

            float xDiff = FixDiff(target.x - LookRotation.x);
            float yDiff = FixDiff(target.y - LookRotation.y);

            StartCoroutine(DoLerpRotation(new Vector2(xDiff, yDiff), null, duration));
        }

        /// <summary>
        /// Lerp look rotation to a specific target rotation.
        /// </summary>
        public void LerpRotation(Vector2 target, Action onLerpComplete, float duration = 0.5f)
        {
            target.x = ClampAngle(target.x);
            target.y = ClampAngle(target.y);

            float xDiff = FixDiff(target.x - LookRotation.x);
            float yDiff = FixDiff(target.y - LookRotation.y);

            StartCoroutine(DoLerpRotation(new Vector2(xDiff, yDiff), onLerpComplete, duration));
        }

        /// <summary>
        /// Lerp look rotation to a specific target transform.
        /// </summary>
        public void LerpRotation(Transform target, float duration = 0.5f, bool keepLookLocked = false)
        {
            Vector3 directionToTarget = target.position - transform.position;
            Quaternion rotationToTarget = Quaternion.LookRotation(directionToTarget);

            Vector3 eulerRotation = rotationToTarget.eulerAngles;
            Vector2 targetRotation = new Vector2(eulerRotation.y, eulerRotation.x);

            // Clamp the target rotation angles.
            targetRotation.x = ClampAngle(targetRotation.x);
            targetRotation.y = ClampAngle(-targetRotation.y);

            // Calculate the differences in each axis.
            float xDiff = FixDiff(targetRotation.x - LookRotation.x);
            float yDiff = FixDiff(targetRotation.y - LookRotation.y);

            // Start the lerp process.
            StartCoroutine(DoLerpRotation(new Vector2(xDiff, yDiff), null, duration, keepLookLocked));
        }

        /// <summary>
        /// Lerp look rotation to a specific target transform.
        /// </summary>
        public void LerpRotation(Transform target, Action onLerpComplete, float duration = 0.5f, bool keepLookLocked = false)
        {
            Vector3 directionToTarget = target.position - transform.position;
            Quaternion rotationToTarget = Quaternion.LookRotation(directionToTarget);

            Vector3 eulerRotation = rotationToTarget.eulerAngles;
            Vector2 targetRotation = new Vector2(eulerRotation.y, eulerRotation.x);

            // Clamp the target rotation angles.
            targetRotation.x = ClampAngle(targetRotation.x);
            targetRotation.y = ClampAngle(targetRotation.y);

            // Calculate the differences in each axis.
            float xDiff = FixDiff(targetRotation.x - LookRotation.x);
            float yDiff = FixDiff(targetRotation.y - LookRotation.y);

            // Start the lerp process.
            StartCoroutine(DoLerpRotation(new Vector2(xDiff, yDiff), onLerpComplete, duration, keepLookLocked));
        }

        /// <summary>
        /// Lerp the look rotation and clamp the look rotation within limits relative to the rotation.
        /// </summary>
        /// <param name="relative">Relative target rotation.</param>
        /// <param name="vLimits">Vertical Limits [Up, Down]</param>
        /// <param name="hLimits">Horizontal Limits [Left, Right]</param>
        public void LerpClampRotation(Vector3 relative, MinMax vLimits, MinMax hLimits, float duration = 0.5f)
        {
            float toAngle = ClampAngle(relative.y);
            float remainder = FixDiff(toAngle - LookRotation.x);

            float targetAngle = LookRotation.x + remainder;
            float min = targetAngle - Mathf.Abs(hLimits.RealMin);
            float max = targetAngle + Mathf.Abs(hLimits.RealMax);

            if (min < -360)
            {
                min += 360;
                max += 360;
                targetAngle += 360;
            }
            else if (max > 360)
            {
                min -= 360;
                max -= 360;
                targetAngle -= 360;
            }

            hLimits = new MinMax(min, max);
            StartCoroutine(DoLerpClampRotation(targetAngle, vLimits, hLimits, duration));
        }

        /// <summary>
        /// Lerp the look rotation manually. This function should only be used in the Update() function.
        /// </summary>
        public void CustomLerp(Vector2 target, float t)
        {
            if (!customLerp)
            {
                targetLook.x = ClampAngle(target.x);
                targetLook.y = ClampAngle(target.y);
                startingLook = LookRotation;
                customLerp = true;
                blockLook = true;
            }

            if ((t = Mathf.Clamp01(t)) < 1)
            {
                LookRotation.x = Mathf.LerpAngle(startingLook.x, targetLook.x, t);
                LookRotation.y = Mathf.LerpAngle(startingLook.y, targetLook.y, t);
            }
        }

        /// <summary>
        /// Reset lerp parameters.
        /// </summary>
        public void ResetCustomLerp()
        {
            StopAllCoroutines();
            targetLook = Vector2.zero;
            startingLook = Vector2.zero;
            customLerp = false;
            blockLook = false;
        }

        /// <summary>
        /// Set look rotation limits.
        /// </summary>
        /// <param name="relative">Relative target rotation.</param>
        /// <param name="vLimits">Vertical Limits [Up, Down]</param>
        /// <param name="hLimits">Horizontal Limits [Left, Right]</param>
        public void SetLookLimits(Vector3 relative, MinMax vLimits, MinMax hLimits)
        {
            if (hLimits.HasValue)
            {
                float toAngle = ClampAngle(relative.y);
                float remainder = FixDiff(toAngle - LookRotation.x);

                float targetAngle = LookRotation.x + remainder;
                float min = targetAngle - Mathf.Abs(hLimits.RealMin);
                float max = targetAngle + Mathf.Abs(hLimits.RealMax);

                if (min < -360)
                {
                    min += 360;
                    max += 360;
                }
                else if (max > 360)
                {
                    min -= 360;
                    max -= 360;
                }

                if (Mathf.Abs(targetAngle - LookRotation.x) > 180)
                {
                    if (LookRotation.x > 0) LookRotation.x -= 360;
                    else if (LookRotation.x < 0) LookRotation.x += 360;
                }

                hLimits = new MinMax(min, max);
                HorizontalLimits = hLimits;
            }

            VerticalLimits = vLimits;
        }

        /// <summary>
        /// Set vertical look rotation limits.
        /// </summary>
        /// <param name="vLimits">Vertical Limits [Up, Down]</param>
        public void SetVerticalLimits(MinMax vLimits)
        {
            VerticalLimits = vLimits;
        }

        /// <summary>
        /// Set horizontal look rotation limits.
        /// </summary>
        /// <param name="relative">Relative target rotation.</param>
        /// <param name="hLimits">Horizontal Limits [Left, Right]</param>
        public void SetHorizontalLimits(Vector3 relative, MinMax hLimits)
        {
            float toAngle = ClampAngle(relative.y);
            float remainder = FixDiff(toAngle - LookRotation.x);

            float targetAngle = LookRotation.x + remainder;
            float min = targetAngle - Mathf.Abs(hLimits.RealMin);
            float max = targetAngle + Mathf.Abs(hLimits.RealMax);

            if (min < -360)
            {
                min += 360;
                max += 360;
            }
            else if (max > 360)
            {
                min -= 360;
                max -= 360;
            }

            if (Mathf.Abs(targetAngle - LookRotation.x) > 180)
            {
                if (LookRotation.x > 0) LookRotation.x -= 360;
                else if (LookRotation.x < 0) LookRotation.x += 360;
            }

            hLimits = new MinMax(min, max);
            HorizontalLimits = hLimits;
        }

        /// <summary>
        /// Reset look rotation to default limits.
        /// </summary>
        public void ResetLookLimits()
        {
            StopAllCoroutines();
            HorizontalLimits = horizontalLimitsOrig;
            VerticalLimits = verticalLimitsOrig;
        }

        /// <summary>
        /// Apply look rotation using a euler angles vector.
        /// </summary>
        /// <remarks>Good to use when you want to set the look rotation from a custom camera.</remarks>
        public void ApplyEulerLook(Vector2 eulerAngles)
        {
            // Clamp the target rotation angles.
            eulerAngles.x = ClampAngle(eulerAngles.x);
            eulerAngles.y = ClampAngle(eulerAngles.y);

            // Calculate the differences in each axis.
            float xDiff = FixDiff(eulerAngles.x - LookRotation.x);
            float yDiff = FixDiff(eulerAngles.y - LookRotation.y);

            LookRotation = new(LookRotation.x + xDiff, LookRotation.y + yDiff);
        }

        private IEnumerator DoLerpRotation(Vector2 target, Action onLerpComplete, float duration, bool keepLookLocked = false)
        {
            blockLook = true;

            target = new Vector2(LookRotation.x + target.x, LookRotation.y + target.y);
            Vector2 current = LookRotation;
            float elapsedTime = 0;

            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                float t = GameTools.SmootherStep(0f, 1f, elapsedTime / duration);

                LookRotation.x = Mathf.LerpAngle(current.x, target.x, t);
                LookRotation.y = Mathf.LerpAngle(current.y, target.y, t);

                yield return null;
            }

            LookRotation = target;
            onLerpComplete?.Invoke();

            blockLook = keepLookLocked;
        }

        private IEnumerator DoLerpClampRotation(float newX, Vector2 vLimit, Vector2 hLimit, float duration, bool keepLookLocked = false)
        {
            blockLook = true;

            float newY = LookRotation.y < vLimit.x
                ? vLimit.x : LookRotation.y > vLimit.y
                ? vLimit.y : LookRotation.y;

            Vector2 target = new Vector2(newX, newY);
            Vector2 current = LookRotation;
            float elapsedTime = 0;

            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                float t = GameTools.SmootherStep(0f, 1f, elapsedTime / duration);

                LookRotation.x = Mathf.LerpAngle(current.x, target.x, t);
                LookRotation.y = Mathf.LerpAngle(current.y, target.y, t);

                yield return null;
            }

            LookRotation = target;
            HorizontalLimits = hLimit;
            VerticalLimits = vLimit;

            blockLook = keepLookLocked;
        }

        private float ClampAngle(float angle, float min, float max)
        {
            float newAngle = angle.FixAngle();
            return Mathf.Clamp(newAngle, min, max);
        }

        private float ClampAngle(float angle)
        {
            angle %= 360f;
            if (angle < 0f)
                angle += 360f;
            return angle;
        }

        private float FixDiff(float angleDiff)
        {
            if (angleDiff > 180f)
            {
                angleDiff -= 360f;
            }
            else if (angleDiff < -180f)
            {
                angleDiff += 360f;
            }

            return angleDiff;
        }
    }
}