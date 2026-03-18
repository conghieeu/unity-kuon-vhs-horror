using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class MovementSystem : MonoBehaviour
{
    [SerializeField] float walkSpeed = 3f;
    [SerializeField] float sprintSpeed = 6f;
    [SerializeField] float gravity = -9.81f;
    [SerializeField] float jumpHeight = 1.5f;
    [SerializeField] Transform cameraPivot;
    
    // Movement acceleration settings
    [SerializeField] float acceleration = 10f;
    [SerializeField] float deceleration = 15f;
    

    private CharacterController controller;
    private PlayerInputHandler input;

    private Vector3 velocity;
    private bool isGrounded;
    private float currentSpeed = 0f;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        input = GetComponent<PlayerInputHandler>();
    }

    private void Update()
    {
        HandleMovement();
    }

    void HandleMovement()
    {
        isGrounded = controller.isGrounded;

        if (isGrounded && velocity.y < 0)
            velocity.y = -2f;

        Vector2 moveInput = input.MoveInput;

        Vector3 forward = cameraPivot != null 
            ? Vector3.ProjectOnPlane(cameraPivot.forward, Vector3.up).normalized 
            : Vector3.ProjectOnPlane(transform.forward, Vector3.up).normalized;
        Vector3 right = cameraPivot != null 
            ? Vector3.ProjectOnPlane(cameraPivot.right, Vector3.up).normalized 
            : Vector3.ProjectOnPlane(transform.right, Vector3.up).normalized;

        Vector3 move = right * moveInput.x + forward * moveInput.y;

        // Determine target speed based on input and sprint state
        float targetSpeed = 0f;
        if (moveInput.sqrMagnitude > 0f)
        {
            targetSpeed = input.SprintPressed ? sprintSpeed : walkSpeed;
        }

        // Accelerate or decelerate towards target speed
        if (currentSpeed < targetSpeed)
        {
            currentSpeed = Mathf.Min(currentSpeed + acceleration * Time.deltaTime, targetSpeed);
        }
        else if (currentSpeed > targetSpeed)
        {
            currentSpeed = Mathf.Max(currentSpeed - deceleration * Time.deltaTime, targetSpeed);
        }

        // Apply movement with current speed
        if (currentSpeed > 0f)
        {
            controller.Move(move.normalized * currentSpeed * Time.deltaTime);
        }

        // Jump
        if (input.JumpPressed && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        // Gravity
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }


}