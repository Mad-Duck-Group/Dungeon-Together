using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Serialization;

public class PlayerMovement : NetworkBehaviour
{
    
    [Header("References")]
    [SerializeField] private InputReader inputReader;
    [SerializeField] private Transform playerTransform;
    [SerializeField] private Rigidbody2D rb2d;
    [SerializeField] private GameObject playerGameObject;
    [SerializeField] private SpriteRenderer spriteRenderer;
    
    [Header("Settings")]
    [SerializeField] private float movementSpeed = 4f;

    private Vector2 movementInput;

    private NetworkVariable<bool> isFlipped = new();
    // Update is called once per frame
    void Update()
    {
        if (!IsOwner)
        {
            return;
        }
        movementInput.Normalize();
        Flip();
    }
    
    [Rpc(SendTo.Server)]
    private void FlipServerRpc(bool flipX)
    {
        isFlipped.Value = flipX;
    }

    
    private void Flip()
    {
        if (movementInput.x != 0)
        { 
            var shouldFlip = movementInput.x < 0;
            FlipServerRpc(shouldFlip);
            spriteRenderer.flipX = shouldFlip;
        }
    }
    
    
    void FixedUpdate()
    {
        if (!IsOwner) return;
        
        rb2d.linearVelocity = movementInput * movementSpeed;
    }
    
    private void HandleMove(Vector2 movement)
    {
        movementInput = movement;
    }
    
    public override void OnNetworkSpawn()
    {
        isFlipped.OnValueChanged += OnSpriteFlip;
        if (!IsOwner)
        {
            spriteRenderer.flipX = isFlipped.Value;
            return;
        }
        inputReader.MoveEvent += HandleMove;
    }
    
    public override void OnNetworkDespawn()
    {
        isFlipped.OnValueChanged -= OnSpriteFlip;
        if (!IsOwner)
        {
            return;
        }
        inputReader.MoveEvent -= HandleMove;
    }
    
    private void OnSpriteFlip(bool oldValue, bool newValue)
    {
        if (IsOwner)
        {
            return;
        }
        spriteRenderer.flipX = newValue;
    }
}
