using System;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Reads raw input from the New Input System and exposes it.
/// No gameplay logic lives here � this is a pure input layer.
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

    private bool _scrambled;

    private void Awake()
    {
        controls = new PlayerControls();

        if (PlayerPrefs.HasKey("rebinds"))
        {
            string json = PlayerPrefs.GetString("rebinds");
            controls.asset.LoadBindingOverridesFromJson(json);
        }

        // First playthrough gag: reverse forward/backward. Same condition the narrator lines use
        // (GameProgress.IsFirstRun); lifted mid-run by UnscrambleControls() at the end of Level 1.
        if (GameProgress.IsFirstRun)
            ScrambleForwardBack();
    }

    /// <summary>Swap the Move composite's up/down (W/S) bindings. Not persisted — session-only.</summary>
    private void ScrambleForwardBack()
    {
        var move = controls.Player.Move;   // 2DVector composite: [0]=header, [1]=up, [2]=down, [3]=left, [4]=right
        string up = move.bindings[1].effectivePath;
        string down = move.bindings[2].effectivePath;
        move.ApplyBindingOverride(1, down);
        move.ApplyBindingOverride(2, up);
        _scrambled = true;
    }

    /// <summary>Undo the first-run scramble and restore the player's real saved bindings.</summary>
    public void UnscrambleControls()
    {
        if (!_scrambled) return;
        _scrambled = false;
        controls.asset.RemoveAllBindingOverrides();
        if (PlayerPrefs.HasKey("rebinds"))
            controls.asset.LoadBindingOverridesFromJson(PlayerPrefs.GetString("rebinds"));
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