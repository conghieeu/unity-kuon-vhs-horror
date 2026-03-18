using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputHandler : MonoBehaviour
{
    private InputSystem_Actions inputActions;

    public Vector2 MoveInput { get; private set; }
    public Vector2 LookInput { get; private set; }
    public bool SprintPressed { get; private set; }
    public bool JumpPressed { get; private set; }

    // Additional actions for modular systems
    public event System.Action OnInteract;
    public event System.Action OnRightClick;
    public event System.Action OnCapturePhoto;

    private void Awake()
    {
        inputActions = new InputSystem_Actions();
    }

    private void OnEnable()
    {
        inputActions.Enable();

        inputActions.Player.Move.performed += ctx => MoveInput = ctx.ReadValue<Vector2>();
        inputActions.Player.Move.canceled += ctx => MoveInput = Vector2.zero;

        inputActions.Player.Look.performed += ctx => LookInput = ctx.ReadValue<Vector2>();
        inputActions.Player.Look.canceled += ctx => LookInput = Vector2.zero;

        inputActions.Player.Sprint.performed += ctx => SprintPressed = true;
        inputActions.Player.Sprint.canceled += ctx => SprintPressed = false;

        inputActions.Player.Jump.performed += ctx => JumpPressed = true;
        inputActions.Player.Jump.canceled += ctx => JumpPressed = false;
        
        // interact action
        inputActions.Player.Interact.performed += ctx => OnInteract?.Invoke();

        // Right click for camera mode
        inputActions.Player.SecondaryFire.performed += ctx => OnRightClick?.Invoke();

        // Space to capture photo
        inputActions.Player.Jump.performed += ctx => OnCapturePhoto?.Invoke();
    }

    private void OnDisable()
    {
        inputActions.Disable();
    }
}