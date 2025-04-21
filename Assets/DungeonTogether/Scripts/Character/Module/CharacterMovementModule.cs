using System.Collections;
using DungeonTogether.Scripts.Utils;
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
        [Title("Movement References")]
        [SerializeField] private Collider2D playerCollider;
        [SerializeField] private Rigidbody2D rb2d;
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private Animator animator;
    
        [Title("Movement Settings")]
        [field: SerializeField] public float MovementSpeed { get; private set; } = 4f;

        [Title("Movement Debug")]
        [SerializeField, ReadOnly] private Vector2 moveDirection;
        
        private NetworkVariable<bool> isFlipped = new();
        private Coroutine dashColliderCoroutine;
        private static readonly int IsMoving = Animator.StringToHash("IsMoving");

        public override void OnNetworkSpawn()
        {
            isFlipped.OnValueChanged += OnSpriteFlip;
            EventBus<ConditionStateEvent>.Event += OnConditionStateChange;
            if (!IsOwner)
            {
                spriteRenderer.flipX = isFlipped.Value;
            }
            
        }
        private void OnConditionStateChange(ConditionStateEvent eventData)
        {
            if (eventData.characterHub != characterHub) return;
            if (eventData.newState != CharacterConditionState.Dead) return;
            rb2d.linearVelocity = Vector2.zero;
            moveDirection = Vector2.zero;
            characterHub.ChangeMovementState(CharacterMovementState.Idle);
        }

        public override void OnNetworkDespawn()
        {
            isFlipped.OnValueChanged -= OnSpriteFlip;
            EventBus<ConditionStateEvent>.Event -= OnConditionStateChange;
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
            if (characterHub.MovementState == CharacterMovementState.Dashing) return;
            rb2d.linearVelocity = moveDirection * MovementSpeed;
            Flip();
        }

        protected override void LateUpdate()
        {
            if (!IsOwner) return;
            LateUpdateModule();
        }

        protected override void LateUpdateModule()
        {
            if (moveDirection.magnitude <= 0 
                && characterHub.MovementState != CharacterMovementState.Dashing)
            {
                rb2d.linearVelocity = Vector2.zero;
            }
            base.LateUpdateModule();
        }

        /// <summary>
        /// Sets the direction of movement.
        /// </summary>
        /// <param name="direction">Direction of movement.</param>
        /// <param name="forceSet">Set even when the module is not permitted?</param>
        public void SetDirection(Vector2 direction, bool forceSet = false)
        {
            if (!ModulePermitted && !forceSet) return;
            moveDirection = direction;
            moveDirection.Normalize();
            if (characterHub.MovementState == CharacterMovementState.Dashing) return;
            var state = moveDirection.magnitude > 0
                ? CharacterMovementState.Walking
                : CharacterMovementState.Idle;
            characterHub.ChangeMovementState(state);
        }

        public void SetPosition(Vector2 position)
        {
            if (!ModulePermitted) return;
            rb2d.position = position;
        }
        
        public void Dash(Vector2 direction, float dashForce, LayerMask ignoreLayer = default, float duration = 0.1f)
        {
            if (!ModulePermitted) return;
            if (characterHub.MovementState == CharacterMovementState.Dashing) return;
            characterHub.ChangeMovementState(CharacterMovementState.Dashing);
            SetDirection(direction, true);
            dashColliderCoroutine = StartCoroutine(DashColliderCoroutine(ignoreLayer, duration));
            rb2d.AddForce(direction * dashForce, ForceMode2D.Impulse);
        }
        
        private IEnumerator DashColliderCoroutine(LayerMask ignoreLayer, float duration)
        {
            var excludeLayers = playerCollider.excludeLayers;
            playerCollider.excludeLayers = ignoreLayer;
            yield return new WaitForSeconds(duration);
            playerCollider.excludeLayers = excludeLayers;
            characterHub.ChangeMovementState(CharacterMovementState.Idle);
        }

        protected override void HandleInput()
        {
            if (characterHub.CharacterType is not CharacterType.Player) return;
            base.HandleInput();
            SetDirection(PlayerInput.MovementInput);
        }

        protected override void UpdateAnimator()
        {
            if (!IsOwner) return;
            base.UpdateAnimator();
            animator.SetBool(IsMoving, moveDirection.magnitude > 0);
        }
    }
}
