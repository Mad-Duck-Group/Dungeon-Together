using Unity.Netcode;
using UnityEngine;

namespace DungeonTogether.Scripts.Character.Module
{
    public abstract class CharacterModule : NetworkBehaviour
    {
        [SerializeField] protected bool moduleEnabled = true;
        protected CharacterHub characterHub;
        protected PlayerInput PlayerInput => characterHub.PlayerInput;

        protected virtual void Update()
        {
            if (!IsOwner) return;
            if (!moduleEnabled) return;
            HandleInput();
            UpdateModule();
        }
        
        protected virtual void HandleInput()
        {
        
        }
        
        protected virtual void UpdateModule()
        {
            
        }
        
        public virtual void Initialize(CharacterHub characterHub)
        {
            this.characterHub = characterHub;
        }

        public virtual void Shutdown()
        {
        
        }
    }
}
