using System;
using System.Collections.Generic;
using System.Linq;
using DungeonTogether.Scripts.Character.Module;
using DungeonTogether.Scripts.Utils;
using TriInspector;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Serialization;

namespace DungeonTogether.Scripts.Character
{
    public enum CharacterType
    {
        Player,
        NPC
    }
    
    /// <summary>
    ///  Character hub is the main class that manages the character and its modules.
    /// </summary>
    [DeclareTabGroup("Debug Tab")]
    public class CharacterHub : NetworkBehaviour
    {
        [InfoBox("Character Hub is the main class that manages the character and its modules.")]
        #region Inspector
        [Title("References")] 
        [SerializeField, 
         PropertyTooltip("GameObject that contains the modules. Leave empty if the module is in the same object that this script is in.")]
        private GameObject moduleParent;
        
        [Title("Character Settings")] 
        [field: SerializeField, 
                PropertyTooltip("Character type, used to determine if this character is a player or an NPC.")]
        public CharacterType CharacterType { get; private set; }
        
        [Title("Debug")] 
        [SerializeField, DisplayAsString, HideLabel]
        private string debugSeparator = string.Empty; //Ignore this, it's just to separate the debug properties
        [SerializeField, ReadOnly, Group("Debug Tab"), Tab("Module"),
         PropertyTooltip("List of active modules in this character.")]
        private List<CharacterModule> modules = new();
        [field: SerializeField, DisplayAsString, GroupNext("Debug Tab"), Tab("State"),
                PropertyTooltip("Current movement state of the character.")] 
        public CharacterStates.CharacterMovementState MovementState { get; private set; } = CharacterStates.CharacterMovementState.Idle;
        [field: SerializeField, DisplayAsString, Tab("State"),
                PropertyTooltip("Current action state of the character.")]
        public CharacterStates.CharacterActionState ActionState { get; private set; } = CharacterStates.CharacterActionState.None;
        [field: SerializeField, DisplayAsString, Tab("State"),
                PropertyTooltip("Current condition state of the character.")] 
        public CharacterStates.CharacterConditionState ConditionState { get; private set; } = CharacterStates.CharacterConditionState.Normal;
        #endregion
        
        #region Properties
        public PlayerInput PlayerInput { get; private set; }
        protected bool initialized;
        public bool Initialized => initialized;
        #endregion

        #region Life Cycle
        public override void OnNetworkSpawn()
        {
            Initialize();
            Subscribe();
        }

        /// <summary>
        /// Initializes the character hub and its modules.
        /// </summary>
        protected virtual void Initialize()
        {
            //if (!IsOwner) return;
            if (!moduleParent)
            {
                moduleParent = gameObject;
            }
            var modules = moduleParent.GetComponentsInChildren<CharacterModule>();
            foreach (var module in modules)
            {
                module.Initialize(this);
                this.modules.Add(module);
            }
            PlayerInput = GetComponent<PlayerInput>();
            if (!PlayerInput && CharacterType == CharacterType.Player)
            {
                Debug.LogError($"{nameof(PlayerInput)} component not found in player object.");
            }
            initialized = true;
        }

        /// <summary>
        /// Subscribes to the events that the character hub needs to listen to.
        /// </summary>
        protected virtual void Subscribe()
        {
            if (!IsOwner) return;
        }
        
        /// <summary>
        /// Shuts down the character hub and its modules.
        /// </summary>
        protected virtual void Shutdown()
        {
            if (!IsOwner) return;
            foreach (var module in modules)
            {
                module.Shutdown();
            }
            initialized = false;
        }

        /// <summary>
        /// Unsubscribes from the events that the character hub was listening to.
        /// </summary>
        protected virtual void Unsubscribe()
        {
            if (!IsOwner) return;
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            Shutdown();
            Unsubscribe();
        }
        #endregion
        
        #region Event Handlers
        
        #endregion
        
        #region States
        public void ChangeMovementState(CharacterStates.CharacterMovementState newState)
        {
            if (newState == MovementState) return;
            CharacterStates.MovementStateEvent.Invoke(this, MovementState, newState);
            MovementState = newState;
        }
        
        public void ChangeActionState(CharacterStates.CharacterActionState newState)
        {
            if (newState == ActionState) return;
            CharacterStates.ActionStateEvent.Invoke(this, ActionState, newState);
            ActionState = newState;
        }
        
        public void ChangeConditionState(CharacterStates.CharacterConditionState newState)
        {
            if (newState == ConditionState) return;
            CharacterStates.ConditionStateEvent.Invoke(this, ConditionState, newState);
            ConditionState = newState;
        }
        #endregion

        #region Utility
        /// <summary>
        /// Finds a module of the specified type in the character hub.
        /// </summary>
        /// <typeparam name="T">Type of the module to find.</typeparam>
        /// <returns>The module of the specified type if found, null otherwise.</returns>
        public T FindModuleOfType<T>() where T : CharacterModule
        {
            return modules.Where(module => module is T).Cast<T>().FirstOrDefault();
        }
        #endregion
    }
}
