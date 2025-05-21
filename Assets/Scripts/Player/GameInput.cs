using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameInput : MonoBehaviour
{
    public static GameInput Instance { get; private set; }
    public event EventHandler OnInteractAction;
    public event EventHandler OnInteractAltAction;
    public event EventHandler OnPauseAction;
    private Actions inputAction;
    private void Awake()
    {
        Instance = this;
        inputAction = new Actions();
        inputAction.Player.Enable();
        inputAction.Player.Interact.performed += Interact_Perform;
        inputAction.Player.InteractAlt.performed += InteractAlt_Perform;
        inputAction.Player.Pause.performed += Pause_performed;
    }

    private void OnDestroy()
    {
        inputAction.Player.Interact.performed -= Interact_Perform;
        inputAction.Player.InteractAlt.performed -= InteractAlt_Perform;
        inputAction.Player.Pause.performed -= Pause_performed;
        inputAction.Dispose();

    }
    private void Pause_performed(InputAction.CallbackContext obj)
    {
        OnPauseAction?.Invoke(this, EventArgs.Empty);
    }

    private void InteractAlt_Perform(InputAction.CallbackContext context)
    {
        OnInteractAltAction?.Invoke(this,EventArgs.Empty);
    }

    private void Interact_Perform(InputAction.CallbackContext obj)
    {
        OnInteractAction?.Invoke(this,EventArgs.Empty);
    }

    public Vector2 GetMovementVector()
    {
        Vector2 inputVector = inputAction.Player.Movement.ReadValue<Vector2>();

        inputVector = inputVector.normalized;
        return inputVector;
    }
}
