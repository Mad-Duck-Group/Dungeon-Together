using System;
using System.Collections.Generic;
using TriInspector;
using Unity.Netcode;
using UnityEngine;

namespace DungeonTogether.Scripts.Character.Module
{
    /// <summary>
    /// Inheritable character module class
    /// </summary>
    public abstract class CharacterModule : NetworkBehaviour
    {
        #region Inspector
        [Title("Settings")]
        [SerializeField, PropertyOrder(0),
        PropertyTooltip("Is this module enabled?")] 
        protected bool moduleEnabled = true;
        [SerializeField, PropertyOrder(0),
        PropertyTooltip("List of movement states that block this module")] 
        protected List<CharacterMovementState> blockedMovementStates = new();
        [SerializeField, PropertyOrder(0),
        PropertyTooltip("List of action states that block this module")] 
        protected List<CharacterActionState> blockedActionStates = new();
        [SerializeField, PropertyOrder(0),
        PropertyTooltip("List of condition states that block this module")] 
        protected List<CharacterConditionState> blockedConditionStates = new();
        
        [Title("Debug")]
        [ShowInInspector, DisplayAsString, PropertyOrder(0),
         PropertyTooltip("Is this module permitted? (Module is permitted if it is enabled and the character is not in a blocked state)")]
        protected bool ModulePermitted
        {
            get
            {
                if (!characterHub) return false;
                return characterHub.Initialized &&
                       !blockedMovementStates.Contains(characterHub.MovementState) &&
                       !blockedActionStates.Contains(characterHub.ActionState) &&
                       !blockedConditionStates.Contains(characterHub.ConditionState) &&
                       moduleEnabled;
            }
        }

        #endregion

        #region Properties
        protected CharacterHub characterHub;
        protected PlayerInput PlayerInput => characterHub.PlayerInput;
        #endregion

        #region Unity Methods
        /// <summary>
        /// Unity update method. Note: It is not recommended to override this method. Use UpdateModule instead.
        /// </summary>
        protected virtual void Update()
        {
            if (!IsOwner) return;
            if (!ModulePermitted) return;
            HandleInput();
            UpdateModule();
        }

        /// <summary>
        /// Unity fixed update method. Note: It is not recommended to override this method. Use FixedUpdateModule instead.
        /// </summary>
        protected virtual void FixedUpdate()
        {
            if (!IsOwner) return;
            if (!ModulePermitted) return;
            FixedUpdateModule();
        }

        /// <summary>
        /// Unity late update method. Note: It is not recommended to override this method. Use LateUpdateModule instead.
        /// </summary>
        protected virtual void LateUpdate()
        {
            if (!IsOwner) return;
            if (!ModulePermitted) return;
            LateUpdateModule();
        }
        #endregion

        #region Life Cycle
        /// <summary>
        /// Initialize module
        /// </summary>
        /// <param name="characterHub">Owner character hub</param>
        public virtual void Initialize(CharacterHub characterHub)
        {
            Debug.Log($"Initializing {GetType().Name} module for {characterHub.name}");
            this.characterHub = characterHub;
            Subscribe();
        }
        
        /// <summary>
        /// Post initialize module, useful for getting modules from the character hub
        /// </summary>
        public virtual void PostInitialize()
        {
            
        }

        /// <summary>
        /// Shutdown module
        /// </summary>
        public virtual void Shutdown()
        {
            Unsubscribe();
        }
        
        protected virtual void Subscribe()
        {
        
        }
        
        protected virtual void Unsubscribe()
        {
        
        }
        #endregion

        #region Input
        /// <summary>
        /// Handle player input
        /// </summary>
        protected virtual void HandleInput()
        {
        
        }
        #endregion

        #region Update Module
        /// <summary>
        /// Update module using standard update rate
        /// </summary>
        protected virtual void UpdateModule()
        {
            
        }
        
        /// <summary>
        /// Update module using fixed update rate
        /// </summary>
        protected virtual void FixedUpdateModule()
        {
            
        }
        
        /// <summary>
        /// Update module using late update rate
        /// </summary>
        protected virtual void LateUpdateModule()
        {
            UpdateAnimator();
        }

        /// <summary>
        /// Update animator
        /// </summary>
        protected virtual void UpdateAnimator()
        {
            
        }
        #endregion
    }
}
