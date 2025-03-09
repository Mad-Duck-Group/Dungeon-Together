using TriInspector;
using Unity.Netcode;
using UnityEngine;

namespace DungeonTogether.Scripts.Character.Module
{
    /// <summary>
    /// Module responsible for handling character movement.
    /// </summary>
    public class CharacterMovementModule : CharacterModule
    {
        [Title("References")]
        [SerializeField] private Rigidbody2D rb2d;
        [SerializeField] private SpriteRenderer spriteRenderer;
    
        [Title("Movement Settings")]
        [SerializeField] private float movementSpeed = 4f;

        private Vector2 moveDirection;

        private NetworkVariable<bool> isFlipped = new();
  
        public override void OnNetworkSpawn()
        {
            isFlipped.OnValueChanged += OnSpriteFlip;
            if (!IsOwner)
            {
                spriteRenderer.flipX = isFlipped.Value;
            }
        }
    
        public override void OnNetworkDespawn()
        {
            isFlipped.OnValueChanged -= OnSpriteFlip;
        }

        [Rpc(SendTo.Server)]
        private void FlipServerRpc(bool flipX)
        {
            isFlipped.Value = flipX;
        }
    
        /// <summary>
        /// Flips the sprite based on the direction of movement.
        /// </summary>
        private void Flip()
        {
            if (moveDirection.x != 0)
            { 
                var shouldFlip = moveDirection.x < 0;
                FlipServerRpc(shouldFlip);
                spriteRenderer.flipX = shouldFlip;
            }
        }
    
        private void OnSpriteFlip(bool oldValue, bool newValue)
        {
            if (IsOwner)
            {
                return;
            }
            spriteRenderer.flipX = newValue;
        }

        protected override void UpdateModule()
        {
            if (!IsOwner) return;
            base.UpdateModule();
            rb2d.linearVelocity = moveDirection * movementSpeed;
            Flip();
        }
        /// <summary>
        /// Sets the direction of movement.
        /// </summary>
        /// <param name="direction">Direction of movement.</param>
        public void SetDirection(Vector2 direction)
        {
            moveDirection = direction;
            moveDirection.Normalize();
            var state = moveDirection.magnitude > 0
                ? CharacterStates.CharacterMovementState.Walking
                : CharacterStates.CharacterMovementState.Idle;
            characterHub.ChangeMovementState(state);
        }
        
        protected override void HandleInput()
        {
            if (characterHub.CharacterType is not CharacterType.Player) return;
            base.HandleInput();
            SetDirection(PlayerInput.MovementInput);
        }
    }
}
