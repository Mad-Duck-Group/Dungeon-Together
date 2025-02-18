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
    
    private NetworkVariable<bool> isFlipped = new NetworkVariable<bool>(false, 
        NetworkVariableReadPermission.Everyone, // ทุกคนสามารถอ่านค่าได้
        NetworkVariableWritePermission.Server); // เฉพาะ Server เท่านั้นที่แก้ไขค่าได้


    private void Start()
    {
        // ทุกครั้งที่ค่าเปลี่ยน จะเรียก Callback และอัปเดต SpriteRenderer
        isFlipped.OnValueChanged += (oldValue, newValue) =>
        {
            spriteRenderer.flipX = newValue;
        };
    }

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
            FlipServerRpc(movementInput.x < 0);
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
