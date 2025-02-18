using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using static Controls;

[CreateAssetMenu(fileName = "InputReader", menuName = "Input/Input Reader")]
public class InputReader : ScriptableObject, IPlayerActions
{
    
    public event Action<Vector2> MoveEvent;
    
    private Controls _controls;
    
    private void OnEnable()
    {
        if (_controls == null)
        {
            _controls = new Controls();
            _controls.Player.SetCallbacks(this);
        }
        
        _controls.Player.Enable();
    }
    
    public void OnMove(InputAction.CallbackContext context)
    {
        MoveEvent?.Invoke(context.ReadValue<Vector2>());
    }

    public void OnAttack(InputAction.CallbackContext context)
    {
        Debug.Log("Attack");
    }
}
