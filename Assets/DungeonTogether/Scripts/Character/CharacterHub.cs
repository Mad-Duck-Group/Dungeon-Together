using System.Collections.Generic;
using System.Linq;
using DungeonTogether.Scripts.Character.Module;
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
    public class CharacterHub : NetworkBehaviour
    {
        [Header("References")] 
        [SerializeField, 
         Tooltip("GameObject that contains the modules. Leave empty if the module is in the same object that this script is in.")]
        private GameObject moduleParent;
        [field: SerializeField] public CharacterType CharacterType { get; private set; }

        [SerializeField, ReadOnly] private List<CharacterModule> modules = new();
        private PlayerInput _playerInput;
        public PlayerInput PlayerInput => _playerInput;

        public override void OnNetworkSpawn()
        {
            Initialize();
        }
        protected virtual void Start()
        {
            //Initialize();
        }

        protected virtual void Initialize()
        {
            if (!IsOwner) return;
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
            _playerInput = GetComponent<PlayerInput>();
            if (!_playerInput && CharacterType == CharacterType.Player)
            {
                Debug.LogError($"{nameof(PlayerInput)} component not found in player object.");
            }
        }
        
        protected virtual void Shutdown()
        {
            if (!IsOwner) return;
            foreach (var module in modules)
            {
                module.Shutdown();
            }
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            Shutdown();
        }

        public T FindModuleOfType<T>() where T : CharacterModule
        {
            return modules.Where(module => module is T).Cast<T>().FirstOrDefault();
        }
    }
}
