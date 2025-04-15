using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DungeonTogether.Scripts.Character.Module;
using DungeonTogether.Scripts.Manangers;
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
        public CharacterMovementState MovementState { get; private set; } = CharacterMovementState.Idle;
        [field: SerializeField, DisplayAsString, Tab("State"),
                PropertyTooltip("Current action state of the character.")]
        public CharacterActionState ActionState { get; private set; } = CharacterActionState.None;
        [field: SerializeField, DisplayAsString, Tab("State"),
                PropertyTooltip("Current condition state of the character.")] 
        public CharacterConditionState ConditionState { get; private set; } = CharacterConditionState.Normal;
        #endregion
        
        #region Properties
        public PlayerInput PlayerInput { get; private set; }
        public CharacterPool CharacterPool { get; set; }
        protected bool initialized;
        protected bool shutdown;
        public bool Initialized => initialized;
        private Coroutine changeActionStateCoroutine;
        private Coroutine changeConditionStateCoroutine;
        private Coroutine changeMovementStateCoroutine;
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
            ChangeActionState(CharacterActionState.None);
            ChangeConditionState(CharacterConditionState.Normal);
            ChangeMovementState(CharacterMovementState.Idle);
            initialized = true;
            shutdown = false;
            foreach (var module in modules)
            {
                module.PostInitialize();
            }
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
            if (changeMovementStateCoroutine != null)
            {
                StopCoroutine(changeMovementStateCoroutine);
            }
            if (changeMovementStateCoroutine != null)
            {
                StopCoroutine(changeMovementStateCoroutine);
            }
            if (changeMovementStateCoroutine != null)
            {
                StopCoroutine(changeMovementStateCoroutine);
            }
            initialized = false;
        }

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();
            ShutdownProcedure();
        }
        
        public override void OnDestroy()
        {
            base.OnDestroy();
            ShutdownProcedure();
        }
        
        /// <summary>
        /// Make the character hub shutdown instantly.
        /// </summary>
        public void ShutdownInstant()
        {
            ShutdownProcedure();
        }

        /// <summary>
        /// Shuts down the character hub and its modules and returns the character to the pool if it has one.
        /// </summary>
        private void ShutdownProcedure()
        {
            if (shutdown) return;
            shutdown = true;
            Shutdown();
            Unsubscribe();
            if (CharacterPool)
            {
                CharacterPool.BackToPool(this);
                CharacterPool = null;
            }
            else
            {
                DespawnRpc();
            }
        }

        [Rpc(SendTo.Server)]
        private void DespawnRpc()
        {
            NetworkObject.Despawn(true);
        }
        #endregion
        
        #region Event Handlers
        /// <summary>
        /// Subscribes to the events that the character hub needs to listen to.
        /// </summary>
        protected virtual void Subscribe()
        {
            //if (!IsOwner) return;
            ClassSelector.OnPreCharacterSpawned += OnPreCharacterSpawned;
        }
        
        /// <summary>
        /// Unsubscribes from the events that the character hub was listening to.
        /// </summary>
        protected virtual void Unsubscribe()
        {
            //if (!IsOwner) return;
            ClassSelector.OnPreCharacterSpawned -= OnPreCharacterSpawned;
        }

        /// <summary>
        /// Shutdown this player character when the new player character is spawned.
        /// </summary>
        /// <param name="id"></param>
        private void OnPreCharacterSpawned(ulong id)
        {
            if (id != NetworkObject.OwnerClientId) return;
            ShutdownProcedure();
        }
        #endregion
        
        #region States
        public void ChangeMovementState(CharacterMovementState newState)
        {
            if (newState == MovementState) return;
            MovementStateEvent.Invoke(this, MovementState, newState);
            MovementState = newState;
        }
        
        public void ChangeMovementState(CharacterMovementState newState, float duration)
        {
            if (changeMovementStateCoroutine != null)
            {
                StopCoroutine(changeMovementStateCoroutine);
            }
            changeMovementStateCoroutine = StartCoroutine(ChangeMovementStateCoroutine(MovementState, newState, duration));
        }

        public void ChangeActionState(CharacterActionState newState)
        {
            if (newState == ActionState) return;
            ActionStateEvent.Invoke(this, ActionState, newState);
            ActionState = newState;
        }
        
        public void ChangeActionState(CharacterActionState newState, float duration)
        {
            if (changeActionStateCoroutine != null)
            {
                StopCoroutine(changeActionStateCoroutine);
            }
            changeActionStateCoroutine = StartCoroutine(ChangeActionStateCoroutine(ActionState, newState, duration));
        }
        
        public void ChangeConditionState(CharacterConditionState newState)
        {
            if (newState == ConditionState) return;
            ConditionStateEvent.Invoke(this, ConditionState, newState);
            ConditionState = newState;
        }
        
        public void ChangeConditionState(CharacterConditionState newState, float duration)
        {
            if (changeConditionStateCoroutine != null)
            {
                StopCoroutine(changeConditionStateCoroutine);
            }
            changeConditionStateCoroutine = StartCoroutine(ChangeConditionStateCoroutine(ConditionState, newState, duration));
        }
        
        private IEnumerator ChangeMovementStateCoroutine(CharacterMovementState oldState, CharacterMovementState newState, float duration)
        {
            ChangeMovementState(newState);
            yield return new WaitForSeconds(duration);
            ChangeMovementState(oldState);
        }
        
        private IEnumerator ChangeActionStateCoroutine(CharacterActionState oldState, CharacterActionState newState, float duration)
        {
            ChangeActionState(newState);
            yield return new WaitForSeconds(duration);
            ChangeActionState(oldState);
        }
        
        private IEnumerator ChangeConditionStateCoroutine(CharacterConditionState oldState, CharacterConditionState newState, float duration)
        {
            ChangeConditionState(newState);
            yield return new WaitForSeconds(duration);
            ChangeConditionState(oldState);
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
