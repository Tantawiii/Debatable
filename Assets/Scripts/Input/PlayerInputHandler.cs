using System;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Reads raw input from the New Input System and exposes it.
/// No gameplay logic lives here — this is a pure input layer.
/// </summary>
public class PlayerInputHandler : MonoBehaviour
{
    private PlayerControls controls;

    // Continuous input (polled every frame by whoever needs it)
    public Vector2 MoveInput { get; private set; }
    public bool SprintHeld { get; private set; }
    public Vector2 LookInput { get; private set; }

    // One-shot input (event-driven)
    public event Action Jumped;
    public event Action ShootPressed;
    public event Action ReloadPressed;
    public event Action InteractPressed;

    private void Awake()
    {
        controls = new PlayerControls();
    }

    private void OnEnable()
    {
        controls.Player.Enable();

        controls.Player.Move.performed += OnMove;
        controls.Player.Move.canceled += OnMove;

        controls.Player.Sprint.performed += OnSprint;
        controls.Player.Sprint.canceled += OnSprint;

        controls.Player.Look.performed += OnLook;
        controls.Player.Look.canceled += OnLook;

        controls.Player.Jump.performed += _ => Jumped?.Invoke();
        controls.Player.Shoot.performed += _ => ShootPressed?.Invoke();
        controls.Player.Reload.performed += _ => ReloadPressed?.Invoke();
        controls.Player.Interact.performed += _ => InteractPressed?.Invoke();
    }

    private void OnDisable()
    {
        controls.Player.Move.performed -= OnMove;
        controls.Player.Move.canceled -= OnMove;
        controls.Player.Sprint.performed -= OnSprint;
        controls.Player.Sprint.canceled -= OnSprint;
        controls.Player.Look.performed -= OnLook;
        controls.Player.Look.canceled -= OnLook;

        controls.Player.Disable();
    }

    private void OnMove(InputAction.CallbackContext ctx) => MoveInput = ctx.ReadValue<Vector2>();
    private void OnSprint(InputAction.CallbackContext ctx) => SprintHeld = ctx.ReadValueAsButton();
    private void OnLook(InputAction.CallbackContext ctx) => LookInput = ctx.ReadValue<Vector2>();
}