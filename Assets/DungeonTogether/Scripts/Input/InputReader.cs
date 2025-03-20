using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using static Controls;

[CreateAssetMenu(fileName = "InputReader", menuName = "Input/Input Reader")]
public class InputReader : ScriptableObject, IPlayerActions
{
    public event Action<InputAction.CallbackContext> MoveEvent;
    public event Action<InputAction.CallbackContext> AttackEvent;
    public event Action<InputAction.CallbackContext> SkillEvent;
    
    private Controls _controls;
    
    private void OnEnable()
    {
        if (_controls == null)
        {
            _controls = new Controls();
            _controls.Player.SetCallbacks(this);
        }
        
        _controls?.Player.Enable();
    }

    private void OnDisable()
    {
        _controls?.Player.Disable();
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        MoveEvent?.Invoke(context);
    }

    public void OnAttack(InputAction.CallbackContext context)
    {
        AttackEvent?.Invoke(context);
    }
    public void OnSkill(InputAction.CallbackContext context)
    {
        SkillEvent?.Invoke(context);
    }
}
