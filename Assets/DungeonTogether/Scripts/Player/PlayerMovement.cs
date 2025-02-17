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
    [SerializeField] private SpriteRenderer spriteRenderer;
    
    [Header("Settings")]
    [SerializeField] private float movementSpeed = 4f;

    private Vector2 movementInput;

    // Update is called once per frame
    void Update()
    {
        if (!IsOwner)
        {
            return;
        }
        movementInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        movementInput.Normalize();
        
        Debug.Log($"Movement Input: {movementInput}");
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
        if (!IsOwner)
        {
            return;
        }
        inputReader.MoveEvent += HandleMove;
    }
    
    public override void OnNetworkDespawn()
    {
        if (!IsOwner)
        {
            return;
        }
        inputReader.MoveEvent -= HandleMove;
    }
}
