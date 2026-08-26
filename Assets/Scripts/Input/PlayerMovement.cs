using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerInputHandler input;

    [Header("Movement")]
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float sprintSpeed = 8f;

    [Header("Jump & Gravity")]
    [SerializeField] private float jumpHeight = 1.2f;
    [SerializeField] private float gravity = -9.81f;
    [SerializeField] private float coyoteTime = 0.15f;
    [SerializeField] private float jumpBufferTime = 0.15f;

    private CharacterController controller;
    private Vector3 verticalVelocity;
    private bool jumpQueued;
    private PlatformMover currentPlatform;

    private float coyoteTimer;
    private float jumpBufferTimer;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
    }

    private void OnEnable()
    {
        input.Jumped += QueueJump;
    }

    private void OnDisable()
    {
        input.Jumped -= QueueJump;
    }

    private void QueueJump()
    {
        jumpQueued = true;
        jumpBufferTimer = jumpBufferTime;
    }

    private void Update()
    {
        bool grounded = controller.isGrounded;

        Vector3 platformDelta = Vector3.zero;
        if (currentPlatform != null)
        {
            platformDelta = currentPlatform.FrameDelta;
            currentPlatform = null; // re-set below by OnControllerColliderHit if still touching
        }

        UpdateTimers(grounded);
        ApplyJumpAndGravity();
        ApplyHorizontalMovement(platformDelta);
    }

    private void UpdateTimers(bool grounded)
    {
        // Coyote time: keep a short grace window after leaving ground
        coyoteTimer = grounded ? coyoteTime : coyoteTimer - Time.deltaTime;

        // Jump buffer: keep remembering a recent Jump press for a short window
        if (jumpBufferTimer > 0f)
        {
            jumpBufferTimer -= Time.deltaTime;
        }
    }

    private void ApplyJumpAndGravity()
    {
        bool canJump = coyoteTimer > 0f;
        bool wantsToJump = jumpBufferTimer > 0f;

        if (canJump && wantsToJump)
        {
            verticalVelocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            coyoteTimer = 0f;      // used up, can't double-jump off the same window
            jumpBufferTimer = 0f;  // consumed
        }
        else if (controller.isGrounded && verticalVelocity.y < 0f)
        {
            verticalVelocity.y = -2f; // keeps the controller firmly grounded
        }

        jumpQueued = false;
        verticalVelocity.y += gravity * Time.deltaTime;
    }
        
    private void ApplyHorizontalMovement(Vector3 platformDelta)
    {
        float speed = input.SprintHeld ? sprintSpeed : walkSpeed;
        Vector3 horizontal = (transform.right * input.MoveInput.x + transform.forward * input.MoveInput.y) * speed;

        controller.Move((horizontal + verticalVelocity) * Time.deltaTime + platformDelta);
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (hit.normal.y > 0.5f && hit.collider.TryGetComponent<PlatformMover>(out var platform))
        {
            currentPlatform = platform;
        }
    }
}