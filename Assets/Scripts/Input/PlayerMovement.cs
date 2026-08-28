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

    [Header("Push")]
    [SerializeField] private float pushStrength = 2.5f;
    [SerializeField] private LayerMask pushLayers = ~0;

    private CharacterController controller;
    private Vector3 verticalVelocity;
    private bool jumpQueued;

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

    private void QueueJump() => jumpQueued = true;

    private void Update()
    {
        bool grounded = controller.isGrounded;

        ApplyJumpAndGravity(grounded);
        ApplyHorizontalMovement(grounded);
    }

    private void ApplyJumpAndGravity(bool grounded)
    {
        if (grounded && verticalVelocity.y < 0f)
        {
            verticalVelocity.y = -2f; // keeps the controller firmly grounded
        }

        if (jumpQueued && grounded)
        {
            verticalVelocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }
        jumpQueued = false;

        verticalVelocity.y += gravity * Time.deltaTime;
    }

    private void ApplyHorizontalMovement(bool grounded)
    {
        float speed = input.SprintHeld ? sprintSpeed : walkSpeed;
        Vector3 horizontal = (transform.right * input.MoveInput.x + transform.forward * input.MoveInput.y) * speed;

        controller.Move((horizontal + verticalVelocity) * Time.deltaTime);
    }

    // CharacterController does not push dynamic Rigidbodies on its own.
    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        Rigidbody body = hit.collider.attachedRigidbody;
        if (body == null || body.isKinematic) return;
        if ((pushLayers.value & (1 << hit.gameObject.layer)) == 0) return;
        if (hit.moveDirection.y < -0.3f) return; // don't shove things we're standing on

        Vector3 dir = new Vector3(hit.moveDirection.x, 0f, hit.moveDirection.z).normalized;
        float speed = input.SprintHeld ? sprintSpeed : walkSpeed;
        Vector3 desired = dir * pushStrength * (speed / walkSpeed);

        // only add speed in the push direction, never yank the body back
        if (Vector3.Dot(body.linearVelocity, dir) < desired.magnitude)
            body.AddForceAtPosition(desired, hit.point, ForceMode.VelocityChange);
    }
}