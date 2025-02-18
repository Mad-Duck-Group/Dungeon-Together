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

    // Update is called once per frame
    void Update()
    {
        if (!IsOwner)
        {
            return;
        }
        movementInput.Normalize();
        
        FlipSprite();
    }
    
    private void FlipSprite()
    {
        if (movementInput.x > 0)
        {
            playerGameObject.transform.localScale = new Vector3(0.6375f, 1, 1);
            Debug.Log("Flip false");
        }
        else if (movementInput.x < 0)
        {
            playerGameObject.transform.localScale = new Vector3(-0.6375f, 1, 1);
            Debug.Log("Flip true");
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
