using UnityEngine;

public class LookSystem : MonoBehaviour
{
    [SerializeField] Transform cameraPivot;
    [SerializeField] float sensitivity = 2f;
    [SerializeField] float maxLookAngle = 80f;

    private PlayerInputHandler input;
    private float xRotation = 0f;
    private float yRotation = 0f;

    private void Awake()
    {
        input = GetComponent<PlayerInputHandler>();
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update()
    {
        HandleLook();
    }

    void HandleLook()
    {
        Vector2 lookInput = input.LookInput * sensitivity;

        float mouseX = lookInput.x;
        float mouseY = lookInput.y;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -maxLookAngle, maxLookAngle);

        yRotation += mouseX;

        cameraPivot.localRotation = Quaternion.Euler(xRotation, yRotation, 0f);
    }
}