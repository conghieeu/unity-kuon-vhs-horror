using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Standalone first-person look controller.
/// Full feature parity with UHFPS LookController but with ZERO external dependencies.
/// Works independently — just attach to a camera or camera holder GameObject.
/// </summary>
public class StandaloneLookController : MonoBehaviour
{
    // ──────────────────────────────────────────────
    // Enums & Structs
    // ──────────────────────────────────────────────

    public enum ForwardStyle { RootForward, LookForward }

    [Serializable]
    public struct FloatRange
    {
        public float min;
        public float max;

        public bool Flipped => max < min;
        public bool HasValue => min != 0 || max != 0;
        public float RealMin => Mathf.Min(min, max);
        public float RealMax => Mathf.Max(max, min);

        public FloatRange(float min, float max)
        {
            this.min = min;
            this.max = max;
        }

        public static implicit operator Vector2(FloatRange r) => new(r.RealMin, r.RealMax);
        public static implicit operator FloatRange(Vector2 v)
        {
            FloatRange r = default;
            r.min = v.x;
            r.max = v.y;
            return r;
        }

        public override string ToString() => $"({RealMin}, {RealMax})";
    }

    // ──────────────────────────────────────────────
    // Inspector Fields
    // ──────────────────────────────────────────────

    [Header("General")]
    public ForwardStyle PlayerForward = ForwardStyle.LookForward;
    public bool LockCursor = true;
    public bool SmoothLook;

    [Header("Sensitivity")]
    public float SensitivityX = 2f;
    public float SensitivityY = 2f;
    public float MultiplierX = 1f;
    public float MultiplierY = 1f;

    [Header("Smoothing")]
    public float SmoothTime = 5f;
    public float SmoothMultiplier = 2f;

    [Header("Limits")]
    public FloatRange HorizontalLimits = new(-360, 360);
    public FloatRange VerticalLimits = new(-80, 90);

    [Header("References (auto-found if empty)")]
    [Tooltip("The camera used for FOV-based sensitivity. Auto-finds Camera.main if empty.")]
    public Camera MainCamera;

    [Tooltip("The root player transform (only needed for RootForward style).")]
    public Transform PlayerRoot;

    [Header("Input")]
    [Tooltip("Input action for mouse/stick look delta. If empty, falls back to Mouse.current delta.")]
    public InputActionReference LookInputAction;

    // ──────────────────────────────────────────────
    // Runtime State
    // ──────────────────────────────────────────────

    [HideInInspector] public Vector2 LookOffset;
    [HideInInspector] public Vector2 LookRotation;

    private bool blockLook;
    private FloatRange horizontalLimitsOrig;
    private FloatRange verticalLimitsOrig;

    private Vector2 targetLook;
    private Vector2 startingLook;
    private bool customLerp;

    // ──────────────────────────────────────────────
    // Public Properties
    // ──────────────────────────────────────────────

    public Vector2 DeltaInput { get; set; }
    public Quaternion RotationX { get; private set; }
    public Quaternion RotationY { get; private set; }
    public Quaternion RotationFinal { get; private set; }

    /// <summary>
    /// 2D forward vector projected onto the XZ plane.
    /// </summary>
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

    /// <summary>
    /// Enable/disable the controller without destroying it.
    /// </summary>
    public bool IsEnabled { get; set; } = true;

    // ──────────────────────────────────────────────
    // Unity Lifecycle
    // ──────────────────────────────────────────────

    private void Start()
    {
        verticalLimitsOrig = VerticalLimits;
        horizontalLimitsOrig = HorizontalLimits;

        if (MainCamera == null)
            MainCamera = Camera.main;

        if (PlayerRoot == null && PlayerForward == ForwardStyle.RootForward)
            PlayerRoot = transform.root;

        if (LockCursor)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        // Enable the input action if provided
        if (LookInputAction != null && LookInputAction.action != null)
            LookInputAction.action.Enable();
    }

    private void OnDestroy()
    {
        if (LookInputAction != null && LookInputAction.action != null)
            LookInputAction.action.Disable();
    }

    private void Update()
    {
        // ── Read Input ──
        if (Cursor.lockState != CursorLockMode.None && !blockLook && IsEnabled)
        {
            DeltaInput = ReadLookInput();
        }
        else
        {
            DeltaInput = Vector2.zero;
        }

        // ── FOV-adjusted sensitivity ──
        float fov = MainCamera != null ? MainCamera.fieldOfView : 60f;
        LookRotation.x += DeltaInput.x * (SensitivityX * MultiplierX) / 30f * fov + LookOffset.x;
        LookRotation.y += DeltaInput.y * (SensitivityY * MultiplierY) / 30f * fov + LookOffset.y;

        // ── Clamp ──
        LookRotation.x = ClampAngle(LookRotation.x, HorizontalLimits.RealMin, HorizontalLimits.RealMax);
        LookRotation.y = ClampAngle(LookRotation.y, VerticalLimits.RealMin, VerticalLimits.RealMax);

        // ── Build Quaternions ──
        RotationX = Quaternion.AngleAxis(LookRotation.x, Vector3.up);
        RotationY = Quaternion.AngleAxis(LookRotation.y, Vector3.left);
        RotationFinal = RotationX * RotationY;

        // ── Apply Rotation ──
        if (PlayerForward == ForwardStyle.LookForward)
        {
            transform.localRotation = SmoothLook
                ? Quaternion.Slerp(transform.localRotation, RotationFinal, SmoothTime * SmoothMultiplier * Time.deltaTime)
                : RotationFinal;
        }
        else
        {
            if (PlayerRoot != null)
            {
                PlayerRoot.localRotation = SmoothLook
                    ? Quaternion.Slerp(PlayerRoot.localRotation, RotationX, SmoothTime * SmoothMultiplier * Time.deltaTime)
                    : RotationX;
            }
            transform.localRotation = SmoothLook
                ? Quaternion.Slerp(transform.localRotation, RotationY, SmoothTime * SmoothMultiplier * Time.deltaTime)
                : RotationY;
        }

        LookOffset.y = 0f;
        LookOffset.x = 0f;
    }

    // ──────────────────────────────────────────────
    // Input Reading
    // ──────────────────────────────────────────────

    private Vector2 ReadLookInput()
    {
        // Priority 1: InputActionReference
        if (LookInputAction != null && LookInputAction.action != null)
            return LookInputAction.action.ReadValue<Vector2>();

        // Priority 2: Raw mouse delta
        if (Mouse.current != null)
            return Mouse.current.delta.ReadValue();

        return Vector2.zero;
    }

    // ──────────────────────────────────────────────
    // Parenting Helpers (for moving platforms, etc.)
    // ──────────────────────────────────────────────

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

        Transform rootParent = PlayerRoot != null ? PlayerRoot.parent : transform.root.parent;
        if (rootParent == null) return;

        Quaternion parentOffset = rootParent.rotation * RotationFinal;
        LookRotation.x = parentOffset.eulerAngles.y;
    }

    // ──────────────────────────────────────────────
    // Wish Direction
    // ──────────────────────────────────────────────

    /// <summary>
    /// Transform wish direction to look direction.
    /// </summary>
    public Vector3 TransformWishDir(Vector3 wishDir)
    {
        if (PlayerForward == ForwardStyle.LookForward)
            return RotationX * wishDir;
        if (PlayerForward == ForwardStyle.RootForward && PlayerRoot != null)
            return PlayerRoot.TransformDirection(wishDir);
        return wishDir;
    }

    // ──────────────────────────────────────────────
    // Lerp Rotation (multiple overloads)
    // ──────────────────────────────────────────────

    /// <summary>Lerp look rotation to a specific target rotation.</summary>
    public void LerpRotation(Vector2 target, float duration = 0.5f)
    {
        target.x = ClampAngle360(target.x);
        target.y = ClampAngle360(target.y);

        float xDiff = FixDiff(target.x - LookRotation.x);
        float yDiff = FixDiff(target.y - LookRotation.y);

        StartCoroutine(DoLerpRotation(new Vector2(xDiff, yDiff), null, duration));
    }

    /// <summary>Lerp look rotation to a specific target rotation with callback.</summary>
    public void LerpRotation(Vector2 target, Action onLerpComplete, float duration = 0.5f)
    {
        target.x = ClampAngle360(target.x);
        target.y = ClampAngle360(target.y);

        float xDiff = FixDiff(target.x - LookRotation.x);
        float yDiff = FixDiff(target.y - LookRotation.y);

        StartCoroutine(DoLerpRotation(new Vector2(xDiff, yDiff), onLerpComplete, duration));
    }

    /// <summary>Lerp look rotation to face a specific target transform.</summary>
    public void LerpRotation(Transform target, float duration = 0.5f, bool keepLookLocked = false)
    {
        Vector3 directionToTarget = target.position - transform.position;
        Quaternion rotationToTarget = Quaternion.LookRotation(directionToTarget);

        Vector3 eulerRotation = rotationToTarget.eulerAngles;
        Vector2 targetRotation = new(eulerRotation.y, eulerRotation.x);

        targetRotation.x = ClampAngle360(targetRotation.x);
        targetRotation.y = ClampAngle360(-targetRotation.y);

        float xDiff = FixDiff(targetRotation.x - LookRotation.x);
        float yDiff = FixDiff(targetRotation.y - LookRotation.y);

        StartCoroutine(DoLerpRotation(new Vector2(xDiff, yDiff), null, duration, keepLookLocked));
    }

    /// <summary>Lerp look rotation to face a specific target transform with callback.</summary>
    public void LerpRotation(Transform target, Action onLerpComplete, float duration = 0.5f, bool keepLookLocked = false)
    {
        Vector3 directionToTarget = target.position - transform.position;
        Quaternion rotationToTarget = Quaternion.LookRotation(directionToTarget);

        Vector3 eulerRotation = rotationToTarget.eulerAngles;
        Vector2 targetRotation = new(eulerRotation.y, eulerRotation.x);

        targetRotation.x = ClampAngle360(targetRotation.x);
        targetRotation.y = ClampAngle360(targetRotation.y);

        float xDiff = FixDiff(targetRotation.x - LookRotation.x);
        float yDiff = FixDiff(targetRotation.y - LookRotation.y);

        StartCoroutine(DoLerpRotation(new Vector2(xDiff, yDiff), onLerpComplete, duration, keepLookLocked));
    }

    // ──────────────────────────────────────────────
    // Lerp + Clamp Rotation
    // ──────────────────────────────────────────────

    /// <summary>
    /// Lerp the look rotation and clamp within limits relative to the rotation.
    /// </summary>
    /// <param name="relative">Relative target rotation.</param>
    /// <param name="vLimits">Vertical Limits [Up, Down]</param>
    /// <param name="hLimits">Horizontal Limits [Left, Right]</param>
    public void LerpClampRotation(Vector3 relative, FloatRange vLimits, FloatRange hLimits, float duration = 0.5f)
    {
        float toAngle = ClampAngle360(relative.y);
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

        hLimits = new FloatRange(min, max);
        StartCoroutine(DoLerpClampRotation(targetAngle, vLimits, hLimits, duration));
    }

    // ──────────────────────────────────────────────
    // Custom Lerp (manual, call from Update)
    // ──────────────────────────────────────────────

    /// <summary>
    /// Lerp the look rotation manually. Call this in Update().
    /// </summary>
    public void CustomLerp(Vector2 target, float t)
    {
        if (!customLerp)
        {
            targetLook.x = ClampAngle360(target.x);
            targetLook.y = ClampAngle360(target.y);
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

    /// <summary>Reset lerp parameters.</summary>
    public void ResetCustomLerp()
    {
        StopAllCoroutines();
        targetLook = Vector2.zero;
        startingLook = Vector2.zero;
        customLerp = false;
        blockLook = false;
    }

    // ──────────────────────────────────────────────
    // Look Limits
    // ──────────────────────────────────────────────

    /// <summary>
    /// Set look rotation limits.
    /// </summary>
    /// <param name="relative">Relative target rotation.</param>
    /// <param name="vLimits">Vertical Limits [Up, Down]</param>
    /// <param name="hLimits">Horizontal Limits [Left, Right]</param>
    public void SetLookLimits(Vector3 relative, FloatRange vLimits, FloatRange hLimits)
    {
        if (hLimits.HasValue)
        {
            float toAngle = ClampAngle360(relative.y);
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

            hLimits = new FloatRange(min, max);
            HorizontalLimits = hLimits;
        }

        VerticalLimits = vLimits;
    }

    /// <summary>Set vertical look rotation limits.</summary>
    public void SetVerticalLimits(FloatRange vLimits)
    {
        VerticalLimits = vLimits;
    }

    /// <summary>Set horizontal look rotation limits.</summary>
    public void SetHorizontalLimits(Vector3 relative, FloatRange hLimits)
    {
        float toAngle = ClampAngle360(relative.y);
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

        hLimits = new FloatRange(min, max);
        HorizontalLimits = hLimits;
    }

    /// <summary>Reset look rotation to default limits.</summary>
    public void ResetLookLimits()
    {
        StopAllCoroutines();
        HorizontalLimits = horizontalLimitsOrig;
        VerticalLimits = verticalLimitsOrig;
    }

    // ──────────────────────────────────────────────
    // Apply Euler Look
    // ──────────────────────────────────────────────

    /// <summary>
    /// Apply look rotation using euler angles.
    /// Good to use when you want to set the look rotation from a custom camera.
    /// </summary>
    public void ApplyEulerLook(Vector2 eulerAngles)
    {
        eulerAngles.x = ClampAngle360(eulerAngles.x);
        eulerAngles.y = ClampAngle360(eulerAngles.y);

        float xDiff = FixDiff(eulerAngles.x - LookRotation.x);
        float yDiff = FixDiff(eulerAngles.y - LookRotation.y);

        LookRotation = new(LookRotation.x + xDiff, LookRotation.y + yDiff);
    }

    // ──────────────────────────────────────────────
    // Public Enable/Disable
    // ──────────────────────────────────────────────

    public void SetEnabled(bool enabled)
    {
        IsEnabled = enabled;
    }

    // ──────────────────────────────────────────────
    // Cursor Management
    // ──────────────────────────────────────────────

    /// <summary>
    /// Hiển thị cursor ra màn hình.
    /// </summary>
    public void ShowCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    /// <summary>
    /// Ẩn cursor đi.
    /// </summary>
    public void HideCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // ──────────────────────────────────────────────
    // Coroutines
    // ──────────────────────────────────────────────

    private IEnumerator DoLerpRotation(Vector2 target, Action onLerpComplete, float duration, bool keepLookLocked = false)
    {
        blockLook = true;

        target = new Vector2(LookRotation.x + target.x, LookRotation.y + target.y);
        Vector2 current = LookRotation;
        float elapsedTime = 0;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = SmootherStep(0f, 1f, elapsedTime / duration);

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

        Vector2 target = new(newX, newY);
        Vector2 current = LookRotation;
        float elapsedTime = 0;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = SmootherStep(0f, 1f, elapsedTime / duration);

            LookRotation.x = Mathf.LerpAngle(current.x, target.x, t);
            LookRotation.y = Mathf.LerpAngle(current.y, target.y, t);

            yield return null;
        }

        LookRotation = target;
        HorizontalLimits = hLimit;
        VerticalLimits = vLimit;

        blockLook = keepLookLocked;
    }

    // ──────────────────────────────────────────────
    // Math Utilities (self-contained, no external deps)
    // ──────────────────────────────────────────────

    /// <summary>Clamp angle within min/max, fixing wrap-around.</summary>
    private static float ClampAngle(float angle, float min, float max)
    {
        float newAngle = FixAngle(angle);
        return Mathf.Clamp(newAngle, min, max);
    }

    /// <summary>Wrap angle to 0-360 range.</summary>
    private static float ClampAngle360(float angle)
    {
        angle %= 360f;
        if (angle < 0f)
            angle += 360f;
        return angle;
    }

    /// <summary>Fix angle difference to be in -180..180 range.</summary>
    private static float FixDiff(float angleDiff)
    {
        if (angleDiff > 180f)
            angleDiff -= 360f;
        else if (angleDiff < -180f)
            angleDiff += 360f;
        return angleDiff;
    }

    /// <summary>Fix angle to -360..360 range (matches UHFPS GameTools.FixAngle).</summary>
    private static float FixAngle(float angle)
    {
        if (angle < -360f)
            angle += 360f;
        if (angle > 360f)
            angle -= 360f;
        return angle;
    }

    /// <summary>SmootherStep interpolation (5th-order Hermite).</summary>
    private static float SmootherStep(float start, float end, float t)
    {
        t = Mathf.Clamp01(t);
        t = t * t * t * (t * (6f * t - 15f) + 10f);
        return Mathf.Lerp(start, end, t);
    }
}
