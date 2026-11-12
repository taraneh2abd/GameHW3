using System;
using UnityEngine;
using UnityEngine.InputSystem;

[CreateAssetMenu(fileName = "InputReader", menuName = "Input/Input Reader")]
public class InputReader : ScriptableObject, GameControls.IPlayerActions
{
    private GameControls controls;

    public Vector2 Move { get; private set; }
    public bool FireHeld { get; private set; }

    public event Action MissilePressed;
    public event Action PausePressed;

    private void OnEnable()
    {
        if (controls == null)
        {
            controls = new GameControls();
            controls.Player.SetCallbacks(this);
        }
        controls.Player.Enable();
    }

    private void OnDisable()
    {
        if (controls == null)
        {
            return;
        }
        controls.Player.Disable();
        Move = Vector2.zero;
        FireHeld = false;
    }

    public void EnableGameplay()
    {
        if (controls == null)
        {
            OnEnable();
            return;
        }
        controls.Player.Enable();
    }

    public void DisableGameplay()
    {
        if (controls == null)
        {
            return;
        }
        controls.Player.Disable();
        Move = Vector2.zero;
        FireHeld = false;
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        Move = context.ReadValue<Vector2>();
    }

    public void OnFire(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            FireHeld = true;
        }
        else if (context.canceled)
        {
            FireHeld = false;
        }
    }

    public void OnMissile(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            MissilePressed?.Invoke();
        }
    }

    public void OnPause(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            PausePressed?.Invoke();
        }
    }
}
