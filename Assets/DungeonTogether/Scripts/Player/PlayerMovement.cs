using System;
using Unity.Netcode;
using UnityEngine;

public class PlayerMovement : NetworkBehaviour
{
    
    [Header("References")]
    [SerializeField] private InputReader inputReader;
    [SerializeField] private Transform playerTransform;
    [SerializeField] private Rigidbody2D rigidbody2D;
    
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
        Movement();
    }
    
    private void FixedUpdate()
    {
        if (!IsOwner)
        {
            return;
        } 
        rigidbody2D.linearVelocity = movementInput * movementSpeed;
    }

    private void Movement()
    {
        movementInput.x = Input.GetAxisRaw("Horizontal");
        movementInput.y = Input.GetAxisRaw("Vertical");
        movementInput.Normalize();
        
        playerTransform.position += (Vector3)movementInput * movementSpeed * Time.deltaTime;
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
