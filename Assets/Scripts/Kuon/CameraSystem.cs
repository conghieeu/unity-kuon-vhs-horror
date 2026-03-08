using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInputHandler))]
public class CameraSystem : MonoBehaviour
{
    [SerializeField] Camera mainCamera;
    [SerializeField] float normalFOV = 60f;
    [SerializeField] float sprintFOV = 75f;
    [SerializeField] float zoomSpeed = 10f;
    [SerializeField] LayerMask ghostLayerMask = 0; // Layer chứa bóng ma, chỉ hiển thị trong photo mode

    private PlayerInputHandler input;
    private bool isPhotoMode = false;
    private int originalCullingMask;
    private InputSystem_Actions inputActions;

    private void Awake()
    {
        input = GetComponent<PlayerInputHandler>();
        inputActions = new InputSystem_Actions();
        if (mainCamera == null)
            mainCamera = Camera.main;
        
        originalCullingMask = mainCamera.cullingMask;
    }

    private void OnEnable()
    {
        // Subscribe to input events
        input.OnRightClick += TogglePhotoMode;
        input.OnCapturePhoto += CapturePhoto;
    }

    private void OnDisable()
    {
        // Unsubscribe from input events
        input.OnRightClick -= TogglePhotoMode;
        input.OnCapturePhoto -= CapturePhoto;
    }

    private void Update()
    {
        if (mainCamera == null) return;

        if (isPhotoMode)
        {
            // Photo mode - zoom và di chuyển camera để tìm bóng ma
            HandlePhotoCameraInput();
        }
        else
        {
            // Normal mode - movement thường
            float targetFOV = input.SprintPressed ? sprintFOV : normalFOV;
            mainCamera.fieldOfView = Mathf.Lerp(mainCamera.fieldOfView, targetFOV, zoomSpeed * Time.deltaTime);
        }
    }

    private void TogglePhotoMode()
    {
        isPhotoMode = !isPhotoMode;
        
        if (isPhotoMode)
        {
            // Bật photo mode - hiển thị layer bóng ma
            // Thêm ghost layer vào culling mask
            mainCamera.cullingMask |= ghostLayerMask;
            Debug.Log("📷 CAMERA MODE ON - Tìm bóng ma...");
        }
        else
        {
            // Tắt photo mode - ẩn bóng ma, trở về bình thường
            mainCamera.cullingMask = originalCullingMask;
            Debug.Log("📷 CAMERA MODE OFF");
        }
    }

    private void HandlePhotoCameraInput()
    {
        // Zoom bằng scroll wheel
        float scroll = inputActions.Player.Look.ReadValue<Vector2>().y * 0.1f; // Dùng Look input hoặc mouse scroll
        
        // Hoặc dùng Mouse scroll trực tiếp từ InputSystem
        var mouse = Mouse.current;
        if (mouse != null)
        {
            float mouseScroll = mouse.scroll.y.ReadValue() * 0.01f;
            if (Mathf.Abs(mouseScroll) > 0.01f)
            {
                mainCamera.fieldOfView = Mathf.Clamp(mainCamera.fieldOfView - mouseScroll * 5f, 20f, 90f);
            }
        }
    }

    private void CapturePhoto()
    {
        if (!isPhotoMode) return; // Chỉ chụp khi ở photo mode
        
        string filename = System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss") + ".png";
        string path = System.IO.Path.Combine(Application.persistentDataPath, filename);
        ScreenCapture.CaptureScreenshot(filename);
        Debug.Log($"📸 Chụp ảnh: {path}");
    }

    public bool IsPhotoMode => isPhotoMode;
}