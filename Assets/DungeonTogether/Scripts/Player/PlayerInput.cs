using System;
using System.Collections;
using System.Collections.Generic;
using TriInspector;
using Unity.Netcode;
using UnityCommunity.UnitySingleton;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInput : NetworkBehaviour
{
    [Serializable]
    public class InputButton
    {
        public Coroutine buttonCoroutine;
        public bool isDown;
        public bool isUp;
        public bool isHeld;
    }
    
    [SerializeField, Tooltip("Input reader scriptable object")]
    private InputReader inputReader;

    [SerializeField, ReadOnly] private Vector2 movementInput;
    public Vector2 MovementInput => movementInput;
    [SerializeField, ReadOnly] private InputButton attackButton;
    public InputButton AttackButton => attackButton;

    #region Subscriptions

    private void Subscribe()
    {
        inputReader.MoveEvent += HandleMove;
        inputReader.AttackEvent += HandleAttack;
    }
    
    private void Unsubscribe()
    {
        inputReader.MoveEvent -= HandleMove;
        inputReader.AttackEvent -= HandleAttack;
    }
    private void OnEnable()
    {
        Subscribe();
    }
    
    private void OnDisable()
    {
        Unsubscribe();
    }
    // public override void OnNetworkSpawn()
    // {
    //     Subscribe();
    // }
    //
    // public override void OnNetworkDespawn()
    // {
    //     Unsubscribe();
    // }
    #endregion

    #region Event Handlers
    private void HandleMove(InputAction.CallbackContext context)
    {
        movementInput = context.ReadValue<Vector2>();
    }
    private void HandleAttack(InputAction.CallbackContext context)
    {
        BindButton(attackButton, context);
    }

    private void BindButton(InputButton button, InputAction.CallbackContext context)
    {
        button.isDown = context.performed;
        button.isUp = context.canceled;
        button.isHeld = context.performed;
        if (button.buttonCoroutine != null)
        {
            StopCoroutine(button.buttonCoroutine);
        }
        button.buttonCoroutine = StartCoroutine(ButtonCoroutine(button));
    }

    private IEnumerator ButtonCoroutine(InputButton button)
    {
        yield return new WaitForEndOfFrame();
        button.isDown = false;
        button.isUp = false;
        button.buttonCoroutine = null;
    }
    #endregion
}
